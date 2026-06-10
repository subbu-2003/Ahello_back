using ahello_backend.Models.Form;
using ahello_backend.Models.Forms;
using MySqlX.XDevAPI;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IFormDynamicRepository
    {
        Task<IEnumerable<FormDynamicGetResponse>> GetAllAsync();

        Task<FormDynamicGetResponse> GetByIdAsync(int formId, int clientId);

        Task<IEnumerable<FormDynamicGetResponse>> GetByUserIdAsync(int userId, int clientId);

        Task<int> CreateAsync(FormDynamicPost model);

        Task<bool> UpdateAsync(int formId,FormDynamicPut model);

        Task<bool> DeleteAsync(int formId,int clientId);
        Task<int> CreateFormTemplateAsync(FormTemplatePost model);
        Task<FormDynamicGetResponse> GetSubmittedFormAsync(int formId, int clientId);
        Task<bool> UpdateFormTemplateAsync(int formId, FormTemplatePut model);
        Task<bool> SubmitFormAsync(FormSubmitPost model);
    }
}