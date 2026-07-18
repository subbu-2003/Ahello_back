using ahello_backend.Models.Payment;

namespace ahello_backend.Services.Interfaces
{
    public interface IEscrowPaymentService
    {
        Task<IEnumerable<EscrowTimelineResponseDto>> GetTimelineByUserIdAsync(int userId);
    }
}
