using ahello_backend.Models.Category;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Category> GetByIdAsync(int categoryId)
        {
            return await _repository.GetByIdAsync(categoryId);
        }

        public async Task<int> CreateAsync(CategoryCreate model)
        {
            return await _repository.CreateAsync(model);
        }

        public async Task<int> UpdateAsync(CategoryUpdate model)
        {
            return await _repository.UpdateAsync(model);
        }

        public async Task<int> DeleteAsync(int categoryId)
        {
            return await _repository.DeleteAsync(categoryId);
        }
    }
}
