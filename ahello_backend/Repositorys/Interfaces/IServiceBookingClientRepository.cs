using ahello_backend.Models.Pagination;
using ahello_backend.Models.ServiceBookingClientResponse;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IServiceBookingClientRepository
    {
        Task<PagedResult<ServiceBookingClientResponse>> GetClientsByUserIdAsync(
            int userId,
            int pageNumber,
            int pageSize,
            string? search);
    }
}