using ahello_backend.Models.Service_reports;

namespace ahello_backend.Models.Invoice_reports
{
    public class InvoiceReportResponseModel
    {
        public InvoiceReportSummaryModel? Summary { get; set; }

        public List<InvoiceStatusSummaryModel>? StatusSummary { get; set; }

        public List<InvoiceMonthlyBreakdownModel>? MonthlyBreakdown { get; set; }

        public List<InvoiceReportItemModel> Data { get; set; } = new();

        public PaginationModel Pagination { get; set; } = new();
    }
}
