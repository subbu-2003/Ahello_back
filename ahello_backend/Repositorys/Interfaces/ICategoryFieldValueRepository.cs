using ahello_backend.Models.Category;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface ICategoryFieldValueRepository
    {
        Task<int> CreateAsync(CreateCategoryFieldValue model);

        Task<bool> UpdateAsync(UpdateCategoryFieldValue model);

        Task<IEnumerable<CategoryFieldValue>> GetByCategoryAsync(int categoryId);

        Task<CategoryFieldValue> GetByIdAsync(int categoryFieldValueId);
    }
}
