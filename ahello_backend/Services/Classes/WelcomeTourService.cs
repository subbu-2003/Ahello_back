using ahello_backend.Models.WelcomeTour;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class WelcomeTourService : IWelcomeTourService
    {
        private readonly IWelcomeTourRepository _repository;

        public WelcomeTourService(IWelcomeTourRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<WelcomeTour>> GetAsync()
        {
            return await _repository.GetAsync();
        }

        public async Task<IEnumerable<WelcomeTour>> GetByUserIdAsync(int userId)
        {
            return await _repository.GetByUserIdAsync(userId);
        }

        public async Task<int> PostAsync(WelcomeTour model)
        {
            return await _repository.PostAsync(model);
        }

        public async Task<bool> PutAsync(WelcomeTour model)
        {
            return await _repository.PutAsync(model);
        }

        public async Task<bool> PutByUserIdAsync(
            int userId,
            bool isActive,
            int modifiedBy)
        {
            return await _repository.PutByUserIdAsync(
                userId,
                isActive,
                modifiedBy
            );
        }
        public async Task<bool> PutByUserIdAndItemKeyAsync(
    int userId,
    string itemKey,
    bool isActive,
    int modifiedBy)
        {
            return await _repository.PutByUserIdAndItemKeyAsync(
                userId,
                itemKey,
                isActive,
                modifiedBy
            );
        }
    }
}
