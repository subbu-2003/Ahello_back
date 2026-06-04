using ahello_backend.Models.Category;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    namespace ahello_backend.Services.Classes
    {
        public class CategoryFieldValueService : ICategoryFieldValueService
        {
            private readonly ICategoryFieldValueRepository _repository;

            public CategoryFieldValueService(
                ICategoryFieldValueRepository repository)
            {
                _repository = repository;
            }

            public Task<int> CreateAsync(CreateCategoryFieldValue model)
                => _repository.CreateAsync(model);

            public Task<bool> UpdateAsync(UpdateCategoryFieldValue model)
                => _repository.UpdateAsync(model);

            public Task<IEnumerable<CategoryFieldValue>> GetByCategoryAsync(int categoryId)
                => _repository.GetByCategoryAsync(categoryId);

            public Task<CategoryFieldValue> GetByIdAsync(int categoryFieldValueId)
                => _repository.GetByIdAsync(categoryFieldValueId);
        }
    }
}
