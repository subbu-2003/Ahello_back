using ahello_backend.Models.Category;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface ICategoryDynamicFieldFormRepository
    {
        Task<CategoryDynamicFieldFormResponse> GetCategoryDynamicFieldFormAsync(int categoryId);
    }
}
