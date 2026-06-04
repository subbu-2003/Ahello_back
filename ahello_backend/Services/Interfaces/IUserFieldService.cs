using ahello_backend.Models.Users;

namespace ahello_backend.Services.Interfaces
{
    public interface IUserFieldService
    {
        Task<int> CreateAsync(CreateUserField model);

        Task<bool> UpdateAsync(UpdateUserField model);

        Task<IEnumerable<UserField>> GetByUserAsync(int userId);

        Task<UserField> GetByIdAsync(int userFieldId);

        Task<bool> DeleteAsync(int userFieldId, string modifiedBy);
        Task<IEnumerable<UserFieldWithOptions>>GetFieldsWithOptionsAsync(int userId);
    }
}
