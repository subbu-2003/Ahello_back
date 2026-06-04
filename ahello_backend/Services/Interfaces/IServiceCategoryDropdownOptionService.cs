using ahello_backend.Models.Service;

namespace ahello_backend.Services.Interfaces
{
    public interface IServiceCategoryDropdownOptionService
    {
        Task<int> AddAsync(ServiceCategoryDropdownOption model);

        Task<int> UpdateAsync(ServiceCategoryDropdownOption model);

        Task<int> DeleteAsync(int id);

        Task<ServiceCategoryDropdownOption?> GetByIdAsync(int id);

        Task<IEnumerable<ServiceCategoryDropdownOption>> GetAllAsync();
    }
}
