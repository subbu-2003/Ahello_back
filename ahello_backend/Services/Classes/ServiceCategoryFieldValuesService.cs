using ahello_backend.Models.Service;
using ahello_backend.Repositorys.Classes;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class ServiceCategoryFieldValuesService : IServiceCategoryFieldValuesService
    {
        private readonly IServiceCategoryFieldValuesRepository _repository;

        public ServiceCategoryFieldValuesService(
            IServiceCategoryFieldValuesRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> AddAsync(ServiceCategoryFieldValues model)
        {
            return await _repository.AddAsync(model);
        }

        public async Task<int> UpdateAsync(ServiceCategoryFieldValues model)
        {
            return await _repository.UpdateAsync(model);
        }

        public async Task<int> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<ServiceCategoryFieldValues?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<ServiceCategoryFieldValues>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}
