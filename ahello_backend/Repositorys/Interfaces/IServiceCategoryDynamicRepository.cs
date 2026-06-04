using ahello_backend.Models.Service;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IServiceCategoryDynamicRepository
    {
        Task<int> AddAsync(ServiceCategoryDynamic model);
        Task<int> UpdateAsync(ServiceCategoryDynamic model);
        Task<int> DeleteAsync(int id);
        Task<ServiceCategoryDynamic?> GetByIdAsync(int id);
        Task<IEnumerable<ServiceCategoryDynamic>> GetAllAsync(int pageNumber, int pageSize);
    }
}
