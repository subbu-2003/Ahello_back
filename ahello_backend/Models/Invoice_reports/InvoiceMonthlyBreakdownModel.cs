namespace ahello_backend.Models.Invoice_reports
{
    public class InvoiceMonthlyBreakdownModel
    {
        public string Month { get; set; }

        public int InvoiceCount { get; set; }

        public decimal Revenue { get; set; }
    }
}
