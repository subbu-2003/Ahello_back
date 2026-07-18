namespace ahello_backend.Models.Payment
{
    public class TimelineStepDto
    {
        public string Key { get; set; } = string.Empty;      // PAYMENT_CREATED, HELD_IN_ESCROW, etc.
        public string Label { get; set; } = string.Empty;     // "Payment created"
        public string Status { get; set; } = "upcoming";      // completed | on_hold | pending | upcoming | failed
        public DateTime? Timestamp { get; set; }
        public string? Note { get; set; }
    }

    public class EscrowTimelineResponseDto
    {
        public int EscrowPaymentId { get; set; }
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int ClientId { get; set; }
        public string ServiceName { get; set; } = string.Empty;

        public string? RazorpayOrderId { get; set; }
        public string? RazorpayPaymentId { get; set; }
        public string? RazorpayTransferId { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal ExpertAmount { get; set; }
        public string Currency { get; set; } = "INR";
        public string Status { get; set; } = string.Empty;

        public List<TimelineStepDto> Timeline { get; set; } = new();
    }
}
