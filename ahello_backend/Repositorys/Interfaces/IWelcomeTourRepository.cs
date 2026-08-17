using ahello_backend.Models.WelcomeTour;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IWelcomeTourRepository
    {
        Task<IEnumerable<WelcomeTour>> GetAsync();
        Task<IEnumerable<WelcomeTour>> GetByUserIdAsync(int userId);
        Task<int> PostAsync(WelcomeTour model);
        Task<bool> PutAsync(WelcomeTour model);
        Task<bool> PutByUserIdAsync(int userId, bool isActive, int modifiedBy);
    }
}
