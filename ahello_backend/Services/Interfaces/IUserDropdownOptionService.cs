using ahello_backend.Models.Users;

namespace ahello_backend.Services.Interfaces
{
    public interface IUserDropdownOptionService
    {
        Task<int> CreateAsync(CreateUserDropdownOption model);

        Task<bool> UpdateAsync(UpdateUserDropdownOption model);

        Task<GetUserDropdownOption> GetByIdAsync(int optionId);

        Task<IEnumerable<GetUserDropdownOption>>
            GetByFieldIdAsync(
                int userFieldId,
                int? userId);
    }
}
