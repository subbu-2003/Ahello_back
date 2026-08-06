using ahello_backend.Models;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _repository;

        public DashboardService(IDashboardRepository repository)
        {
            _repository = repository;
        }

        public async Task<DashboardGet> GetByUserIdAsync(int userId)
        {
            return await _repository.GetByUserIdAsync(userId);
        }
    }
}
