using ahello_backend.Models.Digitalbookpayments;
using ahello_backend.Models.Payment;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;
using System.Text.Json;

namespace ahello_backend.Services.Classes
{
    public class DigitalBookPaymentService : IDigitalBookPaymentService
    {
        private readonly IDigitalBookPaymentRepository _digitalBookPaymentRepo;
        private readonly IRazorpayService _razorpayService;

        public DigitalBookPaymentService(
            IDigitalBookPaymentRepository digitalBookPaymentRepo,
            IRazorpayService razorpayService)
        {
            _digitalBookPaymentRepo = digitalBookPaymentRepo;
            _razorpayService = razorpayService;
        }

        public async Task<DigitalBookPaymentTimelinePagedResponseDto>
            GetTimelineByUserIdAsync(
                int userId,
                string? search,
                string? key,
                int pageNumber,
                int pageSize)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 10;

            if (pageSize > 100)
                pageSize = 100;

            var validTimelineKeys = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
    {
        "PAYMENT_CREATED",
        "PAYMENT_AUTHORIZED",
        "PAYMENT_CAPTURED",
        "HELD_IN_ESCROW",
        "TRANSFER_PROCESSED",
        "SETTLEMENT_INITIATED",
        "SETTLEMENT_PENDING",
        "SETTLEMENT_COMPLETED",
        "RELEASED",
        "REFUNDED"
    };

            if (!string.IsNullOrWhiteSpace(key) &&
                !validTimelineKeys.Contains(key))
            {
                throw new ArgumentException(
                    $"Invalid timeline key: {key}");
            }

            var result =
                await _digitalBookPaymentRepo.GetDigitalBookPaymentDetailsByUserIdAsync(
                    userId,
                    search,
                    null,
                    pageNumber,
                    pageSize);

            var rows = result.Data.ToList();

            foreach (var row in rows)
            {
                if (!string.IsNullOrWhiteSpace(row.RazorpayTransferId))
                {
                    var latestTransferJson =
                        await _razorpayService.GetTransferAsync(
                            row.RazorpayTransferId);

                    row.TransferResponseJson =
                        latestTransferJson;

                    await _digitalBookPaymentRepo.UpdateTransferResponseAsync(
                        row.DigitalBookPaymentId,
                        latestTransferJson);
                }
            }

            var timelineData =
                rows
                    .Select(BuildTimelineDto)
                    .ToList();

            if (!string.IsNullOrWhiteSpace(key))
            {
                timelineData = timelineData
                    .Select(dto =>
                    {
                        dto.Timeline = dto.Timeline
                            .Where(t =>
                                string.Equals(
                                    t.Key,
                                    key,
                                    StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        return dto;
                    })
                    .Where(dto => dto.Timeline.Any())
                    .ToList();
            }

            return new DigitalBookPaymentTimelinePagedResponseDto
            {
                Data = timelineData,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = result.TotalRecords,
                TotalPages = (int)Math.Ceiling(
                    result.TotalRecords /
                    (double)pageSize)
            };
        }

        private DigitalBookPaymentTimelineResponseDto BuildTimelineDto(DigitalBookPaymentDetails row)
        {
            var dto = new DigitalBookPaymentTimelineResponseDto
            {
                DigitalBookPaymentId = row.DigitalBookPaymentId,
                DigitalBookId = row.DigitalBookId,
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

                Status = row.PaidAt != null
                    ? "completed"
                    : (isFailed ? "failed" : "pending"),

                Timestamp = row.PaidAt
            });

            dto.Timeline.Add(new TimelineStepDto
            {
                Key = "PAYMENT_CAPTURED",
                Label = "Payment captured",

                Status = row.VerifyResponseJson != null
                    ? "completed"
                    : (isFailed ? "failed" : "pending"),

                Timestamp = row.PaidAt
            });

            dto.Timeline.Add(new TimelineStepDto
            {
                Key = "HELD_IN_ESCROW",
                Label = "Funds held in escrow",

                Status = row.HeldAt != null
                    ? "completed"
                    : (isFailed ? "failed" : "pending"),

                Timestamp = row.HeldAt
            });

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