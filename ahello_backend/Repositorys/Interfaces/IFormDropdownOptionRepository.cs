using ahello_backend.Models.Form;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IFormDropdownOptionRepository
    {
        Task<int> CreateAsync(
            CreateFormDropdownOption model);

        Task<bool> UpdateAsync(
            UpdateFormDropdownOption model);

        Task<GetFormDropdownOption> GetByIdAsync(
            int optionId);

        Task<IEnumerable<GetFormDropdownOption>>
            GetByFieldIdAsync(
                int formFieldId,
                int? formId);
    }
}
