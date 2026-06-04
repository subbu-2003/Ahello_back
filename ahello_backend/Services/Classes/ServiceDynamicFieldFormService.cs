using ahello_backend.Models.Service;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class ServiceDynamicFieldFormService
        : IServiceDynamicFieldFormService
    {
        private readonly IServiceDynamicFieldFormRepository _repository;

        public ServiceDynamicFieldFormService(
            IServiceDynamicFieldFormRepository repository)
        {
            _repository = repository;
        }

        public async Task<ServiceDynamicFieldFormResponse>
            GetServiceDynamicFieldFormAsync(int serviceId)
        {
            return await _repository
                .GetServiceDynamicFieldFormAsync(serviceId);
        }
    }
}