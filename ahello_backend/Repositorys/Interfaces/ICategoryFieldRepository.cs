using ahello_backend.Models.Category;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface ICategoryFieldRepository
    {
        Task<int> CreateAsync(CreateCategoryField model);

        Task<bool> UpdateAsync(UpdateCategoryField model);

        Task<IEnumerable<CategoryField>> GetByCategoryAsync(int categoryId);

        Task<CategoryField> GetByIdAsync(int categoryFieldId);

        Task<bool> DeleteAsync(int categoryFieldId, string modifiedBy);
    }
}
