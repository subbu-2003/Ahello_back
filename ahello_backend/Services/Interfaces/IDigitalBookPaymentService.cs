using ahello_backend.Models.Digitalbookpayments;

namespace ahello_backend.Services.Interfaces
{
    public interface IDigitalBookPaymentService
    {
        Task<DigitalBookPaymentTimelinePagedResponseDto> GetTimelineByUserIdAsync(
            int userId,
            string? search,
            string? key,
            int pageNumber,
            int pageSize);
    }
}
