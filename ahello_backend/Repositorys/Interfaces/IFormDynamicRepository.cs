using ahello_backend.Models.Form;
using ahello_backend.Models.Forms;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IFormDynamicRepository
    {
        Task<IEnumerable<FormDynamicGetResponse>>
            GetAllAsync();

        Task<FormDynamicGetResponse>
            GetByIdAsync(int formId);

        Task<IEnumerable<FormDynamicGetResponse>>
            GetByUserIdAsync(int userId);

        Task<int> CreateAsync(FormDynamicPost model);

        Task<bool> UpdateAsync(
            int formId,
            FormDynamicPut model);

        Task<bool> DeleteAsync(int formId);
        Task<int> CreateFormTemplateAsync(FormTemplatePost model);
        Task<FormDynamicGetResponse> GetSubmittedFormAsync(int formId);
        Task<bool> UpdateFormTemplateAsync(int formId, FormTemplatePost model);
        Task<bool> SubmitFormAsync(FormSubmitPost model);
    }
}