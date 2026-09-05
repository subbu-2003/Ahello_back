using ahello_backend.Models.WebinarPayment;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebinarPaymentController : ControllerBase
    {
        private readonly IWebinarPaymentRepository _webinarPaymentRepo;
        private readonly IEscrowPaymentLogRepository _logRepo;
        private readonly IRazorpayService _razorpayService;
        private readonly IWebinarPaymentRepository _webinarRegistrationRepo;
        private readonly IExpertPayoutRepository _expertPayoutRepo;
        private readonly IWebinarPaymentService _webinarPaymentService;
        private readonly IPlatformSettingsRepository _platformSettingsRepo;
        //private readonly IInvoiceService _invoiceService;
        //private readonly IEmailRepository _emailRepository;

        public WebinarPaymentController(
            IWebinarPaymentRepository webinarPaymentRepo,
            IEscrowPaymentLogRepository logRepo,
            IRazorpayService razorpayService,
            IWebinarPaymentRepository webinarRegistrationRepo,
            IExpertPayoutRepository expertPayoutRepo,
            IWebinarPaymentService webinarPaymentService,
            IPlatformSettingsRepository platformSettingsRepo,
            IInvoiceService invoiceService,
            IEmailRepository emailRepository)
        {
            _webinarPaymentRepo = webinarPaymentRepo;
            _logRepo = logRepo;
            _razorpayService = razorpayService;
            _webinarRegistrationRepo = webinarRegistrationRepo;
            _expertPayoutRepo = expertPayoutRepo;
            _webinarPaymentService = webinarPaymentService;
            _platformSettingsRepo = platformSettingsRepo;
            //    _invoiceService = invoiceService;
            //    _emailRepository = emailRepository;
        }

        private IActionResult Error(string message, int statusCode = 400, object? details = null)
            => StatusCode(statusCode, new { success = false, message, details });

        private IActionResult Success(string message, object? data = null)
            => Ok(new { success = true, message, data });

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateWebinarOrderDto dto)
        {
            if (dto.UserId <= 0 || dto.ClientId <= 0 || dto.WebinarId <= 0 || dto.WebinarRegistrationId <= 0)
                return Error("Valid webinar registration details are required");

            var webinarPrice = await _webinarPaymentRepo.GetWebinarPriceAsync(dto.WebinarId);
            if (webinarPrice == null || webinarPrice <= 0)
                return Error("Invalid webinar");
            var webinarName = await _webinarPaymentRepo.GetWebinarNameAsync(dto.WebinarId);

            var platformSetting = await _platformSettingsRepo.GetActiveSettingAsync();
            if (platformSetting == null)
                return Error("Platform fee configuration is not available. Contact support.", 500);

            decimal platformFee;
            if (platformSetting.FeeType.Equals("percentage", StringComparison.OrdinalIgnoreCase))
            {
                if (!platformSetting.FeePercentage.HasValue)
                    return Error("Platform fee percentage is not configured.", 500);

                platformFee = Math.Round(webinarPrice.Value * platformSetting.FeePercentage.Value / 100m, 2);
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

            if (platformFee > webinarPrice.Value)
                return Error("Platform fee cannot be greater than the webinar price.", 400);

            var taxAmount = Math.Round(platformFee * platformSetting.GstRate / 100m, 2);
            var grandTotal = webinarPrice.Value + taxAmount;

            string receipt = $"webinarreg_{dto.WebinarRegistrationId}_{DateTime.UtcNow:yyyyMMddHHmmss}";

            try
            {
                var orderResult = await _razorpayService.CreateOrderAsync(
                    grandTotal, "INR", receipt, webinarName ?? "");

                await _logRepo.InsertAsync(
                    null, dto.WebinarRegistrationId, "CREATE_ORDER", "SUCCESS",
                    requestJson: System.Text.Json.JsonSerializer.Serialize(dto),
                    responseJson: orderResult.responseJson,
                    createdBy: dto.CreatedBy);

                return Success("Order created successfully", new
                {
                    orderId = orderResult.orderId,
                    webinarPrice = webinarPrice.Value,
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
                    null, dto.WebinarRegistrationId, "CREATE_ORDER", "ERROR",
                    requestJson: System.Text.Json.JsonSerializer.Serialize(dto),
                    errorMessage: ex.Message);

                return Error("Failed to create Razorpay order", 500, new { razorpayError = ex.Message });
            }
        }
        [HttpPost("verify-payment")]
        public async Task<IActionResult> VerifyPayment([FromBody] VerifyWebinarPaymentDto dto)
        {
            var wp = dto.WebinarRegistrationPayload;

            if (wp.UserId <= 0 || wp.WebinarId <= 0 || wp.WebinarRegistrationId <= 0)
                return Error("Valid webinar registration details are required");

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
                    null, wp.WebinarRegistrationId, "VERIFY_PAYMENT", "FAILED",
                    requestJson: System.Text.Json.JsonSerializer.Serialize(dto),
                    errorMessage: "Invalid Razorpay payment signature",
                    createdBy: wp.CreatedBy);

                return Error("Invalid Razorpay payment signature", 400);
            }

            try
            {
                var webinarPrice = await _webinarPaymentRepo.GetWebinarPriceAsync(wp.WebinarId);
                if (webinarPrice == null || webinarPrice <= 0)
                {
                    await _logRepo.InsertAsync(
                        null, wp.WebinarRegistrationId, "VERIFY_PAYMENT", "ERROR",
                        requestJson: System.Text.Json.JsonSerializer.Serialize(dto),
                        errorMessage: $"Invalid WebinarId {wp.WebinarId} at verify time",
                        createdBy: wp.CreatedBy);

                    return Error("Invalid webinar", 400);
                }

                var expertAccount = await _expertPayoutRepo.GetByUserIdAsync(wp.UserId);

                if (expertAccount == null || expertAccount.AccountStatus != "ACTIVE" ||
                    string.IsNullOrWhiteSpace(expertAccount.RazorpayAccountId))
                {
                    await _logRepo.InsertAsync(
                        null, wp.WebinarRegistrationId, "VERIFY_PAYMENT", "ERROR",
                        requestJson: System.Text.Json.JsonSerializer.Serialize(dto),
                        errorMessage: $"Expert {wp.UserId} payout account not ACTIVE (status: {expertAccount?.AccountStatus ?? "NOT_CREATED"}). Payment captured, escrow blocked pending manual reconciliation.",
                        createdBy: wp.CreatedBy);

                    return Error(
                        $"Payment captured but expert's payout account isn't active yet. Contact support with PaymentId {dto.RazorpayPaymentId}.",
                        500,
                        new { expertStatus = expertAccount?.AccountStatus ?? "NOT_CREATED" });
                }

                var platformSetting = await _platformSettingsRepo.GetActiveSettingAsync();

                if (platformSetting == null)
                {
                    await _logRepo.InsertAsync(
                        null, wp.WebinarRegistrationId, "VERIFY_PAYMENT", "ERROR",
                        requestJson: System.Text.Json.JsonSerializer.Serialize(dto),
                        errorMessage: "No active platform fee setting found.",
                        createdBy: wp.CreatedBy);

                    return Error("Platform fee configuration is not available. Contact support.", 500);
                }

                decimal platformFee;

                if (platformSetting.FeeType.Equals("percentage", StringComparison.OrdinalIgnoreCase))
                {
                    if (!platformSetting.FeePercentage.HasValue)
                        return Error("Platform fee percentage is not configured.", 500);

                    platformFee = Math.Round(webinarPrice.Value * platformSetting.FeePercentage.Value / 100m, 2);
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

                if (platformFee > webinarPrice.Value)
                    return Error("Platform fee cannot be greater than the webinar price.", 400);

                var taxAmount = Math.Round(platformFee * platformSetting.GstRate / 100m, 2);
                var grandTotal = webinarPrice.Value + taxAmount;
                decimal expertAmount = webinarPrice.Value - platformFee;

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
                    await _logRepo.InsertAsync(
                        null, wp.WebinarRegistrationId, "CREATE_HELD_TRANSFER", "ERROR",
                        requestJson: System.Text.Json.JsonSerializer.Serialize(new { wp.WebinarRegistrationId, dto.RazorpayPaymentId, expertAccount.RazorpayAccountId, expertAmount }),
                        errorMessage: ex.Message,
                        createdBy: wp.CreatedBy);

                    return Error(
                        $"Payment verified but transfer to expert failed. Contact support with PaymentId {dto.RazorpayPaymentId}.",
                        500,
                        new { razorpayError = ex.Message });
                }

                var now = DateTime.Now;

                var verifyJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    wp.WebinarRegistrationId,
                    dto.RazorpayOrderId,
                    dto.RazorpayPaymentId,
                    dto.RazorpaySignature,
                    VerifiedAt = now
                });

                var payment = new WebinarPayment
                {
                    WebinarRegistrationId = wp.WebinarRegistrationId,
                    UserId = wp.UserId,
                    ClientId = wp.ClientId,
                    RazorpayAccountId = expertAccount.RazorpayAccountId,
                    RazorpayOrderId = dto.RazorpayOrderId,
                    RazorpayPaymentId = dto.RazorpayPaymentId,
                    RazorpaySignature = dto.RazorpaySignature,
                    RazorpayTransferId = transferId,
                    TotalAmount = grandTotal,
                    PlatformFee = platformFee,
                    TaxAmount = taxAmount,
                    TaxRate = platformSetting.GstRate,
                    ExpertAmount = expertAmount,
                    Currency = "INR",
                    Status = "HELD",
                    VerifyResponseJson = verifyJson,
                    TransferResponseJson = transferJson,
                    PaidAt = now,
                    HeldAt = now,
                    CreatedBy = wp.CreatedBy
                };

                int webinarPaymentId = await _webinarPaymentRepo.InsertAsync(payment);
                payment.WebinarPaymentId = webinarPaymentId;

                await _logRepo.InsertAsync(
                    webinarPaymentId, wp.WebinarRegistrationId, "VERIFY_PAYMENT", "SUCCESS",
                    requestJson: System.Text.Json.JsonSerializer.Serialize(dto),
                    responseJson: verifyJson,
                    createdBy: wp.CreatedBy);

                await _logRepo.InsertAsync(
                    webinarPaymentId, wp.WebinarRegistrationId, "CREATE_HELD_TRANSFER", "SUCCESS",
                    responseJson: transferJson,
                    createdBy: wp.CreatedBy);

                return Success("Payment verified, webinar registration confirmed, transfer held", new
                {
                    webinarPaymentId,
                    webinarRegistrationId = wp.WebinarRegistrationId,
                    orderId = dto.RazorpayOrderId,
                    paymentId = dto.RazorpayPaymentId,
                    transferId,
                    status = "HELD"
                });
            }
            catch (Exception ex)
            {
                await _logRepo.InsertAsync(
                    null, wp.WebinarRegistrationId, "VERIFY_PAYMENT", "ERROR",
                    requestJson: System.Text.Json.JsonSerializer.Serialize(dto),
                    errorMessage: ex.Message,
                    createdBy: wp.CreatedBy);

                return Error(
                    $"Payment captured but webinar payment record creation failed. Contact support with PaymentId {dto.RazorpayPaymentId}.",
                    500,
                    new { razorpayError = ex.Message });
            }
        }

        [HttpPost("release")]
        public async Task<IActionResult> Release([FromBody] ReleaseWebinarDto dto)
        {
            if (dto.WebinarRegistrationId <= 0)
                return Error("Valid WebinarRegistrationId is required");

            var payment = await _webinarPaymentRepo.GetByWebinarRegistrationIdAsync(dto.WebinarRegistrationId);
            if (payment == null)
                return Error("Webinar payment record not found", 404);

            if (payment.Status == "RELEASED")
                return Success("Transfer already released", new { status = "RELEASED" });

            if (payment.Status != "HELD")
                return Error("Payment not in HELD status", 409, new { currentStatus = payment.Status });

            try
            {
                var releaseJson = await _razorpayService.ReleaseTransferAsync(payment.RazorpayTransferId);

                await _webinarPaymentRepo.UpdateReleaseAsync(payment.WebinarPaymentId, releaseJson);
                await _logRepo.InsertAsync(payment.WebinarPaymentId, dto.WebinarRegistrationId,
                    "RELEASE_TRANSFER", "SUCCESS", responseJson: releaseJson);

                return Success("Transfer released successfully", new { status = "RELEASED" });
            }
            catch (Exception ex)
            {
                await _logRepo.InsertAsync(payment.WebinarPaymentId, dto.WebinarRegistrationId,
                    "RELEASE_TRANSFER", "ERROR", errorMessage: ex.Message);
                return Error("Failed to release transfer", 500, new { razorpayError = ex.Message });
            }
        }

        [HttpPost("refund")]
        public async Task<IActionResult> Refund([FromBody] RefundWebinarDto dto)
        {
            if (dto.WebinarRegistrationId <= 0)
                return Error("Valid WebinarRegistrationId is required");

            var payment = await _webinarPaymentRepo.GetByWebinarRegistrationIdAsync(dto.WebinarRegistrationId);
            if (payment == null)
                return Error("Webinar payment record not found", 404);

            if (payment.Status == "REFUNDED")
                return Success("Already refunded", new { status = "REFUNDED" });

            if (!string.IsNullOrEmpty(payment.RazorpayTransferId))
            {
                try
                {
                    var reversalJson = await _razorpayService.ReverseTransferAsync(
                        payment.RazorpayTransferId, payment.ExpertAmount);
                    await _logRepo.InsertAsync(payment.WebinarPaymentId, dto.WebinarRegistrationId,
                        "REVERSE_TRANSFER", "SUCCESS", responseJson: reversalJson);
                }
                catch (Exception ex)
                {
                    await _logRepo.InsertAsync(payment.WebinarPaymentId, dto.WebinarRegistrationId,
                        "REVERSE_TRANSFER", "ERROR", errorMessage: ex.Message);
                    return Error("Failed to reverse transfer before refund", 500,
                        new { razorpayError = ex.Message });
                }
            }

            try
            {
                var refundJson = await _razorpayService.RefundPaymentAsync(
                    payment.RazorpayPaymentId, payment.TotalAmount);

                await _webinarPaymentRepo.UpdateRefundAsync(payment.WebinarPaymentId, refundJson);
                await _logRepo.InsertAsync(payment.WebinarPaymentId, dto.WebinarRegistrationId,
                    "REFUND_PAYMENT", "SUCCESS", responseJson: refundJson);

                return Success("Refund processed successfully", new { status = "REFUNDED" });
            }
            catch (Exception ex)
            {
                await _logRepo.InsertAsync(payment.WebinarPaymentId, dto.WebinarRegistrationId,
                    "REFUND_PAYMENT", "ERROR", errorMessage: ex.Message);
                return Error("Failed to process refund", 500, new { razorpayError = ex.Message });
            }
        }

        [HttpGet("registration/{webinarRegistrationId}")]
        public async Task<IActionResult> GetByWebinarRegistration(int webinarRegistrationId)
        {
            var payment = await _webinarPaymentRepo.GetByWebinarRegistrationIdAsync(webinarRegistrationId);
            if (payment == null) return Error("Not found", 404);
            return Success("Webinar payment fetched", payment);
        }

        [HttpGet("user/{userId}/timeline")]
        public async Task<IActionResult> GetTimelineByUserId(
            int userId,
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
                return Error("PageSize must be between 1 and 100");

            try
            {
                var result = await _webinarPaymentService.GetTimelineByUserIdAsync(
                    userId, search, key, pageNumber, pageSize);

                if (!result.Data.Any())
                    return Error("No webinar payments found for this user", 404);

                return Success("Webinar payment timeline fetched", result);
            }
            catch (ArgumentException ex)
            {
                return Error(ex.Message, 400);
            }
        }
    }
}
