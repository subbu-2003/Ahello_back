using ahello_backend.Models.Form;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class FormFieldService : IFormFieldService
    {
        private readonly IFormFieldRepository _repository;

        public FormFieldService(IFormFieldRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(CreateFormField model)
        {
            return await _repository.CreateAsync(model);
        }

        public async Task<bool> UpdateAsync(UpdateFormField model)
        {
            return await _repository.UpdateAsync(model);
        }

        public async Task<IEnumerable<FormField>> GetByFormAsync(int formId)
        {
            return await _repository.GetByFormAsync(formId);
        }

        public async Task<FormField> GetByIdAsync(int formFieldId)
        {
            return await _repository.GetByIdAsync(formFieldId);
        }

        public async Task<bool> DeleteAsync(
            int formFieldId,
            string modifiedBy
        )
        {
            return await _repository.DeleteAsync(
                formFieldId,
                modifiedBy
            );
        }
    }
}