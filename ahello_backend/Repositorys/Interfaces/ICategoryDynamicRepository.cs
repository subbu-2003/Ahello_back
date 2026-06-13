using ahello_backend.Models.Category;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface ICategoryDynamicRepository
    {
        Task<IEnumerable<CategoryDynamicGetResponse>> GetAllAsync();

        Task<CategoryDynamicGetResponse> GetByIdAsync(int categoryId);

        Task<int> CreateAsync(CategoryDynamicPost model);

        Task<bool> UpdateAsync(int categoryId, CategoryDynamicPut model);

        Task<bool> DeleteAsync(int categoryId);
        Task<PagedCategoryDynamicResponse> GetAllPagedAsync(
        int pageNumber,
        int pageSize, string? search = null);
        Task<bool> UpdateStatusAsync(
        int categoryId, bool isActive,string modifiedBy);
    }
}
