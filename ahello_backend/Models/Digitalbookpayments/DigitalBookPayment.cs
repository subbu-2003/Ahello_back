namespace ahello_backend.Models.Digitalbookpayments
{
    public class DigitalBookPayment
    {
        public int DigitalBookPaymentId { get; set; }
        public int DigitalBookingId { get; set; }
        public int UserId { get; set; }
        public int ClientId { get; set; }
        public string? RazorpayAccountId { get; set; }
        public string? RazorpayOrderId { get; set; }
        public string? RazorpayPaymentId { get; set; }
        public string? RazorpaySignature { get; set; }
        public string? RazorpayTransferId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal ExpertAmount { get; set; }
        public string? Currency { get; set; }
        public string? Status { get; set; }
        public string? OrderResponseJson { get; set; }
        public string? VerifyResponseJson { get; set; }
        public string? TransferResponseJson { get; set; }
        public string? ReleaseResponseJson { get; set; }
        public string? RefundResponseJson { get; set; }
        public string? FailureReason { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? HeldAt { get; set; }
        public DateTime? ReleasedAt { get; set; }
        public DateTime? RefundedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
