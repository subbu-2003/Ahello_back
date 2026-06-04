using ahello_backend.Models.Users;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IUserDynamicFieldFormRepository
    {
        Task<UserDynamicFieldFormResponse>
            GetUserDynamicFieldFormAsync(int userId);
    }
}
