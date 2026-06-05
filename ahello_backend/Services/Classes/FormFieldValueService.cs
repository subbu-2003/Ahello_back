using ahello_backend.Models.Form;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class FormFieldValueService : IFormFieldValueService
    {
        private readonly IFormFieldValueRepository _repository;

        public FormFieldValueService(IFormFieldValueRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(CreateFormFieldValue model)
        {
            return await _repository.CreateAsync(model);
        }

        public async Task<bool> UpdateAsync(UpdateFormFieldValue model)
        {
            return await _repository.UpdateAsync(model);
        }

        public async Task<IEnumerable<FormFieldValue>> GetByFormAsync(int formId)
        {
            return await _repository.GetByFormAsync(formId);
        }

        public async Task<FormFieldValue> GetByIdAsync(int formFieldValueId)
        {
            return await _repository.GetByIdAsync(formFieldValueId);
        }
        public async Task<IEnumerable<FormFieldValueUserResponse>>
    GetByUserIdAsync(int userId)
        {
            return await _repository
                .GetByUserIdAsync(userId);
        }
    }
}