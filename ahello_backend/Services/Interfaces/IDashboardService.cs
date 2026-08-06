using ahello_backend.Models;

namespace ahello_backend.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardGet> GetByUserIdAsync(int userId);
    }
}
