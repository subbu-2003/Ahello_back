using ahello_backend.Models.Admin;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IAdminDashboardRepository _adminDashboardRepository;

        public AdminDashboardService(
            IAdminDashboardRepository adminDashboardRepository)
        {
            _adminDashboardRepository = adminDashboardRepository;
        }

        public async Task<AdminDashboardModel> GetAdminDashboardAsync()
        {
            return await _adminDashboardRepository.GetAdminDashboardAsync();
        }
    }
}
