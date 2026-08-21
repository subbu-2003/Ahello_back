using ahello_backend.DbContexts;
using ahello_backend.Models.Whatsapp;      // ← add this (has WhatsAppSettings)
using ahello_backend.Services.Interfaces;
using Dapper;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;             // ← add this (for MediaTypeHeaderValue)
using System.Text;
using System.Text.Json;                    // ← add this (has JsonSerializer)

namespace ahello_backend.Services.Classes
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _http;
        private readonly DbContext _db;
        private readonly WhatsAppSettings _settings;

        public WhatsAppService(HttpClient http, DbContext db, IOptions<WhatsAppSettings> settings)
        {
            _http = http;
            _db = db;
            _settings = settings.Value;
        }

        // ── Booking confirmation ─────────────────────────────────────────
        public async Task SendBookingConfirmationWhatsAppAsync(
            string? mobileNumber, string clientName, string serviceName,
            string date, string time, int bookingId)
        {
            var bodyParams = new[] { clientName, serviceName, date, time };
            await SendTemplateAsync(
                mobileNumber, _settings.TemplateBookingConfirmation, bodyParams,
                "BookingConfirmation", bookingId, clientName);
        }

        // ── Meeting invite ───────────────────────────────────────────────
        public async Task SendMeetingInviteWhatsAppAsync(
            string? mobileNumber, string clientName, string meetingLink, int bookingId)
        {
            var bodyParams = new[] { clientName, meetingLink };
            await SendTemplateAsync(
                mobileNumber, _settings.TemplateMeetingInvite, bodyParams,
                "MeetingInvite", bookingId, clientName);
        }

        // ── Meeting reminder ─────────────────────────────────────────────
        public async Task SendMeetingReminderWhatsAppAsync(
            string? mobileNumber, string clientName, string otherPersonName, string serviceName,
            DateTime startTime, string meetingLink, int minutesLeft, int bookingId)
        {
            var bodyParams = new[]
            {
                clientName,
                otherPersonName,
                serviceName,
                startTime.ToString("dd MMM yyyy hh:mm tt"),
                minutesLeft.ToString(),
                meetingLink
            };

            await SendTemplateAsync(
                mobileNumber, _settings.TemplateMeetingReminder, bodyParams,
                "MeetingReminder", bookingId, clientName);
        }

        // ── Reschedule confirmation ──────────────────────────────────────
        public async Task SendRescheduleConfirmationWhatsAppAsync(
            string? mobileNumber, string clientName, string serviceName,
            string date, string time, int bookingId)
        {
            var bodyParams = new[] { clientName, serviceName, date, time };
            await SendTemplateAsync(
                mobileNumber, _settings.TemplateReschedule, bodyParams,
                "Reschedule", bookingId, clientName);
        }

        // ── Core sender ───────────────────────────────────────────────────
        private async Task SendTemplateAsync(
            string? mobileNumber,
            string templateName,
            string[] bodyParams,
            string messageType,
            int bookingId,
            string? clientName)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
            {
                await LogAsync(bookingId, mobileNumber, clientName, messageType, templateName,
                    "Failed", null, null, "Mobile number is empty");
                return;
            }

            var digits = new string(mobileNumber.Where(char.IsDigit).ToArray());
            if (digits.Length < 10)
            {
                await LogAsync(bookingId, mobileNumber, clientName, messageType, templateName,
                    "Failed", null, null, $"Invalid mobile number: {mobileNumber}");
                return;
            }

            var to = digits.StartsWith("91") ? digits : "91" + digits;

            var payload = new
            {
                to,
                type = "template",
                template = new
                {
                    language = new { policy = "deterministic", code = "en" },
                    name = templateName,
                    components = new object[]
                    {
                        new
                        {
                            type = "body",
                            parameters = bodyParams.Select(p => new { type = "text", text = p ?? "-" }).ToArray()
                        }
                    }
                }
            };

            try
            {
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.ApiUrl}?token={_settings.Token}");
                request.Content = content;

                var response = await _http.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                var statusCode = (int)response.StatusCode;

                var status = response.IsSuccessStatusCode ? "Success" : "Failed";
                var error = response.IsSuccessStatusCode ? null : $"HTTP {statusCode}: {responseBody}";

                Console.WriteLine($"[WhatsApp] {status} → {to} | {messageType} | {statusCode} | {responseBody}");

                await LogAsync(bookingId, to, clientName, messageType, templateName,
                    status, statusCode, responseBody, error);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[WhatsApp] Send failed: {ex.Message}");
                await LogAsync(bookingId, to, clientName, messageType, templateName,
                    "Failed", null, null, ex.Message);
            }
        }

        // ── Log helper ────────────────────────────────────────────────────
        private async Task LogAsync(
            int bookingId,
            string? mobileNumber,
            string? clientName,
            string messageType,
            string? templateName,
            string status,
            int? responseCode,
            string? responseBody,
            string? errorMessage)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.ExecuteAsync(@"
                    INSERT INTO WhatsAppLog
                    (BookingId, MobileNumber, ClientName, MessageType, TemplateName,
                     Status, ResponseCode, ResponseBody, ErrorMessage, CreatedAt)
                    VALUES
                    (@BookingId, @MobileNumber, @ClientName, @MessageType, @TemplateName,
                     @Status, @ResponseCode, @ResponseBody, @ErrorMessage, NOW());",
                    new
                    {
                        BookingId = bookingId,
                        MobileNumber = mobileNumber,
                        ClientName = clientName,
                        MessageType = messageType,
                        TemplateName = templateName,
                        Status = status,
                        ResponseCode = responseCode,
                        ResponseBody = responseBody,
                        ErrorMessage = errorMessage,
                    });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[WhatsApp] Logging failed: {ex.Message}");
            }
        }
    }
}
