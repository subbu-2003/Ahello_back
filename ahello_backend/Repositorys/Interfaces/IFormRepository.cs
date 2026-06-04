using ahello_backend.Models.Form;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IFormRepository
    {
        Task<IEnumerable<Form>> GetAllAsync();

        Task<Form> GetByIdAsync(int formId);
        Task<IEnumerable<Form>> GetByUserIdAsync(int userId);

        Task<int> CreateAsync(FormCreate model);

        Task<int> UpdateAsync(FormUpdate model);

        Task<int> DeleteAsync(int formId);
    }
}
