using ahello_backend.Models.Category;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface ICategoryDropdownOptionRepository
    {
        Task<int> CreateAsync(
            CreateCategoryDropdownOption model);

        Task<bool> UpdateAsync(
            UpdateCategoryDropdownOption model);

        Task<GetCategoryDropdownOption> GetByIdAsync(
            int optionId);

        Task<IEnumerable<GetCategoryDropdownOption>>
            GetByFieldIdAsync(
                int categoryFieldId,
                int? categoryId);
    }
}
