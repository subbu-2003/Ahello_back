using ahello_backend.Models.Service;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class ServiceDynamicService : IServiceDynamicService
    {
        private readonly IServiceDynamicRepository _repository;

        public ServiceDynamicService(IServiceDynamicRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ServiceDynamicGetResponse>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<ServiceDynamicGetResponse> GetByIdAsync(int serviceId)
        {
            return await _repository.GetByIdAsync(serviceId);
        }
        // ServiceDynamicService.cs

        public async Task<PagedServiceDynamicResponse> GetByUserIdPagedAsync(
            int userId,
            int pageNumber,
            int pageSize,
            string? search = null)
        {
            return await _repository.GetByUserIdPagedAsync(
                userId,
                pageNumber,
                pageSize,
                search);
        }
        public async Task<int> CreateAsync(ServiceDynamicPost model)
        {
            return await _repository.CreateAsync(model);
        }

        public async Task<bool> UpdateAsync(int serviceId, ServiceDynamicPut model)
        {
            return await _repository.UpdateAsync(serviceId, model);
        }

        public async Task<bool> DeleteAsync(int serviceId)
        {
            return await _repository.DeleteAsync(serviceId);
        }

        public async Task<PagedServiceDynamicResponse> GetAllPagedAsync(
            int pageNumber,
            int pageSize,
            string? search = null)
        {
            return await _repository.GetAllPagedAsync(
                pageNumber,
                pageSize,
                search);
        }
    }
}