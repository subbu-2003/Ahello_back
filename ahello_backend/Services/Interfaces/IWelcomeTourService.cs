using ahello_backend.Models.WelcomeTour;

namespace ahello_backend.Services.Interfaces
{
    public interface IWelcomeTourService
    {
        Task<IEnumerable<WelcomeTour>> GetAsync();
        Task<IEnumerable<WelcomeTour>> GetByUserIdAsync(int userId);
        Task<int> PostAsync(WelcomeTour model);
        Task<bool> PutAsync(WelcomeTour model);
        Task<bool> PutByUserIdAsync(int userId, bool isActive, int modifiedBy);
    }
}
