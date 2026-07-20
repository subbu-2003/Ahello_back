namespace ahello_backend.Models.Payment
{
    public class EscrowPaymentDetails
    {
        public int EscrowPaymentId { get; set; }
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int ClientId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;

        public string? RazorpayOrderId { get; set; }
        public string? RazorpayPaymentId { get; set; }
        public string? RazorpayTransferId { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal ExpertAmount { get; set; }
        public string Currency { get; set; } = "INR";
        public string Status { get; set; } = string.Empty;

        public string? VerifyResponseJson { get; set; }
        public string? TransferResponseJson { get; set; }
        public string? ReleaseResponseJson { get; set; }
        public string? RefundResponseJson { get; set; }

        public DateTime? PaidAt { get; set; }
        public DateTime? HeldAt { get; set; }
        public DateTime? ReleasedAt { get; set; }
        public DateTime? RefundedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
