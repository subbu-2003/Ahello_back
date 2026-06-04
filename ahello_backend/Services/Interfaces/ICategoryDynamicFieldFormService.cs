using ahello_backend.Models.Category;

namespace ahello_backend.Services.Interfaces
{
    public interface ICategoryDynamicFieldFormService
    {
        Task<CategoryDynamicFieldFormResponse> GetCategoryDynamicFieldFormAsync(int categoryId);
    }
}
