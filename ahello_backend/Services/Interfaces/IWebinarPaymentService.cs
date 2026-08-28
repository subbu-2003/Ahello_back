using ahello_backend.Models.WebinarPayment;

namespace ahello_backend.Services.Interfaces
{
    public interface IWebinarPaymentService
    {
        Task<WebinarTimelinePagedResponseDto> GetTimelineByUserIdAsync(
            int userId,
            string? search,
            string? key,
            int pageNumber,
            int pageSize);
    }
}
