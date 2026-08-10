using ahello_backend.Models.Admin;

namespace ahello_backend.Services.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardModel> GetAdminDashboardAsync();
    }
}
