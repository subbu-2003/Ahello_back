using ahello_backend.Models.Service;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class ServiceCategoryFieldsService : IServiceCategoryFieldsService
    {
        private readonly IServiceCategoryFieldsRepository _repository;

        public ServiceCategoryFieldsService(
            IServiceCategoryFieldsRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> AddAsync(ServiceCategoryFields model)
        {
            return await _repository.AddAsync(model);
        }

        public async Task<int> UpdateAsync(ServiceCategoryFields model)
        {
            return await _repository.UpdateAsync(model);
        }

        public async Task<int> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<ServiceCategoryFields?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<ServiceCategoryFields>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}
