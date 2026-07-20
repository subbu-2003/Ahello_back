using ahello_backend.Models.Payment;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;
using System.Text.Json;

namespace ahello_backend.Services.Classes
{
    public class EscrowPaymentService : IEscrowPaymentService
    {
        private readonly IEscrowPaymentRepository _escrowRepo;

        public EscrowPaymentService(IEscrowPaymentRepository escrowRepo)
        {
            _escrowRepo = escrowRepo;
        }

        public async Task<IEnumerable<EscrowTimelineResponseDto>> GetTimelineByUserIdAsync(int userId)
        {
            var rows = await _escrowRepo.GetEscrowDetailsByUserIdAsync(userId);
            return rows.Select(BuildTimelineDto);
        }

        private EscrowTimelineResponseDto BuildTimelineDto(EscrowPaymentDetails row)
        {
            var dto = new EscrowTimelineResponseDto
            {
                EscrowPaymentId = row.EscrowPaymentId,
                BookingId = row.BookingId,
                UserId = row.UserId,
                ClientId = row.ClientId,
                ServiceName = row.ServiceName,
                RazorpayOrderId = row.RazorpayOrderId,
                RazorpayPaymentId = row.RazorpayPaymentId,
                RazorpayTransferId = row.RazorpayTransferId,
                TotalAmount = row.TotalAmount,
                PlatformFee = row.PlatformFee,
                ExpertAmount = row.ExpertAmount,
                Currency = row.Currency,
                Status = row.Status
            };

            var (onHold, settlementStatus, processedAt) = ParseTransferInfo(row.TransferResponseJson);
            bool isFailed = row.Status?.Equals("FAILED", StringComparison.OrdinalIgnoreCase) == true;
            bool isRefunded = row.Status?.Equals("REFUNDED", StringComparison.OrdinalIgnoreCase) == true;

            dto.Timeline.Add(new TimelineStepDto
            {
                Key = "PAYMENT_CREATED",
                Label = "Payment created",
                Status = "completed",
                Timestamp = row.CreatedAt
            });

            dto.Timeline.Add(new TimelineStepDto
            {
                Key = "PAYMENT_AUTHORIZED",
                Label = "Payment authorized",
                Status = row.PaidAt != null ? "completed" : (isFailed ? "failed" : "pending"),
                Timestamp = row.PaidAt
            });

            dto.Timeline.Add(new TimelineStepDto
            {
                Key = "PAYMENT_CAPTURED",
                Label = "Payment captured",
                Status = row.VerifyResponseJson != null ? "completed" : (isFailed ? "failed" : "pending"),
                Timestamp = row.PaidAt
            });

            dto.Timeline.Add(new TimelineStepDto
            {
                Key = "HELD_IN_ESCROW",
                Label = "Funds held in escrow",
                Status = row.HeldAt != null ? "completed" : (isFailed ? "failed" : "pending"),
                Timestamp = row.HeldAt
            });

            dto.Timeline.Add(new TimelineStepDto
            {
                Key = "TRANSFER_TO_EXPERT",
                Label = "Transfer to expert initiated",
                Status = row.RazorpayTransferId == null
                    ? "pending"
                    : (onHold ? "on_hold" : "completed"),
                Timestamp = row.HeldAt,
                Note = onHold ? "On hold until release conditions are met" : null
            });

            dto.Timeline.Add(new TimelineStepDto
            {
                Key = "SETTLEMENT",
                Label = "Settlement (to be processed)",
                Status = processedAt != null ? "completed" : "pending",
                Timestamp = processedAt,
                Note = processedAt == null ? "Will be processed after transfer is released" : null
            });

            if (isRefunded)
            {
                dto.Timeline.Add(new TimelineStepDto
                {
                    Key = "REFUNDED",
                    Label = "Refunded to client",
                    Status = "completed",
                    Timestamp = row.RefundedAt
                });
            }
            else
            {
                dto.Timeline.Add(new TimelineStepDto
                {
                    Key = "RELEASED",
                    Label = "Released to expert",
                    Status = row.ReleasedAt != null ? "completed" : "upcoming",
                    Timestamp = row.ReleasedAt
                });
            }

            return dto;
        }

        // Handles both TransferResponseJson shapes seen in the DB:
        // 1) a raw transfer object  2) {"entity":"collection","items":[{...}]}
        private (bool onHold, string? settlementStatus, DateTime? processedAt) ParseTransferInfo(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return (false, null, null);

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                JsonElement transfer = root;
                if (root.TryGetProperty("items", out var items) &&
                    items.ValueKind == JsonValueKind.Array &&
                    items.GetArrayLength() > 0)
                {
                    transfer = items[0];
                }

                bool onHold = transfer.TryGetProperty("on_hold", out var oh) &&
                              oh.ValueKind == JsonValueKind.True;

                string? settlementStatus = transfer.TryGetProperty("settlement_status", out var ss) &&
                                            ss.ValueKind == JsonValueKind.String
                    ? ss.GetString()
                    : null;

                DateTime? processedAt = null;
                if (transfer.TryGetProperty("processed_at", out var pa) &&
                    pa.ValueKind == JsonValueKind.Number)
                {
                    var unix = pa.GetInt64();
                    if (unix > 0)
                        processedAt = DateTimeOffset.FromUnixTimeSeconds(unix).LocalDateTime;
                }

                return (onHold, settlementStatus, processedAt);
            }
            catch
            {
                // malformed/unexpected JSON — fall back to pending state, don't crash the response
                return (false, null, null);
            }
        }
    }
}