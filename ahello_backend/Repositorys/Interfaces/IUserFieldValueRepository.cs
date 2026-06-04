using ahello_backend.Models.Users;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IUserFieldValueRepository
    {
        Task<int> CreateAsync(CreateUserFieldValue model);

        Task<bool> UpdateAsync(UpdateUserFieldValue model);

        Task<IEnumerable<UserFieldValue>> GetByUserAsync(int userId);

        Task<UserFieldValue> GetByIdAsync(int userFieldValueId);
    }
}
