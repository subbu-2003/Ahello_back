using ahello_backend.Models.Form;

namespace ahello_backend.Services.Interfaces
{
    public interface IFormFieldService
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
