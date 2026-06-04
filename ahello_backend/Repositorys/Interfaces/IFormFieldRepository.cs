using ahello_backend.Models.Form;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IFormFieldRepository
    {
        Task<int> CreateAsync(CreateFormField model);

        Task<bool> UpdateAsync(UpdateFormField model);

        Task<IEnumerable<FormField>> GetByFormAsync(int formId);

        Task<FormField> GetByIdAsync(int formFieldId);

        Task<bool> DeleteAsync(
            int formFieldId,
            string modifiedBy
        );
    }
}
