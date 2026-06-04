using ahello_backend.Models.Service;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IServiceCategoryFieldValuesRepository
    {
        Task<int> AddAsync(ServiceCategoryFieldValues model);

        Task<int> UpdateAsync(ServiceCategoryFieldValues model);

        Task<int> DeleteAsync(int id);

        Task<ServiceCategoryFieldValues?> GetByIdAsync(int id);

        Task<IEnumerable<ServiceCategoryFieldValues>> GetAllAsync();
    }
}
