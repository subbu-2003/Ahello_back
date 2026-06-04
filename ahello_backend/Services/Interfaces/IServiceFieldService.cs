using ahello_backend.Models.Service;

namespace ahello_backend.Services.Interfaces
{
    public interface IServiceFieldService
    {
        Task<int> CreateAsync(CreateServiceField model);

        Task<bool> UpdateAsync(UpdateServiceField model);

        Task<IEnumerable<ServiceField>> GetByServiceAsync(int serviceId);

        Task<ServiceField> GetByIdAsync(int serviceFieldId);

        Task<bool> DeleteAsync(int serviceFieldId, string modifiedBy);
    }
}
