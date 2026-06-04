using ahello_backend.Models.Service;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IServiceDynamicFieldFormRepository
    {
        Task<ServiceDynamicFieldFormResponse>
            GetServiceDynamicFieldFormAsync(int serviceId);
    }
}
