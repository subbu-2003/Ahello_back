using ahello_backend.Models.Users;

namespace ahello_backend.Services.Interfaces
{
    public interface IUserFieldValueService
    {
        Task<int> CreateAsync(CreateUserFieldValue model);

        Task<bool> UpdateAsync(UpdateUserFieldValue model);

        Task<IEnumerable<UserFieldValue>> GetByUserAsync(int userId);

        Task<UserFieldValue> GetByIdAsync(int userFieldValueId);
    }
}
