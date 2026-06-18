using ahello_backend.Models.Admin;

namespace ahello_backend.Services.Interfaces
{
    public interface IAdminService
    {
        Task<string?> LoginAsync(AdminLoginDto dto);

        Task<int> CreateAdminAsync(AdminCreateDto dto);
    }
}
