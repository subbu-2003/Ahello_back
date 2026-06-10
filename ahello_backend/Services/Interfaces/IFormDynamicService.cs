using ahello_backend.Models.Form;
using ahello_backend.Models.Forms;

namespace ahello_backend.Services.Interfaces
{
    public interface IFormDynamicService
    {
        Task<IEnumerable<FormDynamicGetResponse>>  GetAllAsync();

        Task<FormDynamicGetResponse>GetByIdAsync(int formId, int clientId);

        Task<IEnumerable<FormDynamicGetResponse>>GetByUserIdAsync(int userId, int clientId);

        Task<int> CreateAsync(FormDynamicPost model);

        Task<bool> UpdateAsync(int formId,FormDynamicPut model);

        Task<bool> DeleteAsync(int formId, int clientId);
        Task<int> CreateFormTemplateAsync(FormTemplatePost model);
        Task<FormDynamicGetResponse> GetSubmittedFormAsync(int formId, int clientId);
        Task<bool> UpdateFormTemplateAsync(int formId, FormTemplatePut model);
        Task<bool> SubmitFormAsync(FormSubmitPost model);
    }
}
