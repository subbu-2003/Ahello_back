using ahello_backend.Models.Payment;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;
using System.Text.Json;

namespace ahello_backend.Services.Classes
{
    public class EscrowPaymentService : IEscrowPaymentService
    {
        private readonly IEscrowPaymentRepository _escrowRepo;
        private readonly IRazorpayService _razorpayService;
        public EscrowPaymentService(IEscrowPaymentRepository escrowRepo, IRazorpayService razorpayService)
        {
            _escrowRepo = escrowRepo;
            _razorpayService = razorpayService;
        }

        public async Task<IEnumerable<EscrowTimelineResponseDto>> GetTimelineByUserIdAsync(int userId)
        {
            var rows = (await _escrowRepo.GetEscrowDetailsByUserIdAsync(userId)).ToList();

            foreach (var row in rows)
            {
                if (!string.IsNullOrWhiteSpace(row.RazorpayTransferId))
                {
                    var latestTransferJson =
                        await _razorpayService.GetTransferAsync(row.RazorpayTransferId);

                    row.TransferResponseJson = latestTransferJson;

                    await _escrowRepo.UpdateTransferResponseAsync(
                        row.EscrowPaymentId,
                        latestTransferJson);
                }
            }

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
                ServiceName = row.ServiceTitle,
                RazorpayOrderId = row.RazorpayOrderId,
                RazorpayPaymentId = row.RazorpayPaymentId,
                RazorpayTransferId = row.RazorpayTransferId,
                TotalAmount = row.TotalAmount,
                PlatformFee = row.PlatformFee,
                ExpertAmount = row.ExpertAmount,
                Currency = row.Currency,
                Status = row.Status
            };

            // Get latest Razorpay transfer information
            var (
                onHold,
                settlementStatus,
                processedAt,
                settlementId
            ) = ParseTransferInfo(row.TransferResponseJson);

            bool isFailed =
                row.Status?.Equals(
                    "FAILED",
                    StringComparison.OrdinalIgnoreCase) == true;

            bool isRefunded =
                row.Status?.Equals(
                    "REFUNDED",
                    StringComparison.OrdinalIgnoreCase) == true;


            // ============================================================
            // 1. PAYMENT CREATED
            // ============================================================

            dto.Timeline.Add(new TimelineStepDto
            {
                Key = "PAYMENT_CREATED",
                Label = "Payment created",
                Status = "completed",
                Timestamp = row.CreatedAt
            });


            // ============================================================
            // 2. PAYMENT AUTHORIZED
            // ============================================================

            dto.Timeline.Add(new TimelineStepDto
            {
                Key = "PAYMENT_AUTHORIZED",
                Label = "Payment authorized",

                Status = row.PaidAt != null
                    ? "completed"
                    : (isFailed ? "failed" : "pending"),

                Timestamp = row.PaidAt
            });


            // ============================================================
            // 3. PAYMENT CAPTURED
            // ============================================================

            dto.Timeline.Add(new TimelineStepDto
            {
                Key = "PAYMENT_CAPTURED",
                Label = "Payment captured",

                Status = row.VerifyResponseJson != null
                    ? "completed"
                    : (isFailed ? "failed" : "pending"),

                Timestamp = row.PaidAt
            });


            // ============================================================
            // 4. FUNDS HELD IN ESCROW
            // ============================================================

            dto.Timeline.Add(new TimelineStepDto
            {
                Key = "HELD_IN_ESCROW",
                Label = "Funds held in escrow",

                Status = row.HeldAt != null
                    ? "completed"
                    : (isFailed ? "failed" : "pending"),

                Timestamp = row.HeldAt
            });


            // ============================================================
            // 5. TRANSFER PROCESSED
            //
            // Razorpay:
            // processed_at = transfer processed timestamp
            // ============================================================

            string transferStatus;

            if (processedAt != null)
            {
                transferStatus = "completed";
            }
            else if (onHold)
            {
                transferStatus = "on_hold";
            }
            else
            {
                transferStatus = "pending";
            }

            dto.Timeline.Add(new TimelineStepDto
            {
                Key = "TRANSFER_PROCESSED",
                Label = "Transfer processed",
                Status = transferStatus,
                Timestamp = processedAt,

                Note = onHold
                    ? "Transfer is on hold until release conditions are met"
                    : processedAt == null
                        ? "Transfer is being processed"
                        : null
            });


            // ============================================================
            // 6. SETTLEMENT INITIATED
            //
            // This is NOT the same as processed_at.
            //
            // We use transfer processed time as the timeline event time
            // for when settlement processing starts.
            // ============================================================

            bool settlementInitiated =
                processedAt != null &&
                !string.IsNullOrWhiteSpace(settlementStatus);

            dto.Timeline.Add(new TimelineStepDto
            {
                Key = "SETTLEMENT_INITIATED",
                Label = "Settlement initiated",

                Status = settlementInitiated
                    ? "completed"
                    : "pending",

                Timestamp = settlementInitiated
                    ? processedAt
                    : null,

                Note = settlementInitiated
                    ? settlementId != null
                        ? $"Settlement ID: {settlementId}"
                        : null
                    : "Settlement will be initiated after transfer processing"
            });


            // ============================================================
            // 7. ON THE WAY TO BANK ACCOUNT
            //
            // settlement_status = pending
            // ============================================================

            bool settlementPending =
                string.Equals(
                    settlementStatus,
                    "pending",
                    StringComparison.OrdinalIgnoreCase);

            bool settlementCompleted =
                string.Equals(
                    settlementStatus,
                    "processed",
                    StringComparison.OrdinalIgnoreCase);


            if (settlementPending)
            {
                dto.Timeline.Add(new TimelineStepDto
                {
                    Key = "SETTLEMENT_PENDING",
                    Label = "On the way to your bank account",
                    Status = "pending",
                    Timestamp = null,

                    Note = "Settlement status: Pending. Expect deposit according to Razorpay settlement cycle"
                });
            }
            else if (settlementCompleted)
            {
                dto.Timeline.Add(new TimelineStepDto
                {
                    Key = "SETTLEMENT_COMPLETED",
                    Label = "Settlement completed",
                    Status = "completed",

                    // Do not use processedAt as actual bank settlement time.
                    // Keep null unless Razorpay provides an actual settlement timestamp.
                    Timestamp = null,

                    Note = settlementId != null
                        ? $"Settlement ID: {settlementId}"
                        : "Funds have been settled to the bank account"
                });
            }
            else
            {
                dto.Timeline.Add(new TimelineStepDto
                {
                    Key = "SETTLEMENT_PENDING",
                    Label = "On the way to your bank account",
                    Status = "pending",
                    Timestamp = null,

                    Note = "Settlement status: Pending. Expect deposit according to Razorpay settlement cycle"
                });
            }


            // ============================================================
            // 8. REFUND / RELEASE
            // ============================================================

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

                    Status = row.ReleasedAt != null
                        ? "completed"
                        : "upcoming",

                    Timestamp = row.ReleasedAt
                });
            }

            return dto;
        }

        // Handles both TransferResponseJson shapes seen in the DB:
        // 1) a raw transfer object  2) {"entity":"collection","items":[{...}]}
        private (
        bool onHold,
        string? settlementStatus,
        DateTime? processedAt,
        string? settlementId) ParseTransferInfo(string? json)
            {
            if (string.IsNullOrWhiteSpace(json))
                return (false, null, null, null);

            try
            {
                using var doc = JsonDocument.Parse(json);

                var root = doc.RootElement;

                JsonElement transfer = root;

                // Handle collection response
                if (root.TryGetProperty("items", out var items) &&
                    items.ValueKind == JsonValueKind.Array &&
                    items.GetArrayLength() > 0)
                {
                    transfer = items[0];
                }

                bool onHold =
                    transfer.TryGetProperty("on_hold", out var oh) &&
                    oh.ValueKind == JsonValueKind.True;

                string? settlementStatus =
                    transfer.TryGetProperty("settlement_status", out var ss) &&
                    ss.ValueKind == JsonValueKind.String
                        ? ss.GetString()
                        : null;

                string? settlementId =
                    transfer.TryGetProperty("recipient_settlement_id", out var rsid) &&
                    rsid.ValueKind == JsonValueKind.String
                        ? rsid.GetString()
                        : null;

                DateTime? processedAt = null;

                if (transfer.TryGetProperty("processed_at", out var pa) &&
                    pa.ValueKind == JsonValueKind.Number)
                {
                    var unix = pa.GetInt64();

                    if (unix > 0)
                    {
                        processedAt =
                            DateTimeOffset
                                .FromUnixTimeSeconds(unix)
                                .LocalDateTime;
                    }
                }

                return (
                    onHold,
                    settlementStatus,
                    processedAt,
                    settlementId
                );
            }
            catch
            {
                return (false, null, null, null);
            }
        }
    }
}