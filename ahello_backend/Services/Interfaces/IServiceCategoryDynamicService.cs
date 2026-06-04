using ahello_backend.Models.Service;

namespace ahello_backend.Services.Interfaces
{
    public interface IServiceCategoryDynamicService
    {
        Task<int> AddAsync(ServiceCategoryDynamic model);

        Task<int> UpdateAsync(ServiceCategoryDynamic model);

        Task<int> DeleteAsync(int id);

        Task<ServiceCategoryDynamic?> GetByIdAsync(int id);

        Task<IEnumerable<ServiceCategoryDynamic>> GetAllAsync(
            int pageNumber,
            int pageSize);
    }
}
