using ahello_backend.Models.Pagination;
using ahello_backend.Models.ServiceBookingClientResponse;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class ServiceBookingClientService : IServiceBookingClientService
    {
        private readonly IServiceBookingClientRepository _repository;

        public ServiceBookingClientService(IServiceBookingClientRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResult<ServiceBookingClientResponse>> GetClientsByUserIdAsync(
            int userId,
            int pageNumber,
            int pageSize,
            string? search)
        {
            return await _repository.GetClientsByUserIdAsync(
                userId,
                pageNumber,
                pageSize,
                search);
        }
    }
}