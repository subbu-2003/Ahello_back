using ahello_backend.Models.Service;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class ServiceCategoryDropdownOptionService : IServiceCategoryDropdownOptionService
    {
        private readonly IServiceCategoryDropdownOptionRepository _repository;

        public ServiceCategoryDropdownOptionService(
            IServiceCategoryDropdownOptionRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> AddAsync(ServiceCategoryDropdownOption model)
        {
            return await _repository.AddAsync(model);
        }

        public async Task<int> UpdateAsync(ServiceCategoryDropdownOption model)
        {
            return await _repository.UpdateAsync(model);
        }

        public async Task<int> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<ServiceCategoryDropdownOption?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<ServiceCategoryDropdownOption>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}
