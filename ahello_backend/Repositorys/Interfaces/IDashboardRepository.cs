using ahello_backend.Models;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IDashboardRepository
    {
        Task<DashboardGet> GetByUserIdAsync(int userId);
    }
}
