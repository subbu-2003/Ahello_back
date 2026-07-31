using ahello_backend.Models.Service_reports;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class ServiceReportService : IServiceReportService
    {
        private readonly IServiceReportRepository _repository;

        public ServiceReportService(IServiceReportRepository repository)
        {
            _repository = repository;
        }

        public async Task<ServiceReportResponseModel> GetServiceReportDatewiseAsync(ServiceReportRequestModel request)
        {
            Normalize(request);
            return await _repository.GetServiceReportDatewiseAsync(request);
        }

        public async Task<ServiceReportResponseModel> GetServiceReportMonthwiseAsync(ServiceReportRequestModel request)
        {
            Normalize(request);
            return await _repository.GetServiceReportMonthwiseAsync(request);
        }

        public async Task<ServiceReportResponseModel> GetServiceReportYearwiseAsync(ServiceReportRequestModel request)
        {
            Normalize(request);
            return await _repository.GetServiceReportYearwiseAsync(request);
        }

        // Basic validation / defaulting shared by all three report types
        private void Normalize(ServiceReportRequestModel request)
        {
            if (request.UserId <= 0)
                throw new ArgumentException("UserId is required.");

            if (request.FromDate == default)
                request.FromDate = new DateTime(DateTime.UtcNow.Year, 1, 1);

            if (request.ToDate == default)
                request.ToDate = DateTime.UtcNow;

            if (request.FromDate > request.ToDate)
                throw new ArgumentException("FromDate cannot be later than ToDate.");

            if (request.PageNumber <= 0)
                request.PageNumber = 1;

            if (request.PageSize <= 0)
                request.PageSize = 10;
        }
        public async Task<byte[]> ExportDatewiseExcelAsync(ServiceReportRequestModel request)
        {
            Normalize(request);
            return await _repository.ExportDatewiseExcelAsync(request);
        }

        public async Task<byte[]> ExportMonthwiseExcelAsync(ServiceReportRequestModel request)
        {
            Normalize(request);
            return await _repository.ExportMonthwiseExcelAsync(request);
        }

        public async Task<byte[]> ExportYearwiseExcelAsync(ServiceReportRequestModel request)
        {
            Normalize(request);
            return await _repository.ExportYearwiseExcelAsync(request);
        }
    }
}
