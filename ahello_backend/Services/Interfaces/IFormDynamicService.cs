using ahello_backend.Models.Form;
using ahello_backend.Models.Forms;
using ahello_backend.Models.Pagination;

namespace ahello_backend.Services.Interfaces
{
    public interface IFormDynamicService
    {
        Task<IEnumerable<FormDynamicGetResponse>>  GetAllAsync();

        Task<FormDynamicGetResponse>GetByIdAsync(int formId);

        Task<PagedResult<FormDynamicGetResponse>>GetByUserIdAsync(FormSearchRequest model);

        Task<int> CreateAsync(FormDynamicPost model);

        Task<bool> UpdateAsync(int formId,FormDynamicPut model);

        Task<bool> DeleteAsync(int formId);
        Task<int> CreateFormTemplateAsync(FormTemplatePost model);
        Task<FormDynamicGetResponse> GetSubmittedFormAsync(int formId);
        Task<bool> UpdateFormTemplateAsync(int formId, FormTemplatePut model);
        Task<bool> SubmitFormAsync(FormSubmitPost model);
        Task<bool> UpdateFormStatusAsync(FormStatusUpdateRequest model);
    }
}
