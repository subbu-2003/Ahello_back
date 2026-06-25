namespace ahello_backend.Models.Payment
{
    public class EscrowPaymentLog
    {
        public int EscrowPaymentLogId { get; set; }

        public int? EscrowPaymentId { get; set; }
        public int? BookingId { get; set; }

        public string Action { get; set; } = string.Empty;
        public string? Status { get; set; }

        public string? RequestJson { get; set; }
        public string? ResponseJson { get; set; }
        public string? ErrorMessage { get; set; }

        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }
}