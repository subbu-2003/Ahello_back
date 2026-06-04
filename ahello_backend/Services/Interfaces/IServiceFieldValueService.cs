using ahello_backend.Models.Service;

namespace ahello_backend.Services.Interfaces
{
    public interface IServiceFieldValueService
    {
        Task<int> CreateAsync(CreateServiceFieldValue model);

        Task<bool> UpdateAsync(UpdateServiceFieldValue model);

        Task<IEnumerable<ServiceFieldValue>> GetByServiceAsync(int serviceId);

        Task<ServiceFieldValue> GetByIdAsync(int serviceFieldValueId);
    }
}
