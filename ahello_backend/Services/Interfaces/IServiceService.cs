using ahello_backend.Models.Service;

namespace ahello_backend.Services.Interfaces
{
    public interface IServiceService
    {
        Task<IEnumerable<Service>> GetAllAsync();

        Task<Service> GetByIdAsync(int serviceId);

        Task<int> CreateAsync(ServicePost model);

        Task<bool> UpdateAsync(
            int serviceId,
            ServicePut model);

        Task<bool> DeleteAsync(int serviceId);

        Task<IEnumerable<Service>> GetByServiceCategoryIdAsync(int serviceCategoryId);
    }
}
