using ahello_backend.Models.Category;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class CategoryDropdownOptionService
        : ICategoryDropdownOptionService
    {
        private readonly
            ICategoryDropdownOptionRepository _repo;

        public CategoryDropdownOptionService(
            ICategoryDropdownOptionRepository repo)
        {
            _repo = repo;
        }

        public async Task<int> CreateAsync(
            CreateCategoryDropdownOption model)
        {
            return await _repo.CreateAsync(model);
        }

        public async Task<bool> UpdateAsync(
            UpdateCategoryDropdownOption model)
        {
            return await _repo.UpdateAsync(model);
        }

        public async Task<GetCategoryDropdownOption>
            GetByIdAsync(int optionId)
        {
            return await _repo.GetByIdAsync(optionId);
        }

        public async Task<IEnumerable<GetCategoryDropdownOption>>
            GetByFieldIdAsync(
                int categoryFieldId,
                int? categoryId)
        {
            return await _repo.GetByFieldIdAsync(
                categoryFieldId,
                categoryId);
        }
    }
}