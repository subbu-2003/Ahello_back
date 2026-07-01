using ahello_backend.Models.Pagination;
using ahello_backend.Models.ServiceBookingClientResponse;

namespace ahello_backend.Services.Interfaces
{
    public interface IServiceBookingClientService
    {
        Task<PagedResult<ServiceBookingClientResponse>> GetClientsByUserIdAsync(
            int userId,
            int pageNumber,
            int pageSize,
            string? search);
    }
}