using ahello_backend.Models.Logos;

namespace ahello_backend.Services.Interfaces
{
    public interface ILogoService
    {
        Task<IEnumerable<LogoGetResponse>> GetAllAsync();

        Task<int> CreateAsync(LogoPost model);

        Task<bool> UpdateAsync(int id, LogoPut model);
    }
}
