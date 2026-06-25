using ahello_backend.DTO.Payment;
using ahello_backend.Models.Payment;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EscrowPaymentController : ControllerBase
    {
        private readonly IEscrowPaymentRepository _escrowRepo;
        private readonly IEscrowPaymentLogRepository _logRepo;
        private readonly IRazorpayService _razorpayService;

        public EscrowPaymentController(
            IEscrowPaymentRepository escrowRepo,
            IEscrowPaymentLogRepository logRepo,
            IRazorpayService razorpayService)
        {
            _escrowRepo = escrowRepo;
            _logRepo = logRepo;
            _razorpayService = razorpayService;
        }

        private IActionResult Error(string message, int statusCode = 400, object? details = null)
        {
            return StatusCode(statusCode, new
            {
                success = false,
                message,
                details
            });
        }

        private IActionResult Success(string message, object? data = null)
        {
            return Ok(new
            {
                success = true,
                message,
                data
            });
        }

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            if (dto.BookingId <= 0)
                return Error("Valid BookingId is required");

            var existingPayment = await _escrowRepo.GetByBookingIdAsync(dto.BookingId);

            if (existingPayment != null &&
                !string.IsNullOrWhiteSpace(existingPayment.RazorpayOrderId))
            {
                return Success("Order already created", new
                {
                    escrowPaymentId = existingPayment.EscrowPaymentId,
                    orderId = existingPayment.RazorpayOrderId,
                    amount = existingPayment.TotalAmount,
                    currency = existingPayment.Currency,
                    status = existingPayment.Status
                });
            }

            var bookingInfo = await _escrowRepo.GetBookingPaymentInfoAsync(dto.BookingId);

            if (bookingInfo == null)
                return Error("Booking not found", 404);

            decimal totalAmount = Convert.ToDecimal(bookingInfo.Price);

            if (totalAmount <= 0)
                return Error("Invalid service price");

            decimal platformFee = Math.Round(totalAmount * 0.10m, 2);
            decimal expertAmount = totalAmount - platformFee;

            string receipt = $"booking_{dto.BookingId}";

            try
            {
                var orderResult = await _razorpayService.CreateOrderAsync(
                    totalAmount,
                    "INR",
                    receipt);

                var payment = new EscrowPayment
                {
                    BookingId = Convert.ToInt32(bookingInfo.BookingId),
                    UserId = Convert.ToInt32(bookingInfo.UserId),
                    ClientId = Convert.ToInt32(bookingInfo.ClientId),

                    // Route not enabled now, so keep placeholder
                    RazorpayAccountId = "ROUTE_NOT_ENABLED",

                    RazorpayOrderId = orderResult.orderId,

                    TotalAmount = totalAmount,
                    PlatformFee = platformFee,
                    ExpertAmount = expertAmount,
                    Currency = "INR",
                    Status = "CREATED",
                    OrderResponseJson = orderResult.responseJson,
                    CreatedBy = Convert.ToString(bookingInfo.ClientId)
                };

                int escrowPaymentId = await _escrowRepo.InsertAsync(payment);

                await _logRepo.InsertAsync(
                    escrowPaymentId,
                    dto.BookingId,
                    "CREATE_ORDER",
                    "SUCCESS",
                    requestJson: $"BookingId: {dto.BookingId}",
                    responseJson: orderResult.responseJson,
                    createdBy: Convert.ToString(bookingInfo.ClientId));

                return Success("Order created successfully", new
                {
                    escrowPaymentId,
                    orderId = orderResult.orderId,
                    amount = totalAmount,
                    amountInPaise = totalAmount * 100,
                    currency = "INR",
                    bookingId = dto.BookingId
                });
            }
            catch (Exception ex)
            {
                await _logRepo.InsertAsync(
                    null,
                    dto.BookingId,
                    "CREATE_ORDER",
                    "ERROR",
                    requestJson: $"BookingId: {dto.BookingId}",
                    errorMessage: ex.Message);

                return Error("Failed to create Razorpay order", 500, new
                {
                    razorpayError = ex.Message
                });
            }
        }
        [HttpPost("verify-payment")]
        public async Task<IActionResult> VerifyPayment([FromBody] VerifyAndHoldDto dto)
        {
            if (dto.BookingId <= 0)
                return Error("Valid BookingId is required");

            if (string.IsNullOrWhiteSpace(dto.RazorpayOrderId))
                return Error("RazorpayOrderId is required");

            if (string.IsNullOrWhiteSpace(dto.RazorpayPaymentId))
                return Error("RazorpayPaymentId is required");

            if (string.IsNullOrWhiteSpace(dto.RazorpaySignature))
                return Error("RazorpaySignature is required");

            var escrow = await _escrowRepo.GetByBookingIdAsync(dto.BookingId);

            if (escrow == null)
                return Error("Escrow payment record not found", 404);

            if (escrow.Status == "PAYMENT_VERIFIED")
            {
                return Success("Payment already verified", new
                {
                    escrowPaymentId = escrow.EscrowPaymentId,
                    bookingId = escrow.BookingId,
                    paymentId = escrow.RazorpayPaymentId,
                    status = escrow.Status
                });
            }

            if (escrow.Status != "CREATED")
            {
                return Error("Invalid payment status", 409, new
                {
                    currentStatus = escrow.Status,
                    requiredStatus = "CREATED"
                });
            }

            if (escrow.RazorpayOrderId != dto.RazorpayOrderId)
            {
                return Error("Razorpay order id mismatch", 400, new
                {
                    savedOrderId = escrow.RazorpayOrderId,
                    receivedOrderId = dto.RazorpayOrderId
                });
            }

            bool isValid = _razorpayService.VerifySignature(
                dto.RazorpayOrderId,
                dto.RazorpayPaymentId,
                dto.RazorpaySignature);

            if (!isValid)
            {
                await _escrowRepo.UpdateStatusAsync(
                    escrow.EscrowPaymentId,
                    "PAYMENT_FAILED",
                    "Invalid Razorpay payment signature");

                await _logRepo.InsertAsync(
                    escrow.EscrowPaymentId,
                    dto.BookingId,
                    "VERIFY_PAYMENT",
                    "FAILED",
                    requestJson: System.Text.Json.JsonSerializer.Serialize(dto),
                    errorMessage: "Invalid Razorpay payment signature",
                    createdBy: escrow.ClientId.ToString());

                return Error("Invalid Razorpay payment signature", 400);
            }

            var verifyJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                dto.BookingId,
                dto.RazorpayOrderId,
                dto.RazorpayPaymentId,
                dto.RazorpaySignature,
                VerifiedAt = DateTime.Now
            });

            await _escrowRepo.UpdatePaymentVerifiedAsync(
                escrow.EscrowPaymentId,
                dto.RazorpayPaymentId,
                dto.RazorpaySignature,
                verifyJson);

            await _logRepo.InsertAsync(
                escrow.EscrowPaymentId,
                dto.BookingId,
                "VERIFY_PAYMENT",
                "SUCCESS",
                requestJson: System.Text.Json.JsonSerializer.Serialize(dto),
                responseJson: verifyJson,
                createdBy: escrow.ClientId.ToString());

            return Success("Payment verified successfully", new
            {
                escrowPaymentId = escrow.EscrowPaymentId,
                bookingId = escrow.BookingId,
                orderId = dto.RazorpayOrderId,
                paymentId = dto.RazorpayPaymentId,
                status = "PAYMENT_VERIFIED"
            });
        }
    }
}