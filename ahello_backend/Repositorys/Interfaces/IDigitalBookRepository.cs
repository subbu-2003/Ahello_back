using ahello_backend.Models.DigitalBook;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IDigitalBookRepository
    {
        Task<int> CreateAsync(DigitalBook digitalBook);

        Task<bool> UpdateAsync(DigitalBook digitalBook);

        Task<IEnumerable<DigitalBook>> GetAllAsync();

        Task<IEnumerable<DigitalBook>> GetByUserIdAsync(int userId);

        Task<bool> DeactivateAsync(int digitalBookId, int modifiedBy);

        Task<IEnumerable<DigitalBook>> GetBySlugAsync(string slug);
    }
}
