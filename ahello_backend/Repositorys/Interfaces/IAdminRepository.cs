using ahello_backend.Models.Admin;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IAdminRepository
    {
        Task<AdminUser?> GetByUsernameAsync(string username);

        Task<int> CreateAsync(AdminUser admin);
        Task<bool> UsernameExistsAsync(string username);
    }
}
