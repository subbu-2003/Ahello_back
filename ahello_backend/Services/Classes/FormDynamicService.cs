using ahello_backend.Models.Form;
using ahello_backend.Models.Forms;
using ahello_backend.Models.Pagination;
using ahello_backend.Repositorys.Classes;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    namespace ahello_backend.Services.Classes
    {
        public class FormDynamicService
            : IFormDynamicService
        {
            private readonly IFormDynamicRepository
                _repository;

            public FormDynamicService(IFormDynamicRepository repository)
            {
                _repository = repository;
            }

            public async Task<IEnumerable<FormDynamicGetResponse>>  GetAllAsync()
            {
                return await _repository .GetAllAsync();
            }

            public async Task<FormDynamicGetResponse>GetByIdAsync(int formId)
            {
                return await _repository.GetByIdAsync(formId);
            }

            public async Task<PagedResult<FormDynamicGetResponse>>GetByUserIdAsync(FormSearchRequest model)
            {
                return await _repository.GetByUserIdAsync(model);
            }

            public async Task<int> CreateAsync(FormDynamicPost model)
            {
                return await _repository.CreateAsync(model);
            }

            public async Task<bool> UpdateAsync(  int formId, FormDynamicPut model)
            {
                return await _repository.UpdateAsync(formId, model);
            }

            public async Task<bool> DeleteAsync(int formId  )
            {
                return await _repository.DeleteAsync(formId);
            }
            public async Task<int> CreateFormTemplateAsync(FormTemplatePost model)
            {
                return await _repository.CreateFormTemplateAsync(model);
            }

            public async Task<FormDynamicGetResponse> GetSubmittedFormAsync(int formId)
            {
                return await _repository.GetSubmittedFormAsync(formId);
            }
            public async Task<bool> UpdateFormTemplateAsync(
                                        int formId,
                                        FormTemplatePut model)
            {
                return await _repository.UpdateFormTemplateAsync(formId, model);
            }
            public async Task<bool> SubmitFormAsync(FormSubmitPost model)
            {
                return await _repository
                    .SubmitFormAsync(model);
            }
            public async Task<bool> UpdateFormStatusAsync(FormStatusUpdateRequest model)
            {
                return await _repository
                    .UpdateFormStatusAsync(model);
            }
        }
    }
}
