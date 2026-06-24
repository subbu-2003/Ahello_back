using ahello_backend.Models.Service;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class ServiceFieldService : IServiceFieldService
    {
        private readonly IServiceFieldRepository _repository;

        public ServiceFieldService(IServiceFieldRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(CreateServiceField model)
        {
            return await _repository.CreateAsync(model);
        }

        public async Task<bool> UpdateAsync(UpdateServiceField model)
        {
            return await _repository.UpdateAsync(model);
        }

        public async Task<IEnumerable<ServiceField>> GetByServiceAsync(int serviceId)
        {
            return await _repository.GetByServiceAsync(serviceId);
        }

        public async Task<ServiceField> GetByIdAsync(int serviceFieldId)
        {
            return await _repository.GetByIdAsync(serviceFieldId);
        }
        public async Task<IEnumerable<ServiceField>> GetByUserAsync(string userId)
        {
            return await _repository.GetByUserAsync(userId);
        }
        public async Task<IEnumerable<ServiceField>> GetByUserAllAsync(string userId)
        {
            return await _repository.GetByUserAllAsync(userId);
        }
        public async Task<bool> DeleteAsync(int serviceFieldId, string modifiedBy)
        {
            return await _repository.DeleteAsync(serviceFieldId, modifiedBy);
        }
    }
}