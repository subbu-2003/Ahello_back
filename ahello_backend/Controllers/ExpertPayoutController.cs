using ahello_backend.DTO.Payment;
using ahello_backend.Models.Payment;
using ahello_backend.Models.Payments;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpertPayoutController : ControllerBase
    {
        private readonly IUserDynamicRepository _userRepo;
        private readonly IExpertPayoutRepository _payoutRepo;
        private readonly IRazorpayService _razorpayService;

        public ExpertPayoutController(
            IUserDynamicRepository userRepo,
            IExpertPayoutRepository payoutRepo,
            IRazorpayService razorpayService)
        {
            _userRepo = userRepo;
            _payoutRepo = payoutRepo;
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

        [HttpPost("create-linked-account")]
        public async Task<IActionResult> CreateLinkedAccount([FromBody] CreateLinkedAccountDto dto)
        {
            if (dto.UserId <= 0)
                return Error("Valid UserId is required");

            if (string.IsNullOrWhiteSpace(dto.BusinessType))
                return Error("Business type is required");

            if (string.IsNullOrWhiteSpace(dto.CustomerFacingBusinessName))
                return Error("Customer facing business name is required");

            var user = await _userRepo.GetByIdAsync(dto.UserId);

            if (user == null)
                return Error("User not found", 404);

            if (string.IsNullOrWhiteSpace(user.Email))
                return Error("User email is required for Razorpay account creation");

            if (string.IsNullOrWhiteSpace(user.MobileNumber))
                return Error("User mobile number is required for Razorpay account creation");

            if (string.IsNullOrWhiteSpace(user.FullName))
                return Error("User full name is required for Razorpay account creation");

            var existing = await _payoutRepo.GetByUserIdAsync(dto.UserId);

            if (existing != null &&
                existing.AccountStatus != "NOT_CREATED" &&
                existing.AccountStatus != "FAILED")
            {
                return Error("Razorpay account already created for this user", 409, new
                {
                    existing.AccountStatus,
                    existing.RazorpayAccountId
                });
            }

            try
            {
                if (existing == null)
                {
                    await _payoutRepo.InsertAsync(new ExpertPayoutAccount
                    {
                        UserId = dto.UserId,
                        BusinessType = dto.BusinessType,
                        CustomerFacingBusinessName = dto.CustomerFacingBusinessName,
                        CreatedBy = dto.UserId.ToString()
                    });
                }

                var result = await _razorpayService.CreateLinkedAccountAsync(dto, user);

                await _payoutRepo.UpdateAccountCreatedAsync(
                    dto.UserId,
                    result.accountId,
                    result.responseJson);

                return Success("Linked account created successfully", new
                {
                    status = "ACCOUNT_CREATED",
                    accountId = result.accountId
                });
            }
            catch (Exception ex)
            {
                await _payoutRepo.SetFailedAsync(dto.UserId, ex.Message);

                return Error("Failed to create linked account", 500, new
                {
                    razorpayError = ex.Message
                });
            }
        }

        [HttpPost("create-stakeholder")]
        public async Task<IActionResult> CreateStakeholder([FromBody] CreateStakeholderDto dto)
        {
            if (dto.UserId <= 0)
                return Error("Valid UserId is required");

            if (string.IsNullOrWhiteSpace(dto.Pan))
                return Error("PAN is required");

            if (string.IsNullOrWhiteSpace(dto.Dob))
                return Error("Date of birth is required");

            var user = await _userRepo.GetByIdAsync(dto.UserId);

            if (user == null)
                return Error("User not found", 404);

            var account = await _payoutRepo.GetByUserIdAsync(dto.UserId);

            if (account == null)
                return Error("Linked account not created. Create linked account first");

            if (account.AccountStatus != "ACCOUNT_CREATED")
                return Error("Invalid payout account status", 409, new
                {
                    currentStatus = account.AccountStatus,
                    requiredStatus = "ACCOUNT_CREATED"
                });

            if (string.IsNullOrWhiteSpace(account.RazorpayAccountId))
                return Error("Razorpay account id missing");

            try
            {
                var result = await _razorpayService.CreateStakeholderAsync(
                    account.RazorpayAccountId,
                    dto,
                    user);

                await _payoutRepo.UpdateStakeholderCreatedAsync(
                    dto.UserId,
                    result.stakeholderId,
                    result.responseJson);

                return Success("Stakeholder created successfully", new
                {
                    status = "STAKEHOLDER_CREATED",
                    stakeholderId = result.stakeholderId
                });
            }
            catch (Exception ex)
            {
                await _payoutRepo.SetFailedAsync(dto.UserId, ex.Message);

                return Error("Failed to create stakeholder", 500, new
                {
                    razorpayError = ex.Message
                });
            }
        }

        [HttpPost("request-product")]
        public async Task<IActionResult> RequestProduct([FromBody] ExpertIdDto dto)
        {
            if (dto.UserId <= 0)
                return Error("Valid UserId is required");

            var account = await _payoutRepo.GetByUserIdAsync(dto.UserId);

            if (account == null)
                return Error("Linked account not created");

            if (account.AccountStatus != "STAKEHOLDER_CREATED")
                return Error("Invalid payout account status", 409, new
                {
                    currentStatus = account.AccountStatus,
                    requiredStatus = "STAKEHOLDER_CREATED"
                });

            if (string.IsNullOrWhiteSpace(account.RazorpayAccountId))
                return Error("Razorpay account id missing");

            try
            {
                var result = await _razorpayService.RequestProductAsync(
                    account.RazorpayAccountId);

                await _payoutRepo.UpdateProductRequestedAsync(
                    dto.UserId,
                    result.productId,
                    result.responseJson);

                return Success("Route product requested successfully", new
                {
                    status = "PRODUCT_REQUESTED",
                    productId = result.productId
                });
            }
            catch (Exception ex)
            {
                await _payoutRepo.SetFailedAsync(dto.UserId, ex.Message);

                return Error("Failed to request Route product", 500, new
                {
                    razorpayError = ex.Message
                });
            }
        }

        [HttpPatch("update-product")]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductDto dto)
        {
            if (dto.UserId <= 0)
                return Error("Valid UserId is required");

            if (string.IsNullOrWhiteSpace(dto.AccountNumber))
                return Error("Bank account number is required");

            if (string.IsNullOrWhiteSpace(dto.IfscCode))
                return Error("IFSC code is required");

            if (string.IsNullOrWhiteSpace(dto.BeneficiaryName))
                return Error("Beneficiary name is required");

            var account = await _payoutRepo.GetByUserIdAsync(dto.UserId);

            if (account == null)
                return Error("Linked account not created");

            if (account.AccountStatus != "PRODUCT_REQUESTED")
                return Error("Invalid payout account status", 409, new
                {
                    currentStatus = account.AccountStatus,
                    requiredStatus = "PRODUCT_REQUESTED"
                });

            if (string.IsNullOrWhiteSpace(account.RazorpayAccountId))
                return Error("Razorpay account id missing");

            if (string.IsNullOrWhiteSpace(account.RazorpayProductId))
                return Error("Razorpay product id missing");

            try
            {
                var responseJson = await _razorpayService.UpdateProductAsync(
                    account.RazorpayAccountId,
                    account.RazorpayProductId,
                    dto);

                await _payoutRepo.UpdateProductActivatedAsync(
                    dto.UserId,
                    responseJson);

                return Success("Product activated successfully", new
                {
                    status = "ACTIVE"
                });
            }
            catch (Exception ex)
            {
                await _payoutRepo.SetFailedAsync(dto.UserId, ex.Message);

                return Error("Failed to update Route product", 500, new
                {
                    razorpayError = ex.Message
                });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            if (userId <= 0)
                return Error("Valid UserId is required");

            var account = await _payoutRepo.GetByUserIdAsync(userId);

            if (account == null)
                return Error("Payout account not found", 404);

            return Success("Payout account fetched successfully", account);
        }
    }
}