using ahello_backend.Models.Pagination;
using ahello_backend.Models.Servicetype;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class ServiceTypeService : IServiceTypeService
    {
        private readonly IServiceTypeRepository _repository;

        public ServiceTypeService(
            IServiceTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ServiceType>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }
        public async Task<PagedResult<ServiceType>> GetPagedAsync(
        int pageNumber,int pageSize,string? search,
        DateTime? createdDate)
        {
            return await _repository.GetPagedAsync(
                pageNumber,
                pageSize,
                search,
                createdDate);
        }
        public async Task<ServiceType> GetByIdAsync(int serviceTypeId)
        {
            return await _repository.GetByIdAsync(serviceTypeId);
        }

        public async Task<int> CreateAsync(ServiceTypePost model)
        {
            return await _repository.CreateAsync(model);
        }

        public async Task<bool> UpdateAsync(
            int serviceTypeId,
            ServiceTypePut model)
        {
            return await _repository.UpdateAsync(
                serviceTypeId,
                model);
        }

        public async Task<bool> DeleteAsync(int serviceTypeId)
        {
            return await _repository.DeleteAsync(serviceTypeId);
        }
        public async Task<bool> UpdateStatusAsync(int serviceTypeId,ServiceTypeStatusUpdate model)
        {
            return await _repository.UpdateStatusAsync(
                serviceTypeId,
                model);
        }
    }
}