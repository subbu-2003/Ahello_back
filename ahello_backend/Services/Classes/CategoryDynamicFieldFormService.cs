using ahello_backend.Models.Category;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class CategoryDynamicFieldFormService : ICategoryDynamicFieldFormService
    {
        private readonly ICategoryDynamicFieldFormRepository _repository;

        public CategoryDynamicFieldFormService(
            ICategoryDynamicFieldFormRepository repository)
        {
            _repository = repository;
        }

        public async Task<CategoryDynamicFieldFormResponse>
            GetCategoryDynamicFieldFormAsync(int categoryId)
        {
            return await _repository
                .GetCategoryDynamicFieldFormAsync(categoryId);
        }
    }
}