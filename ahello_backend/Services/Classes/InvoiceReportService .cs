using ahello_backend.Models.Invoice_reports;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class InvoiceReportService : IInvoiceReportService
    {
        private readonly IInvoiceReportRepository _repository;

        public InvoiceReportService(IInvoiceReportRepository repository)
        {
            _repository = repository;
        }

        public async Task<InvoiceReportResponseModel> GetDatewiseAsync(InvoiceReportRequestModel request)
        {
            Normalize(request);
            return await _repository.GetDatewiseAsync(request);
        }

        public async Task<InvoiceReportResponseModel> GetMonthwiseAsync(InvoiceReportRequestModel request)
        {
            Normalize(request);
            return await _repository.GetMonthwiseAsync(request);
        }

        public async Task<InvoiceReportResponseModel> GetYearwiseAsync(InvoiceReportRequestModel request)
        {
            Normalize(request);
            return await _repository.GetYearwiseAsync(request);
        }

        public async Task<byte[]> ExportDatewiseExcelAsync(InvoiceReportRequestModel request)
        {
            Normalize(request);
            return await _repository.ExportDatewiseExcelAsync(request);
        }

        public async Task<byte[]> ExportMonthwiseExcelAsync(InvoiceReportRequestModel request)
        {
            Normalize(request);
            return await _repository.ExportMonthwiseExcelAsync(request);
        }

        public async Task<byte[]> ExportYearwiseExcelAsync(InvoiceReportRequestModel request)
        {
            Normalize(request);
            return await _repository.ExportYearwiseExcelAsync(request);
        }

        /// <summary>
        /// Common validation/default values for all invoice reports.
        /// </summary>
        private void Normalize(InvoiceReportRequestModel request)
        {
            if (request.FromDate == default)
                request.FromDate = new DateTime(DateTime.UtcNow.Year, 1, 1);

            if (request.ToDate == default)
                request.ToDate = DateTime.UtcNow;

            if (request.FromDate > request.ToDate)
                throw new ArgumentException("FromDate cannot be greater than ToDate.");

            if (request.PageNumber <= 0)
                request.PageNumber = 1;

            if (request.PageSize <= 0)
                request.PageSize = 10;

            // Optional filters
            if (request.UserId.HasValue && request.UserId <= 0)
                request.UserId = null;

            if (request.BookingId.HasValue && request.BookingId <= 0)
                request.BookingId = null;

            request.Search = string.IsNullOrWhiteSpace(request.Search)
                ? null
                : request.Search.Trim();
        }
    }
}
