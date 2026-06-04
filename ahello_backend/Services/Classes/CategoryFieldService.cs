using ahello_backend.Models.Category;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class CategoryFieldService : ICategoryFieldService
    {
        private readonly ICategoryFieldRepository _repository;

        public CategoryFieldService(ICategoryFieldRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CreateAsync(CreateCategoryField model)
            => _repository.CreateAsync(model);

        public Task<bool> UpdateAsync(UpdateCategoryField model)
            => _repository.UpdateAsync(model);

        public Task<IEnumerable<CategoryField>> GetByCategoryAsync(int categoryId)
            => _repository.GetByCategoryAsync(categoryId);

        public Task<CategoryField> GetByIdAsync(int categoryFieldId)
            => _repository.GetByIdAsync(categoryFieldId);

        public Task<bool> DeleteAsync(int categoryFieldId, string modifiedBy)
            => _repository.DeleteAsync(categoryFieldId, modifiedBy);
    }
}