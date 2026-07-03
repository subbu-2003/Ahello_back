using ahello_backend.DTO.Payment;
using ahello_backend.Services.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ahello_backend.Services.Classes
{
    public class RazorpayService : IRazorpayService
    {
        private readonly HttpClient _http;
        private readonly string _keyId;
        private readonly string _keySecret;

        public RazorpayService(IConfiguration config, HttpClient http)
        {
            _http = http;

            _keyId = config["Razorpay:Key"] ?? "";
            _keySecret = config["Razorpay:Secret"] ?? "";

            var credentials = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_keyId}:{_keySecret}")
            );

            _http.BaseAddress = new Uri("https://api.razorpay.com/");
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);
        }

        // Helper: throws with actual Razorpay error body
        private static async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Razorpay {(int)response.StatusCode}: {body}");
            }
        }


        private static (string category, string subcategory) GetRazorpayCategory(string categoryName)
        {
            return categoryName?.Trim().ToLower() switch
            {
                "doctors" => ("healthcare", "clinic"),

                "trainers" => ("education", "coaching"),
                "professor" => ("education", "coaching"),
                "coach" => ("education", "coaching"),

                "consultation" => ("services", "consulting"),
                "business" => ("services", "consulting"),
                "entrepreneur" => ("services", "consulting"),
                "entrepreneurs" => ("services", "consulting"),
                "influencer" => ("services", "consulting"),

                _ => ("services", "consulting")
            };
        }

        public async Task<(string accountId, string responseJson)> CreateLinkedAccountAsync(
    CreateLinkedAccountDto dto,
    dynamic user)
        {
            string ahlloCategory = Convert.ToString(user.CategoryName) ?? "";

            var razorpayCategory = GetRazorpayCategory(ahlloCategory);

            var payload = new
            {
                email = user.Email,
                phone = user.MobileNumber,
                type = "route",
                reference_id = $"expert_{user.UserId}",
                legal_business_name = user.FullName,

                customer_facing_business_name =
                    string.IsNullOrWhiteSpace(dto.CustomerFacingBusinessName)
                        ? user.FullName
                        : dto.CustomerFacingBusinessName,

                business_type =
                    string.IsNullOrWhiteSpace(dto.BusinessType)
                        ? "individual"
                        : dto.BusinessType.Trim().ToLower(),

                contact_name = user.FullName,

                profile = new
                {
                    category = razorpayCategory.category,
                    subcategory = razorpayCategory.subcategory,
                    addresses = new
                    {
                        registered = new
                        {
                            street1 = user.Address,
                            street2 = "NA",
                            city = user.City,
                            state = user.State,
                            postal_code = user.Pincode,
                            country = "IN"
                        }
                    }
                },

                notes = new
                {
                    expert_id = Convert.ToString(user.UserId),
                    ahllo_category = ahlloCategory
                }
            };

            var response = await _http.PostAsJsonAsync("v2/accounts", payload);
            var json = await response.Content.ReadAsStringAsync();

            await EnsureSuccessAsync(response);

            var data = JsonSerializer.Deserialize<JsonElement>(json);
            var accountId = data.GetProperty("id").GetString() ?? "";

            return (accountId, json);
        }

        public async Task<(string stakeholderId, string responseJson)> CreateStakeholderAsync(
            string accountId,
            CreateStakeholderDto dto,
            dynamic user)
        {
            var payload = new
            {
                name = user.FullName,
                email = user.Email,
                phone = new
                {
                    primary = user.MobileNumber
                },

                relationship = new
                {
                    director = true,
                    executive = true
                },

                addresses = new
                {
                    residential = new
                    {
                        street = user.Address ?? "NA",
                        city = user.City ?? "NA",
                        state = user.State ?? "NA",
                        postal_code = user.Pincode ?? "000000",
                        country = "IN"
                    }
                }
            };

            var response = await _http.PostAsJsonAsync(
                $"v2/accounts/{accountId}/stakeholders",
                payload);

            var json = await response.Content.ReadAsStringAsync();

            await EnsureSuccessAsync(response);

            var data = JsonSerializer.Deserialize<JsonElement>(json);
            var stakeholderId = data.GetProperty("id").GetString() ?? "";

            return (stakeholderId, json);
        }

        public async Task<(string productId, string responseJson)> RequestProductAsync(
            string accountId)
        {
            var payload = new
            {
                product_name = "route",
                tnc_accepted = true
            };

            var response = await _http.PostAsJsonAsync(
                $"v2/accounts/{accountId}/products",
                payload);

            var json = await response.Content.ReadAsStringAsync();

            await EnsureSuccessAsync(response);

            var data = JsonSerializer.Deserialize<JsonElement>(json);
            var productId = data.GetProperty("id").GetString() ?? "";

            return (productId, json);
        }

        public async Task<string> UpdateProductAsync(
            string accountId,
            string productId,
            UpdateProductDto dto)
        {
            var payload = new
            {
                settlements = new
                {
                    account_number = dto.AccountNumber,
                    ifsc_code = dto.IfscCode,
                    beneficiary_name = dto.BeneficiaryName
                },
                tnc_accepted = true
            };

            var response = await _http.PatchAsJsonAsync(
                $"v2/accounts/{accountId}/products/{productId}",
                payload);

            var json = await response.Content.ReadAsStringAsync();

            await EnsureSuccessAsync(response);

            return json;
        }

        public async Task<(string orderId, string responseJson)> CreateOrderAsync(
            decimal amount,
            string currency,
            string receipt)
        {
            var paise = (long)(amount * 100);

            var payload = new
            {
                amount = paise,
                currency = currency,
                receipt = receipt,
                payment_capture = 1
            };

            var response = await _http.PostAsJsonAsync("v1/orders", payload);
            var json = await response.Content.ReadAsStringAsync();

            await EnsureSuccessAsync(response);

            var data = JsonSerializer.Deserialize<JsonElement>(json);
            var orderId = data.GetProperty("id").GetString() ?? "";

            return (orderId, json);
        }

        public bool VerifySignature(
            string orderId,
            string paymentId,
            string signature)
        {
            var payload = $"{orderId}|{paymentId}";

            using var hmac = new HMACSHA256(
                Encoding.UTF8.GetBytes(_keySecret)
            );

            var hash = BitConverter
                .ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)))
                .Replace("-", "")
                .ToLower();

            return hash == signature;
        }

        public async Task<(string transferId, string responseJson)> CreateHeldTransferAsync(
            string paymentId,
            string accountId,
            decimal expertAmount)
        {
            var paise = (long)(expertAmount * 100);

            var payload = new
            {
                transfers = new[]
                {
                    new
                    {
                        account = accountId,
                        amount = paise,
                        currency = "INR",
                        on_hold = true
                    }
                }
            };

            var response = await _http.PostAsJsonAsync(
                $"v1/payments/{paymentId}/transfers",
                payload);

            var json = await response.Content.ReadAsStringAsync();

            await EnsureSuccessAsync(response);

            var data = JsonSerializer.Deserialize<JsonElement>(json);
            var transfer = data.GetProperty("items")[0];
            var transferId = transfer.GetProperty("id").GetString() ?? "";

            return (transferId, json);
        }

        public async Task<string> ReleaseTransferAsync(string transferId)
        {
            var payload = new { on_hold = false };

            var request = new HttpRequestMessage(
                HttpMethod.Patch,
                $"v1/transfers/{transferId}");

            request.Content = JsonContent.Create(payload);

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            await EnsureSuccessAsync(response);

            return json;
        }

        public async Task<string> ReverseTransferAsync(
            string transferId,
            decimal amount)
        {
            var paise = (long)(amount * 100);

            var payload = new { amount = paise };

            var response = await _http.PostAsJsonAsync(
                $"v1/transfers/{transferId}/reversals",
                payload);

            var json = await response.Content.ReadAsStringAsync();

            await EnsureSuccessAsync(response);

            return json;
        }

        public async Task<string> RefundPaymentAsync(
            string paymentId,
            decimal amount)
        {
            var paise = (long)(amount * 100);

            var payload = new { amount = paise };

            var response = await _http.PostAsJsonAsync(
                $"v1/payments/{paymentId}/refund",
                payload);

            var json = await response.Content.ReadAsStringAsync();

            await EnsureSuccessAsync(response);

            return json;
        }
    }
}