using ahello_backend.Models.Form;

namespace ahello_backend.Services.Interfaces
{
    public interface IFormService
    {
        Task<IEnumerable<Form>> GetAllAsync();

        Task<Form> GetByIdAsync(int formId);
        Task<IEnumerable<Form>> GetByUserIdAsync(int userId);
        Task<int> CreateAsync(FormCreate model);

        Task<int> UpdateAsync(FormUpdate model);

        Task<int> DeleteAsync(int formId);
    }
}
