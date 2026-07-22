using ahello_backend.Models.Payment;

namespace ahello_backend.Services.Interfaces
{
    public interface IEscrowPaymentService
    {
        Task<EscrowTimelinePagedResponseDto> GetTimelineByUserIdAsync(
        int userId,
        string? search,
        string? key,
        int pageNumber,
        int pageSize);
    }
}
