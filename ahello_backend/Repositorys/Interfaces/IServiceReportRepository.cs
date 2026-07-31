using ahello_backend.Models.Service_reports;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IServiceReportRepository
    {
        Task<ServiceReportResponseModel> GetServiceReportDatewiseAsync(ServiceReportRequestModel request);
        Task<ServiceReportResponseModel> GetServiceReportMonthwiseAsync(ServiceReportRequestModel request);
        Task<ServiceReportResponseModel> GetServiceReportYearwiseAsync(ServiceReportRequestModel request);
        Task<byte[]> ExportDatewiseExcelAsync(ServiceReportRequestModel request);

        Task<byte[]> ExportMonthwiseExcelAsync(ServiceReportRequestModel request);

        Task<byte[]> ExportYearwiseExcelAsync(ServiceReportRequestModel request);
    }
}
