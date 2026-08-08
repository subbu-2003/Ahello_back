using ahello_backend.Models.Logos;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface ILogoRepository
    {
        Task<IEnumerable<LogoGetResponse>> GetAllAsync();

        Task<int> CreateAsync(LogoPost model);

        Task<bool> UpdateAsync(int id, LogoPut model);
    }
}
