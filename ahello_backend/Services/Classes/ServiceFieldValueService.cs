using ahello_backend.Models.Service;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class ServiceFieldValueService : IServiceFieldValueService
    {
        private readonly IServiceFieldValueRepository _repository;

        public ServiceFieldValueService(
            IServiceFieldValueRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(CreateServiceFieldValue model)
        {
            return await _repository.CreateAsync(model);
        }

        public async Task<bool> UpdateAsync(UpdateServiceFieldValue model)
        {
            return await _repository.UpdateAsync(model);
        }

        public async Task<IEnumerable<ServiceFieldValue>> GetByServiceAsync(int serviceId)
        {
            return await _repository.GetByServiceAsync(serviceId);
        }

        public async Task<ServiceFieldValue> GetByIdAsync(int serviceFieldValueId)
        {
            return await _repository.GetByIdAsync(serviceFieldValueId);
        }
    }
}