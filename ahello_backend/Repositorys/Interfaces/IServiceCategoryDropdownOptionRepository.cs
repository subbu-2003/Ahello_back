using ahello_backend.Models.Service;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IServiceCategoryDropdownOptionRepository
    {
        Task<int> AddAsync(ServiceCategoryDropdownOption model);

        Task<int> UpdateAsync(ServiceCategoryDropdownOption model);

        Task<int> DeleteAsync(int id);

        Task<ServiceCategoryDropdownOption?> GetByIdAsync(int id);

        Task<IEnumerable<ServiceCategoryDropdownOption>> GetAllAsync();
    }
}
