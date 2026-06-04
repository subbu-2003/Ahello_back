using ahello_backend.Models.Service;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _repository;

        public ServiceService(
            IServiceRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Service>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Service> GetByIdAsync(int serviceId)
        {
            return await _repository.GetByIdAsync(serviceId);
        }

        public async Task<int> CreateAsync(ServicePost model)
        {
            return await _repository.CreateAsync(model);
        }

        public async Task<bool> UpdateAsync(
            int serviceId,
            ServicePut model)
        {
            return await _repository.UpdateAsync(
                serviceId,
                model);
        }

        public async Task<bool> DeleteAsync(int serviceId)
        {
            return await _repository.DeleteAsync(serviceId);
        }
        public async Task<IEnumerable<Service>> GetByServiceCategoryIdAsync(
             int serviceCategoryId)
        {
            return await _repository.GetByServiceCategoryIdAsync(
                serviceCategoryId);
        }
    }
}