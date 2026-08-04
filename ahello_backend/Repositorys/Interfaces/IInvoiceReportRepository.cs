using ahello_backend.Models.Invoice_reports;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IInvoiceReportRepository
    {
        Task<InvoiceReportResponseModel> GetDatewiseAsync(InvoiceReportRequestModel request);

        Task<InvoiceReportResponseModel> GetMonthwiseAsync(InvoiceReportRequestModel request);

        Task<InvoiceReportResponseModel> GetYearwiseAsync(InvoiceReportRequestModel request);

        Task<byte[]> ExportDatewiseExcelAsync(InvoiceReportRequestModel request);

        Task<byte[]> ExportMonthwiseExcelAsync(InvoiceReportRequestModel request);

        Task<byte[]> ExportYearwiseExcelAsync(InvoiceReportRequestModel request);
    }
}
