using ahello_backend.Models.Service;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IServiceCategoryFieldsRepository
    {
        Task<int> AddAsync(ServiceCategoryFields model);

        Task<int> UpdateAsync(ServiceCategoryFields model);

        Task<int> DeleteAsync(int id);

        Task<ServiceCategoryFields?> GetByIdAsync(int id);

        Task<IEnumerable<ServiceCategoryFields>> GetAllAsync();
    }
}
