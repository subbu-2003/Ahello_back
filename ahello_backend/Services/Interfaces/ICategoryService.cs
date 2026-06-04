using ahello_backend.Models.Category;

namespace ahello_backend.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync();

        Task<Category> GetByIdAsync(int categoryId);

        Task<int> CreateAsync(CategoryCreate model);

        Task<int> UpdateAsync(CategoryUpdate model);

        Task<int> DeleteAsync(int categoryId);
    }
}
