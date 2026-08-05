using ahello_backend.DTO.Payment;
using ahello_backend.Models.Bookings;
using ahello_backend.Models.Payment;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Classes;
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
        private readonly IBookingRepository _bookingRepo;
        private readonly IExpertPayoutRepository _expertPayoutRepo;
        private readonly IEscrowPaymentService _escrowPaymentService;
        private readonly IPlatformSettingsRepository _platformSettingsRepo;
        private readonly IInvoiceService _invoiceService;
        private readonly IEmailRepository _emailRepository;

        public EscrowPaymentController(
            IEscrowPaymentRepository escrowRepo,
            IEscrowPaymentLogRepository logRepo,
            IRazorpayService razorpayService,
            IBookingRepository bookingRepo,
            IExpertPayoutRepository expertPayoutRepo,
            IEscrowPaymentService escrowPaymentService,
            IPlatformSettingsRepository platformSettingsRepo,
            IInvoiceService invoiceService,
            IEmailRepository emailRepository)
        {
            _escrowRepo = escrowRepo;
            _logRepo = logRepo;
            _razorpayService = razorpayService;
            _bookingRepo = bookingRepo;
            _expertPayoutRepo = expertPayoutRepo;
            _escrowPaymentService = escrowPaymentService;
            _platformSettingsRepo = platformSettingsRepo;
            _invoiceService = invoiceService;
            _emailRepository = emailRepository;
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
            if (dto.UserId <= 0 || dto.ServiceId <= 0 || dto.SlotId <= 0)
                return Error("Valid booking details are required");

            var servicePrice = await _escrowRepo.GetServicePriceAsync(dto.ServiceId);
            if (servicePrice == null || servicePrice <= 0)
                return Error("Invalid service");
            var serviceName = await _escrowRepo.GetServiceNameAsync(dto.ServiceId);

            var platformSetting = await _platformSettingsRepo.GetActiveSettingAsync();
            if (platformSetting == null)
                return Error("Platform fee configuration is not available. Contact support.", 500);

            decimal platformFee;
            if (platformSetting.FeeType.Equals("percentage", StringComparison.OrdinalIgnoreCase))
            {
                if (!platformSetting.FeePercentage.HasValue)
                    return Error("Platform fee percentage is not configured.", 500);

                platformFee = Math.Round(servicePrice.Value * platformSetting.FeePercentage.Value / 100m, 2);
            }
            else if (platformSetting.FeeType.Equals("amount", StringComparison.OrdinalIgnoreCase))
            {
                if (!platformSetting.FeeAmount.HasValue)
                    return Error("Platform fee amount is not configured.", 500);

                platformFee = Math.Round(platformSetting.FeeAmount.Value, 2);
            }
            else
            {
                return Error("Invalid platform fee configuration.", 500);
            }

            if (platformFee > servicePrice.Value)
                return Error("Platform fee cannot be greater than the service price.", 400);

            var taxAmount = Math.Round(platformFee * platformSetting.GstRate, 2);
            var grandTotal = servicePrice.Value + taxAmount;

            string receipt = $"slot_{dto.SlotId}_{DateTime.UtcNow:yyyyMMddHHmmss}";

            try
            {
                var orderResult = await _razorpayService.CreateOrderAsync(
                    grandTotal, "INR", receipt, serviceName ?? "");

                await _logRepo.InsertAsync(
                    null, null, "CREATE_ORDER", "SUCCESS",
                    requestJson: System.Text.Json.JsonSerializer.Serialize(dto),
                    responseJson: orderResult.responseJson,
                    createdBy: dto.CreatedBy);

                return Success("Order created successfully", new
                {
                    orderId = orderResult.orderId,
                    servicePrice = servicePrice.Value,
                    platformFee,
                    taxAmount,
                    taxLabel = "GST",
                    amount = grandTotal,
                    amountInPaise = grandTotal * 100,
                    currency = "INR"
                });
            }
            catch (Exception ex)
            {
                await _logRepo.InsertAsync(
                    null, null, "CREATE_ORDER", "ERROR",
                    requestJson: System.Text.Json.JsonSerializer.Serialize(dto),
                    errorMessage: ex.Message);

                return Error("Failed to create Razorpay order", 500, new { razorpayError = ex.Message });
            }
        }

        [HttpPost("verify-payment")]
        public async Task<IActionResult> VerifyPayment([FromBody] VerifyAndHoldDto dto)
        {
            var bp = dto.BookingPayload;

            if (bp.UserId <= 0 || bp.ServiceId <= 0 || bp.SlotId <= 0)
                return Error("Valid booking details are required");

            if (string.IsNullOrWhiteSpace(dto.RazorpayOrderId))
                return Error("RazorpayOrderId is required");

            if (string.IsNullOrWhiteSpace(dto.RazorpayPaymentId))
                return Error("RazorpayPaymentId is required");

            if (string.IsNullOrWhiteSpace(dto.RazorpaySignature))
                return Error("RazorpaySignature is required");

            bool isValid = _razorpayService.VerifySignature(
                dto.RazorpayOrderId, dto.RazorpayPaymentId, dto.RazorpaySignature);

            if (!isValid)
            {
                await _logRepo.InsertAsync(
                    null, null, "VERIFY_PAYMENT", "FAILED",
                    requestJson: System.Text.Json.JsonSerializer.Serialize(dto),
                    errorMessage: "Invalid Razorpay payment signature",
                    createdBy: bp.CreatedBy);

                return Error("Invalid Razorpay payment signature", 400);
            }

            try
            {
                // Re-fetch authoritative price server-side — never trust client TotalAmount
                var servicePrice = await _escrowRepo.GetServicePriceAsync(bp.ServiceId);
                if (servicePrice == null || servicePrice <= 0)
                {
                    await _logRepo.InsertAsync(
                        null, null, "VERIFY_PAYMENT", "ERROR",
                        requestJson: System.Text.Json.JsonSerializer.Serialize(dto),
                        errorMessage: $"Invalid ServiceId {bp.ServiceId} at verify time",
                        createdBy: bp.CreatedBy);

                    return Error("Invalid service", 400);
                }

                // 1. Insert booking NOW — payment confirmed
                var bookingId = await _bookingRepo.CreateAsync(new BookingPost
                {
                    UserId = bp.UserId,
                    ClientId = bp.ClientId,
                    ServiceId = bp.ServiceId,
                    SlotId = bp.SlotId,
                    ScheduleDate = bp.ScheduleDate,
                    StartTime = bp.StartTime,
                    EndTime = bp.EndTime,
                    Status = bp.Status,
                    CreatedBy = bp.CreatedBy
                });

                // Fetch expert's real Razorpay linked account (bp.UserId = expert/host)
                var expertAccount = await _expertPayoutRepo.GetByUserIdAsync(bp.UserId);

                if (expertAccount == null || expertAccount.AccountStatus != "ACTIVE" ||
                    string.IsNullOrWhiteSpace(expertAccount.RazorpayAccountId))
                {
                    await _logRepo.InsertAsync(
                        null, bookingId, "VERIFY_PAYMENT", "ERROR",
                        requestJson: System.Text.Json.JsonSerializer.Serialize(dto),
                        errorMessage: $"Expert {bp.UserId} payout account not ACTIVE (status: {expertAccount?.AccountStatus ?? "NOT_CREATED"}). Payment captured, escrow blocked pending manual reconciliation.",
                        createdBy: bp.CreatedBy);

                    return Error(
                        $"Payment captured but expert's payout account isn't active yet. Contact support with PaymentId {dto.RazorpayPaymentId}.",
                        500,
                        new { expertStatus = expertAccount?.AccountStatus ?? "NOT_CREATED" });
                }

                var platformSetting = await _platformSettingsRepo.GetActiveSettingAsync();

                if (platformSetting == null)
                {
                    await _logRepo.InsertAsync(
                        null,
                        bookingId,
                        "VERIFY_PAYMENT",
                        "ERROR",
                        requestJson: System.Text.Json.JsonSerializer.Serialize(dto),
                        errorMessage: "No active platform fee setting found.",
                        createdBy: bp.CreatedBy);

                    return Error(
                        "Platform fee configuration is not available. Contact support.",
                        500);
                }

                decimal platformFee;

                if (platformSetting.FeeType.Equals(
                        "percentage",
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (!platformSetting.FeePercentage.HasValue)
                        return Error("Platform fee percentage is not configured.", 500);

                    platformFee = Math.Round(
                        servicePrice.Value * platformSetting.FeePercentage.Value / 100m,
                        2);
                }
                else if (platformSetting.FeeType.Equals(
                             "amount",
                             StringComparison.OrdinalIgnoreCase))
                {
                    if (!platformSetting.FeeAmount.HasValue)
                        return Error("Platform fee amount is not configured.", 500);

                    platformFee = Math.Round(
                        platformSetting.FeeAmount.Value,
                        2);
                }
                else
                {
                    return Error("Invalid platform fee configuration.", 500);
                }

                if (platformFee > servicePrice.Value)
                {
                    return Error(
                        "Platform fee cannot be greater than the service price.",
                        400);
                }

                var taxAmount = Math.Round(platformFee * platformSetting.GstRate, 2);
                var grandTotal = servicePrice.Value + taxAmount;

                decimal expertAmount = servicePrice.Value - platformFee;

                // Create held transfer to expert's linked account now that
                // payment is verified and expert account is ACTIVE
                string transferId;
                string transferJson;
                try
                {
                    (transferId, transferJson) = await _razorpayService.CreateHeldTransferAsync(
                        dto.RazorpayPaymentId,
                        expertAccount.RazorpayAccountId,
                        expertAmount);
                }
                catch (Exception ex)
                {
                    // Payment is verified but transfer failed — needs reconciliation,
                    // do not lose the payment record.
                    await _logRepo.InsertAsync(
                        null, bookingId, "CREATE_HELD_TRANSFER", "ERROR",
                        requestJson: System.Text.Json.JsonSerializer.Serialize(new { bookingId, dto.RazorpayPaymentId, expertAccount.RazorpayAccountId, expertAmount }),
                        errorMessage: ex.Message,
                        createdBy: bp.CreatedBy);

                    return Error(
                        $"Payment verified but transfer to expert failed. Contact support with PaymentId {dto.RazorpayPaymentId}.",
                        500,
                        new { razorpayError = ex.Message });
                }

                var now = DateTime.Now;

                var verifyJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    bookingId,
                    dto.RazorpayOrderId,
                    dto.RazorpayPaymentId,
                    dto.RazorpaySignature,
                    VerifiedAt = now
                });

                // 2. Insert escrow row referencing the new bookingId
                var payment = new EscrowPayment
                {
                    BookingId = bookingId,
                    UserId = bp.UserId,
                    ClientId = bp.ClientId,
                    RazorpayAccountId = expertAccount.RazorpayAccountId,
                    RazorpayOrderId = dto.RazorpayOrderId,
                    RazorpayPaymentId = dto.RazorpayPaymentId,
                    RazorpaySignature = dto.RazorpaySignature,
                    RazorpayTransferId = transferId,
                    TotalAmount = grandTotal,          // ← was servicePrice.Value
                    PlatformFee = platformFee,
                    TaxAmount = taxAmount,             // ← add this
                    TaxRate = platformSetting.GstRate, // ← add this
                    ExpertAmount = expertAmount,
                    Currency = "INR",
                    Status = "HELD",
                    VerifyResponseJson = verifyJson,
                    TransferResponseJson = transferJson,
                    PaidAt = now,
                    HeldAt = now,
                    CreatedBy = bp.CreatedBy
                };

                int escrowPaymentId = await _escrowRepo.InsertAsync(payment);
                payment.EscrowPaymentId = escrowPaymentId;

                try
                {
                    var bookingRead = await _bookingRepo.GetByIdAsync(bookingId);
                    await _invoiceService.CreateInvoiceFromVerifiedPaymentAsync(payment, bookingRead, bp.CreatedBy);
                    var invoicePdf = await _invoiceService.DownloadInvoicePdfAsync(bookingId);
                    var invoice = await _invoiceService.GetInvoiceByBookingIdAsync(bookingId);

                    // Send Booking Confirmation Email with Invoice PDF
                    await _emailRepository.SendBookingConfirmationEmailAsync(
                        bookingRead.ClientEmail,
                        bookingRead.ClientName,
                        bookingRead.ServiceTitle,
                        bookingRead.ScheduleDate.ToString("dddd, MMMM dd yyyy"),
                        bookingRead.StartTime.ToString(@"hh\:mm"),
                        invoicePdf);
                }
                catch (Exception ex)
                {
                    // Don't fail the payment response if invoice creation has an issue —
                    // payment + escrow are already committed. Log it for follow-up instead.
                    await _logRepo.InsertAsync(
                        escrowPaymentId, bookingId, "CREATE_INVOICE", "ERROR",
                        errorMessage: ex.Message, createdBy: bp.CreatedBy);
                }
                await _logRepo.InsertAsync(
                    escrowPaymentId, bookingId, "VERIFY_PAYMENT", "SUCCESS",
                    requestJson: System.Text.Json.JsonSerializer.Serialize(dto),
                    responseJson: verifyJson,
                    createdBy: bp.CreatedBy);

                await _logRepo.InsertAsync(
                    escrowPaymentId, bookingId, "CREATE_HELD_TRANSFER", "SUCCESS",
                    responseJson: transferJson,
                    createdBy: bp.CreatedBy);

                return Success("Payment verified, booking created, transfer held", new
                {
                    escrowPaymentId,
                    bookingId,
                    orderId = dto.RazorpayOrderId,
                    paymentId = dto.RazorpayPaymentId,
                    transferId,
                    status = "HELD"
                });
            }
            catch (Exception ex)
            {
                // Razorpay already captured the payment at this point — this needs
                // manual reconciliation, not a silent failure.
                await _logRepo.InsertAsync(
                    null, null, "VERIFY_PAYMENT", "ERROR",
                    requestJson: System.Text.Json.JsonSerializer.Serialize(dto),
                    errorMessage: ex.Message,
                    createdBy: bp.CreatedBy);

                return Error(
                    $"Payment captured but booking/escrow creation failed. Contact support with PaymentId {dto.RazorpayPaymentId}.",
                    500,
                    new { razorpayError = ex.Message });
            }
        }

        // POST /api/EscrowPayment/release
        [HttpPost("release")]
        public async Task<IActionResult> Release([FromBody] ReleaseDto dto)
        {
            if (dto.BookingId <= 0)
                return Error("Valid BookingId is required");

            var escrow = await _escrowRepo.GetByBookingIdAsync(dto.BookingId);
            if (escrow == null)
                return Error("Escrow payment record not found", 404);

            if (escrow.Status == "RELEASED")
                return Success("Transfer already released", new { status = "RELEASED" });

            if (escrow.Status != "HELD")
                return Error("Payment not in HELD status", 409, new { currentStatus = escrow.Status });

            try
            {
                var releaseJson = await _razorpayService.ReleaseTransferAsync(escrow.RazorpayTransferId);

                await _escrowRepo.UpdateReleaseAsync(escrow.EscrowPaymentId, releaseJson);
                await _logRepo.InsertAsync(escrow.EscrowPaymentId, dto.BookingId,
                    "RELEASE_TRANSFER", "SUCCESS", responseJson: releaseJson);

                return Success("Transfer released successfully", new { status = "RELEASED" });
            }
            catch (Exception ex)
            {
                await _logRepo.InsertAsync(escrow.EscrowPaymentId, dto.BookingId,
                    "RELEASE_TRANSFER", "ERROR", errorMessage: ex.Message);
                return Error("Failed to release transfer", 500, new { razorpayError = ex.Message });
            }
        }

        // POST /api/EscrowPayment/refund
        [HttpPost("refund")]
        public async Task<IActionResult> Refund([FromBody] RefundDto dto)
        {
            if (dto.BookingId <= 0)
                return Error("Valid BookingId is required");

            var escrow = await _escrowRepo.GetByBookingIdAsync(dto.BookingId);
            if (escrow == null)
                return Error("Escrow payment record not found", 404);

            if (escrow.Status == "REFUNDED")
                return Success("Already refunded", new { status = "REFUNDED" });

            // Reverse transfer first if it exists
            if (!string.IsNullOrEmpty(escrow.RazorpayTransferId))
            {
                try
                {
                    var reversalJson = await _razorpayService.ReverseTransferAsync(
                        escrow.RazorpayTransferId, escrow.ExpertAmount);
                    await _logRepo.InsertAsync(escrow.EscrowPaymentId, dto.BookingId,
                        "REVERSE_TRANSFER", "SUCCESS", responseJson: reversalJson);
                }
                catch (Exception ex)
                {
                    await _logRepo.InsertAsync(escrow.EscrowPaymentId, dto.BookingId,
                        "REVERSE_TRANSFER", "ERROR", errorMessage: ex.Message);
                    return Error("Failed to reverse transfer before refund", 500,
                        new { razorpayError = ex.Message });
                }
            }

            try
            {
                var refundJson = await _razorpayService.RefundPaymentAsync(
                    escrow.RazorpayPaymentId, escrow.TotalAmount);

                await _escrowRepo.UpdateRefundAsync(escrow.EscrowPaymentId, refundJson);
                await _logRepo.InsertAsync(escrow.EscrowPaymentId, dto.BookingId,
                    "REFUND_PAYMENT", "SUCCESS", responseJson: refundJson);

                return Success("Refund processed successfully", new { status = "REFUNDED" });
            }
            catch (Exception ex)
            {
                await _logRepo.InsertAsync(escrow.EscrowPaymentId, dto.BookingId,
                    "REFUND_PAYMENT", "ERROR", errorMessage: ex.Message);
                return Error("Failed to process refund", 500, new { razorpayError = ex.Message });
            }
        }

        // GET /api/EscrowPayment/booking/{bookingId}
        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetByBooking(int bookingId)
        {
            var escrow = await _escrowRepo.GetByBookingIdAsync(bookingId);
            if (escrow == null) return Error("Not found", 404);
            return Success("Escrow payment fetched", escrow);
        }

        [HttpGet("user/{userId}/timeline")]
        public async Task<IActionResult> GetTimelineByUserId(int userId,
        [FromQuery] string? search = null,
        [FromQuery] string? key = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            if (userId <= 0)
                return Error("Valid UserId is required");

            if (pageNumber < 1)
                return Error("PageNumber must be greater than 0");

            if (pageSize < 1 || pageSize > 100)
                return Error(
                    "PageSize must be between 1 and 100");

            try
            {
                var result =
                    await _escrowPaymentService
                        .GetTimelineByUserIdAsync(
                            userId,
                            search,
                            key,
                            pageNumber,
                            pageSize);

                if (!result.Data.Any())
                    return Error(
                        "No escrow payments found for this user",
                        404);

                return Success(
                    "Escrow payment timeline fetched",
                    result);
            }
            catch (ArgumentException ex)
            {
                return Error(
                    ex.Message,
                    400);
            }
        }
    }
}