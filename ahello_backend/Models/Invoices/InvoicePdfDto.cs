namespace ahello_backend.Models.Invoices
{
    public class InvoicePdfDto
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }

        public DateTime ScheduleDate { get; set; }
        public TimeSpan StartTime { get; set; }

        public string ExpertName { get; set; } = string.Empty;
        public string ExpertEmail { get; set; } = string.Empty;
        public string ExpertMobile { get; set; } = string.Empty;

        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientMobile { get; set; } = string.Empty;

        public string ServiceTitle { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal ExpertAmount { get; set; }
    }
}
