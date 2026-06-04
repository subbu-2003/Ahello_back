using ahello_backend.Models.Service;

namespace ahello_backend.Services.Interfaces
{
    public interface IServiceDynamicFieldFormService
    {
        Task<ServiceDynamicFieldFormResponse>
            GetServiceDynamicFieldFormAsync(int serviceId);
    }
}
