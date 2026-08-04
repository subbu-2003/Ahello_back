namespace ahello_backend.Models.Invoice_reports
{
    public class InvoiceReportSummaryModel
    {
        public int TotalInvoices { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal TotalPlatformFee { get; set; }

        public decimal TotalTaxAmount { get; set; }

        public decimal TotalExpertAmount { get; set; }
    }
}
