using ahello_backend.Models.Servicetype;

namespace ahello_backend.Services.Interfaces
{
    public interface IServiceTypeService
    {
        Task<IEnumerable<ServiceType>> GetAllAsync();

        Task<ServiceType> GetByIdAsync(int serviceTypeId);

        Task<int> CreateAsync(ServiceTypePost model);

        Task<bool> UpdateAsync(
            int serviceTypeId,
            ServiceTypePut model);

        Task<bool> DeleteAsync(int serviceTypeId);
    }
}
