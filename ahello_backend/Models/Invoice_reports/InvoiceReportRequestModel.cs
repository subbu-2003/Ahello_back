namespace ahello_backend.Models.Invoice_reports
{
    public class InvoiceReportRequestModel
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public int? UserId { get; set; }

        public int? BookingId { get; set; }

        public string? Search { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
