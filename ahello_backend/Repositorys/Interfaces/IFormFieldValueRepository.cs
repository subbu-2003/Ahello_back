using ahello_backend.Models.Form;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IFormFieldValueRepository
    {
        Task<int> CreateAsync(CreateFormFieldValue model);

        Task<bool> UpdateAsync(UpdateFormFieldValue model);

        Task<IEnumerable<FormFieldValue>> GetByFormAsync(int formId);

        Task<FormFieldValue> GetByIdAsync(int formFieldValueId);
    }
}
