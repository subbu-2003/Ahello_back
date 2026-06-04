using ahello_backend.Models.Form;

namespace ahello_backend.Services.Interfaces
{
    public interface IFormFieldValueService
    {
        Task<int> CreateAsync(CreateFormFieldValue model);

        Task<bool> UpdateAsync(UpdateFormFieldValue model);

        Task<IEnumerable<FormFieldValue>> GetByFormAsync(int formId);

        Task<FormFieldValue> GetByIdAsync(int formFieldValueId);
    }
}
