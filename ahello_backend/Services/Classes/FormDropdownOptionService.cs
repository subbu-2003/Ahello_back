using ahello_backend.Models.Form;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class FormDropdownOptionService
        : IFormDropdownOptionService
    {
        private readonly IFormDropdownOptionRepository
            _repository;

        public FormDropdownOptionService(
            IFormDropdownOptionRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(
            CreateFormDropdownOption model)
        {
            return await _repository.CreateAsync(model);
        }

        public async Task<bool> UpdateAsync(
            UpdateFormDropdownOption model)
        {
            return await _repository.UpdateAsync(model);
        }

        public async Task<GetFormDropdownOption> GetByIdAsync(
            int optionId)
        {
            return await _repository.GetByIdAsync(optionId);
        }

        public async Task<IEnumerable<GetFormDropdownOption>>
            GetByFieldIdAsync(
                int formFieldId,
                int? formId)
        {
            return await _repository.GetByFieldIdAsync(
                formFieldId,
                formId);
        }
    }
}