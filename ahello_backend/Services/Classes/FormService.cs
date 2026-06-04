using ahello_backend.Models.Form;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class FormService : IFormService
    {
        private readonly IFormRepository _formRepository;

        public FormService(IFormRepository formRepository)
        {
            _formRepository = formRepository;
        }

        public async Task<IEnumerable<Form>> GetAllAsync()
        {
            return await _formRepository.GetAllAsync();
        }

        public async Task<Form> GetByIdAsync(int formId)
        {
            return await _formRepository.GetByIdAsync(formId);
        }
        public async Task<IEnumerable<Form>> GetByUserIdAsync(int userId)
        {
            return await _formRepository.GetByUserIdAsync(userId);
        }
        public async Task<int> CreateAsync(FormCreate model)
        {
            return await _formRepository.CreateAsync(model);
        }

        public async Task<int> UpdateAsync(FormUpdate model)
        {
            return await _formRepository.UpdateAsync(model);
        }

        public async Task<int> DeleteAsync(int formId)
        {
            return await _formRepository.DeleteAsync(formId);
        }
    }
}