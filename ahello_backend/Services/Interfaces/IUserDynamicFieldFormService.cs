using ahello_backend.Models.Users;

namespace ahello_backend.Services.Interfaces
{
    public interface IUserDynamicFieldFormService
    {
        Task<UserDynamicFieldFormResponse>
            GetUserDynamicFieldFormAsync(int userId);
    }
}
