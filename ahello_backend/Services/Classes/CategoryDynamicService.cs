using ahello_backend.Models.Category;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class CategoryDynamicService : ICategoryDynamicService
    {
        private readonly ICategoryDynamicRepository _repository;

        public CategoryDynamicService(ICategoryDynamicRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<CategoryDynamicGetResponse>> GetAllAsync()
            => _repository.GetAllAsync();

        public Task<CategoryDynamicGetResponse> GetByIdAsync(int categoryId)
            => _repository.GetByIdAsync(categoryId);

        public Task<int> CreateAsync(CategoryDynamicPost model)
            => _repository.CreateAsync(model);

        public Task<bool> UpdateAsync(int categoryId, CategoryDynamicPut model)
            => _repository.UpdateAsync(categoryId, model);

        public Task<bool> DeleteAsync(int categoryId)
            => _repository.DeleteAsync(categoryId);
        public Task<PagedCategoryDynamicResponse> GetAllPagedAsync(
        int pageNumber,
        int pageSize, string? search = null)
    => _repository.GetAllPagedAsync(pageNumber, pageSize,search);
        public Task<bool> UpdateStatusAsync(
        int categoryId, bool isActive,string modifiedBy)
    => _repository.UpdateStatusAsync(
        categoryId,isActive, modifiedBy);
    }
}