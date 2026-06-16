using ahello_backend.Models.Category;

namespace ahello_backend.Services.Interfaces
{
    public interface ICategoryDynamicService
    {
        Task<PagedCategoryDynamicResponse> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        DateTime? createdDate = null);

        Task<CategoryDynamicGetResponse> GetByIdAsync(int categoryId);

        Task<int> CreateAsync(CategoryDynamicPost model);

        Task<bool> UpdateAsync(int categoryId, CategoryDynamicPut model);

        Task<bool> DeleteAsync(int categoryId);
        Task<PagedCategoryDynamicResponse> GetAllPagedAsync(
        int pageNumber,
        int pageSize, string? search = null);
        Task<bool> UpdateStatusAsync(
        int categoryId,bool isActive,string modifiedBy);
        }
}
