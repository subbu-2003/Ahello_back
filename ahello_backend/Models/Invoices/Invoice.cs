namespace ahello_backend.Models.Invoices
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public int BookingId { get; set; }
        public int EscrowPaymentId { get; set; }
        public int UserId { get; set; }        // expert
        public int ClientId { get; set; }
        public int ServiceId { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal ExpertAmount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }     // Issued / Cancelled
        public DateTime IssuedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
    }
}
