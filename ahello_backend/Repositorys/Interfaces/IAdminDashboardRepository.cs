using ahello_backend.Models.Admin;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IAdminDashboardRepository
    {
        Task<AdminDashboardModel> GetAdminDashboardAsync();
    }
}
