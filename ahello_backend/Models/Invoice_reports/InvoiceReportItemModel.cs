namespace ahello_backend.Models.Invoice_reports
{
    public class InvoiceReportItemModel
    {
        public int InvoiceId { get; set; }

        public string InvoiceNumber { get; set; }

        public int BookingId { get; set; }

        public int UserId { get; set; }

        public int ClientId { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal PlatformFee { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal ExpertAmount { get; set; }

        public string Currency { get; set; }

        public string Status { get; set; }

        public DateTime IssuedAt { get; set; }
    }
}
