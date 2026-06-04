using ahello_backend.Models.Service;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class ServiceCategoryDynamicService : IServiceCategoryDynamicService
    {
        private readonly IServiceCategoryDynamicRepository _repository;

        public ServiceCategoryDynamicService(
            IServiceCategoryDynamicRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> AddAsync(ServiceCategoryDynamic model)
        {
            return await _repository.AddAsync(model);
        }

        public async Task<int> UpdateAsync(ServiceCategoryDynamic model)
        {
            return await _repository.UpdateAsync(model);
        }

        public async Task<int> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<ServiceCategoryDynamic?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<ServiceCategoryDynamic>> GetAllAsync(
            int pageNumber,
            int pageSize)
        {
            return await _repository.GetAllAsync(pageNumber, pageSize);
        }
    }
}
