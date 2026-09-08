using ahello_backend.Models.Digitalbookpayments;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DigitalBookPaymentController : ControllerBase
    {
        private readonly IDigitalBookPaymentRepository _digitalBookPaymentRepo;
        private readonly IDigitalBookPaymentLogRepository _logRepo;
        private readonly IRazorpayService _razorpayService;
        private readonly IExpertPayoutRepository _expertPayoutRepo;
        private readonly IDigitalBookPaymentService _digitalBookPaymentService;
        private readonly IPlatformSettingsRepository _platformSettingsRepo;

        public DigitalBookPaymentController(
            IDigitalBookPaymentRepository digitalBookPaymentRepo,
            IDigitalBookPaymentLogRepository logRepo,
            IRazorpayService razorpayService,
            IExpertPayoutRepository expertPayoutRepo,
            IDigitalBookPaymentService digitalBookPaymentService,
            IPlatformSettingsRepository platformSettingsRepo)
        {
            _digitalBookPaymentRepo = digitalBookPaymentRepo;
            _logRepo = logRepo;
            _razorpayService = razorpayService;
            _expertPayoutRepo = expertPayoutRepo;
            _digitalBookPaymentService = digitalBookPaymentService;
            _platformSettingsRepo = platformSettingsRepo;
        }

        private IActionResult Error(
            string message,
            int statusCode = 400,
            object? details = null)
        {
            return StatusCode(statusCode, new
            {
                success = false,
                message,
                details
            });
        }

        private IActionResult Success(
            string message,
            object? data = null)
        {
            return Ok(new
            {
                success = true,
                message,
                data
            });
        }

        // ============================================================
        // CREATE RAZORPAY ORDER
        // POST: /api/DigitalBookPayment/create-order
        // ============================================================
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder(
            [FromBody] DigitalBookCreateOrderDto dto)
        {
            if (dto.UserId <= 0 ||
                dto.ClientId <= 0 ||
                dto.DigitalBookId <= 0)
            {
                return Error(
                    "Valid UserId, ClientId and DigitalBookId are required");
            }

            var bookInfo =
                await _digitalBookPaymentRepo
                    .GetDigitalBookPaymentInfoAsync(dto.DigitalBookId);

            if (bookInfo == null)
                return Error("Digital book not found", 404);

            decimal bookPrice = bookInfo.Price;
            string bookTitle = bookInfo.Title;

            if (bookPrice <= 0)
                return Error("Invalid digital book price");

            // --------------------------------------------------------
            // Platform Fee
            // --------------------------------------------------------
            var platformSetting =
                await _platformSettingsRepo.GetActiveSettingAsync();

            if (platformSetting == null)
            {
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
                {
                    return Error(
                        "Platform fee percentage is not configured.",
                        500);
                }

                platformFee = Math.Round(
                    bookPrice *
                    platformSetting.FeePercentage.Value /
                    100m,
                    2);
            }
            else if (platformSetting.FeeType.Equals(
                         "amount",
                         StringComparison.OrdinalIgnoreCase))
            {
                if (!platformSetting.FeeAmount.HasValue)
                {
                    return Error(
                        "Platform fee amount is not configured.",
                        500);
                }

                platformFee = Math.Round(
                    platformSetting.FeeAmount.Value,
                    2);
            }
            else
            {
                return Error(
                    "Invalid platform fee configuration.",
                    500);
            }

            if (platformFee > bookPrice)
            {
                return Error(
                    "Platform fee cannot be greater than the digital book price.");
            }

            // --------------------------------------------------------
            // GST
            // --------------------------------------------------------
            decimal taxAmount = Math.Round(
                platformFee *
                platformSetting.GstRate /
                100m,
                2);

            decimal grandTotal = bookPrice + taxAmount;

            string receipt =
                $"digitalbook_{dto.DigitalBookId}_{DateTime.UtcNow:yyyyMMddHHmmss}";

            try
            {
                var orderResult =
                    await _razorpayService.CreateOrderAsync(
                        grandTotal,
                        "INR",
                        receipt,
                        bookTitle ?? "");

                await _logRepo.InsertAsync(
                    null,
                    dto.DigitalBookId,
                    "CREATE_ORDER",
                    "SUCCESS",
                    requestJson:
                        System.Text.Json.JsonSerializer.Serialize(dto),
                    responseJson: orderResult.responseJson,
                    createdBy: dto.CreatedBy.ToString());

                return Success(
                    "Order created successfully",
                    new
                    {
                        orderId = orderResult.orderId,
                        digitalBookId = dto.DigitalBookId,
                        bookPrice,
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
                    null,
                    dto.DigitalBookId,
                    "CREATE_ORDER",
                    "ERROR",
                    requestJson:
                        System.Text.Json.JsonSerializer.Serialize(dto),
                    errorMessage: ex.Message,
                    createdBy: dto.CreatedBy.ToString());

                return Error(
                    "Failed to create Razorpay order",
                    500,
                    new
                    {
                        razorpayError = ex.Message
                    });
            }
        }

        // ============================================================
        // VERIFY PAYMENT
        // POST: /api/DigitalBookPayment/verify-payment
        // ============================================================
        [HttpPost("verify-payment")]
        public async Task<IActionResult> VerifyPayment(
            [FromBody] DigitalBookVerifyAndHoldDto dto)
        {
            if (dto.UserId <= 0 ||
                dto.ClientId <= 0 ||
                dto.DigitalBookId <= 0)
            {
                return Error(
                    "Valid UserId, ClientId and DigitalBookId are required");
            }

            if (string.IsNullOrWhiteSpace(dto.RazorpayOrderId))
                return Error("RazorpayOrderId is required");

            if (string.IsNullOrWhiteSpace(dto.RazorpayPaymentId))
                return Error("RazorpayPaymentId is required");

            if (string.IsNullOrWhiteSpace(dto.RazorpaySignature))
                return Error("RazorpaySignature is required");

            // --------------------------------------------------------
            // Verify Razorpay Signature
            // --------------------------------------------------------
            bool isValid =
                _razorpayService.VerifySignature(
                    dto.RazorpayOrderId,
                    dto.RazorpayPaymentId,
                    dto.RazorpaySignature);

            if (!isValid)
            {
                await _logRepo.InsertAsync(
                    null,
                    dto.DigitalBookId,
                    "VERIFY_PAYMENT",
                    "FAILED",
                    requestJson:
                        System.Text.Json.JsonSerializer.Serialize(dto),
                    errorMessage:
                        "Invalid Razorpay payment signature",
                    createdBy: dto.CreatedBy.ToString());

                return Error(
                    "Invalid Razorpay payment signature");
            }

            try
            {
                // ----------------------------------------------------
                // Get Digital Book Price
                // ----------------------------------------------------
                var bookInfo =
                    await _digitalBookPaymentRepo
                        .GetDigitalBookPaymentInfoAsync(
                            dto.DigitalBookId);

                if (bookInfo == null)
                {
                    await _logRepo.InsertAsync(
                        null,
                        dto.DigitalBookId,
                        "VERIFY_PAYMENT",
                        "ERROR",
                        requestJson:
                            System.Text.Json.JsonSerializer.Serialize(dto),
                        errorMessage:
                            $"Digital book {dto.DigitalBookId} not found at verify time",
                        createdBy: dto.CreatedBy.ToString());

                    return Error(
                        "Invalid digital book",
                        400);
                }

                decimal bookPrice = bookInfo.Price;

                if (bookPrice <= 0)
                    return Error(
                        "Invalid digital book price",
                        400);

                // ----------------------------------------------------
                // Expert Razorpay Account
                // ----------------------------------------------------
                var expertAccount =
                    await _expertPayoutRepo
                        .GetByUserIdAsync(dto.UserId);

                if (expertAccount == null ||
                    expertAccount.AccountStatus != "ACTIVE" ||
                    string.IsNullOrWhiteSpace(
                        expertAccount.RazorpayAccountId))
                {
                    await _logRepo.InsertAsync(
                        null,
                        dto.DigitalBookId,
                        "VERIFY_PAYMENT",
                        "ERROR",
                        requestJson:
                            System.Text.Json.JsonSerializer.Serialize(dto),
                        errorMessage:
                            $"Expert {dto.UserId} payout account not ACTIVE " +
                            $"(status: {expertAccount?.AccountStatus ?? "NOT_CREATED"}). " +
                            "Payment captured, escrow blocked pending manual reconciliation.",
                        createdBy: dto.CreatedBy.ToString());

                    return Error(
                        $"Payment captured but expert's payout account isn't active yet. " +
                        $"Contact support with PaymentId {dto.RazorpayPaymentId}.",
                        500,
                        new
                        {
                            expertStatus =
                                expertAccount?.AccountStatus ??
                                "NOT_CREATED"
                        });
                }

                // ----------------------------------------------------
                // Platform Settings
                // ----------------------------------------------------
                var platformSetting =
                    await _platformSettingsRepo
                        .GetActiveSettingAsync();

                if (platformSetting == null)
                {
                    await _logRepo.InsertAsync(
                        null,
                        dto.DigitalBookId,
                        "VERIFY_PAYMENT",
                        "ERROR",
                        requestJson:
                            System.Text.Json.JsonSerializer.Serialize(dto),
                        errorMessage:
                            "No active platform fee setting found.",
                        createdBy: dto.CreatedBy.ToString());

                    return Error(
                        "Platform fee configuration is not available. Contact support.",
                        500);
                }

                // ----------------------------------------------------
                // Platform Fee
                // ----------------------------------------------------
                decimal platformFee;

                if (platformSetting.FeeType.Equals(
                        "percentage",
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (!platformSetting.FeePercentage.HasValue)
                    {
                        return Error(
                            "Platform fee percentage is not configured.",
                            500);
                    }

                    platformFee = Math.Round(
                        bookPrice *
                        platformSetting.FeePercentage.Value /
                        100m,
                        2);
                }
                else if (platformSetting.FeeType.Equals(
                             "amount",
                             StringComparison.OrdinalIgnoreCase))
                {
                    if (!platformSetting.FeeAmount.HasValue)
                    {
                        return Error(
                            "Platform fee amount is not configured.",
                            500);
                    }

                    platformFee = Math.Round(
                        platformSetting.FeeAmount.Value,
                        2);
                }
                else
                {
                    return Error(
                        "Invalid platform fee configuration.",
                        500);
                }

                if (platformFee > bookPrice)
                {
                    return Error(
                        "Platform fee cannot be greater than the digital book price.",
                        400);
                }

                // ----------------------------------------------------
                // GST + Total
                // ----------------------------------------------------
                decimal taxAmount = Math.Round(
                    platformFee *
                    platformSetting.GstRate /
                    100m,
                    2);

                decimal grandTotal =
                    bookPrice + taxAmount;

                decimal expertAmount =
                    bookPrice - platformFee;

                // ----------------------------------------------------
                // Create Held Transfer
                // ----------------------------------------------------
                string transferId;
                string transferJson;

                try
                {
                    (transferId, transferJson) =
                        await _razorpayService
                            .CreateHeldTransferAsync(
                                dto.RazorpayPaymentId,
                                expertAccount.RazorpayAccountId,
                                expertAmount);
                }
                catch (Exception ex)
                {
                    await _logRepo.InsertAsync(
                        null,
                        dto.DigitalBookId,
                        "CREATE_HELD_TRANSFER",
                        "ERROR",
                        requestJson:
                            System.Text.Json.JsonSerializer.Serialize(
                                new
                                {
                                    dto.DigitalBookId,
                                    dto.RazorpayPaymentId,
                                    expertAccount.RazorpayAccountId,
                                    expertAmount
                                }),
                        errorMessage: ex.Message,
                        createdBy: dto.CreatedBy.ToString());

                    return Error(
                        $"Payment verified but transfer to expert failed. " +
                        $"Contact support with PaymentId {dto.RazorpayPaymentId}.",
                        500,
                        new
                        {
                            razorpayError = ex.Message
                        });
                }

                var now = DateTime.Now;

                // ----------------------------------------------------
                // Verify Response
                // ----------------------------------------------------
                var verifyJson =
                    System.Text.Json.JsonSerializer.Serialize(
                        new
                        {
                            dto.DigitalBookId,
                            dto.RazorpayOrderId,
                            dto.RazorpayPaymentId,
                            dto.RazorpaySignature,
                            VerifiedAt = now
                        });

                // ----------------------------------------------------
                // Payment Record
                // ----------------------------------------------------
                var payment = new DigitalBookPayment
                {
                    DigitalBookId = dto.DigitalBookId,
                    UserId = dto.UserId,
                    ClientId = dto.ClientId,

                    RazorpayAccountId =
                        expertAccount.RazorpayAccountId,

                    RazorpayOrderId =
                        dto.RazorpayOrderId,

                    RazorpayPaymentId =
                        dto.RazorpayPaymentId,

                    RazorpaySignature =
                        dto.RazorpaySignature,

                    RazorpayTransferId =
                        transferId,

                    TotalAmount =
                        grandTotal,

                    PlatformFee =
                        platformFee,

                    TaxAmount =
                        taxAmount,

                    TaxRate =
                        platformSetting.GstRate,

                    ExpertAmount =
                        expertAmount,

                    Currency = "INR",

                    Status = "HELD",

                    VerifyResponseJson =
                        verifyJson,

                    TransferResponseJson =
                        transferJson,

                    PaidAt = now,

                    HeldAt = now,

                    CreatedBy =
                        dto.CreatedBy.ToString()
                };

                int digitalBookPaymentId =
                    await _digitalBookPaymentRepo
                        .InsertAsync(payment);

                payment.DigitalBookPaymentId =
                    digitalBookPaymentId;

                // ----------------------------------------------------
                // Logs
                // ----------------------------------------------------
                await _logRepo.InsertAsync(
                    digitalBookPaymentId,
                    dto.DigitalBookId,
                    "VERIFY_PAYMENT",
                    "SUCCESS",
                    requestJson:
                        System.Text.Json.JsonSerializer.Serialize(dto),
                    responseJson: verifyJson,
                    createdBy: dto.CreatedBy.ToString());

                await _logRepo.InsertAsync(
                    digitalBookPaymentId,
                    dto.DigitalBookId,
                    "CREATE_HELD_TRANSFER",
                    "SUCCESS",
                    responseJson: transferJson,
                    createdBy: dto.CreatedBy.ToString());

                return Success(
                    "Payment verified, transfer held",
                    new
                    {
                        digitalBookPaymentId,
                        digitalBookId =
                            dto.DigitalBookId,
                        orderId =
                            dto.RazorpayOrderId,
                        paymentId =
                            dto.RazorpayPaymentId,
                        transferId,
                        status = "HELD"
                    });
            }
            catch (Exception ex)
            {
                await _logRepo.InsertAsync(
                    null,
                    dto.DigitalBookId,
                    "VERIFY_PAYMENT",
                    "ERROR",
                    requestJson:
                        System.Text.Json.JsonSerializer.Serialize(dto),
                    errorMessage: ex.Message,
                    createdBy: dto.CreatedBy.ToString());

                return Error(
                    $"Payment captured but escrow creation failed. " +
                    $"Contact support with PaymentId {dto.RazorpayPaymentId}.",
                    500,
                    new
                    {
                        razorpayError = ex.Message
                    });
            }
        }

        // ============================================================
        // RELEASE
        // POST: /api/DigitalBookPayment/release
        // ============================================================
        [HttpPost("release")]
        public async Task<IActionResult> Release(
            [FromBody] DigitalBookReleaseDto dto)
        {
            if (dto.DigitalBookPaymentId <= 0)
                return Error(
                    "Valid DigitalBookPaymentId is required");

            var payment =
                await _digitalBookPaymentRepo
                    .GetByIdAsync(dto.DigitalBookPaymentId);

            if (payment == null)
                return Error(
                    "Digital book payment record not found",
                    404);

            if (payment.Status == "RELEASED")
            {
                return Success(
                    "Transfer already released",
                    new
                    {
                        status = "RELEASED"
                    });
            }

            if (payment.Status != "HELD")
            {
                return Error(
                    "Payment not in HELD status",
                    409,
                    new
                    {
                        currentStatus = payment.Status
                    });
            }

            try
            {
                var releaseJson =
                    await _razorpayService
                        .ReleaseTransferAsync(
                            payment.RazorpayTransferId);

                await _digitalBookPaymentRepo
                    .UpdateReleaseAsync(
                        payment.DigitalBookPaymentId,
                        releaseJson);

                await _logRepo.InsertAsync(
                    payment.DigitalBookPaymentId,
                    payment.DigitalBookId,
                    "RELEASE_TRANSFER",
                    "SUCCESS",
                    responseJson: releaseJson);

                return Success(
                    "Transfer released successfully",
                    new
                    {
                        status = "RELEASED"
                    });
            }
            catch (Exception ex)
            {
                await _logRepo.InsertAsync(
                    payment.DigitalBookPaymentId,
                    payment.DigitalBookId,
                    "RELEASE_TRANSFER",
                    "ERROR",
                    errorMessage: ex.Message);

                return Error(
                    "Failed to release transfer",
                    500,
                    new
                    {
                        razorpayError = ex.Message
                    });
            }
        }

        // ============================================================
        // REFUND
        // POST: /api/DigitalBookPayment/refund
        // ============================================================
        [HttpPost("refund")]
        public async Task<IActionResult> Refund(
            [FromBody] DigitalBookRefundDto dto)
        {
            if (dto.DigitalBookId <= 0)
                return Error(
                    "Valid DigitalBookId is required");

            var payment =
                await _digitalBookPaymentRepo
                    .GetByDigitalBookIdAsync(
                        dto.DigitalBookId);

            if (payment == null)
                return Error(
                    "Digital book payment record not found",
                    404);

            if (payment.Status == "REFUNDED")
            {
                return Success(
                    "Already refunded",
                    new
                    {
                        status = "REFUNDED"
                    });
            }

            // --------------------------------------------------------
            // Reverse held transfer first
            // --------------------------------------------------------
            if (!string.IsNullOrEmpty(
                    payment.RazorpayTransferId))
            {
                try
                {
                    var reversalJson =
                        await _razorpayService
                            .ReverseTransferAsync(
                                payment.RazorpayTransferId,
                                payment.ExpertAmount);

                    await _logRepo.InsertAsync(
                        payment.DigitalBookPaymentId,
                        dto.DigitalBookId,
                        "REVERSE_TRANSFER",
                        "SUCCESS",
                        responseJson: reversalJson);
                }
                catch (Exception ex)
                {
                    await _logRepo.InsertAsync(
                        payment.DigitalBookPaymentId,
                        dto.DigitalBookId,
                        "REVERSE_TRANSFER",
                        "ERROR",
                        errorMessage: ex.Message);

                    return Error(
                        "Failed to reverse transfer before refund",
                        500,
                        new
                        {
                            razorpayError = ex.Message
                        });
                }
            }

            // --------------------------------------------------------
            // Refund Payment
            // --------------------------------------------------------
            try
            {
                var refundJson =
                    await _razorpayService
                        .RefundPaymentAsync(
                            payment.RazorpayPaymentId,
                            payment.TotalAmount);

                await _digitalBookPaymentRepo
                    .UpdateRefundAsync(
                        payment.DigitalBookPaymentId,
                        refundJson);

                await _logRepo.InsertAsync(
                    payment.DigitalBookPaymentId,
                    dto.DigitalBookId,
                    "REFUND_PAYMENT",
                    "SUCCESS",
                    responseJson: refundJson);

                return Success(
                    "Refund processed successfully",
                    new
                    {
                        status = "REFUNDED"
                    });
            }
            catch (Exception ex)
            {
                await _logRepo.InsertAsync(
                    payment.DigitalBookPaymentId,
                    dto.DigitalBookId,
                    "REFUND_PAYMENT",
                    "ERROR",
                    errorMessage: ex.Message);

                return Error(
                    "Failed to process refund",
                    500,
                    new
                    {
                        razorpayError = ex.Message
                    });
            }
        }

        // ============================================================
        // GET PAYMENT BY DIGITAL BOOK
        // GET: /api/DigitalBookPayment/digitalbook/{digitalBookId}
        // ============================================================
        [HttpGet("digitalbook/{digitalBookId}")]
        public async Task<IActionResult> GetByDigitalBook(
            int digitalBookId)
        {
            if (digitalBookId <= 0)
                return Error(
                    "Valid DigitalBookId is required");

            var payment =
                await _digitalBookPaymentRepo
                    .GetByDigitalBookIdAsync(
                        digitalBookId);

            if (payment == null)
                return Error(
                    "Digital book payment not found",
                    404);

            return Success(
                "Digital book payment fetched",
                payment);
        }

        // ============================================================
        // USER PAYMENT TIMELINE
        // GET:
        // /api/DigitalBookPayment/user/{userId}/timeline
        // ============================================================
        [HttpGet("user/{userId}/timeline")]
        public async Task<IActionResult> GetTimelineByUserId(
            int userId,
            [FromQuery] string? search = null,
            [FromQuery] string? key = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (userId <= 0)
                return Error(
                    "Valid UserId is required");

            if (pageNumber < 1)
                return Error(
                    "PageNumber must be greater than 0");

            if (pageSize < 1 || pageSize > 100)
            {
                return Error(
                    "PageSize must be between 1 and 100");
            }

            try
            {
                var result =
                    await _digitalBookPaymentService
                        .GetTimelineByUserIdAsync(
                            userId,
                            search,
                            key,
                            pageNumber,
                            pageSize);

                if (!result.Data.Any())
                {
                    return Error(
                        "No digital book payments found for this user",
                        404);
                }

                return Success(
                    "Digital book payment timeline fetched",
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