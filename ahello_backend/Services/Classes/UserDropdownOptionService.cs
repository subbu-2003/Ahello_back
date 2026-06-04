using ahello_backend.Models.Users;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class UserDropdownOptionService
        : IUserDropdownOptionService
    {
        private readonly IUserDropdownOptionRepository _repo;

        public UserDropdownOptionService(
            IUserDropdownOptionRepository repo)
        {
            _repo = repo;
        }

        public async Task<int> CreateAsync(
            CreateUserDropdownOption model)
        {
            return await _repo.CreateAsync(model);
        }

        public async Task<bool> UpdateAsync(
            UpdateUserDropdownOption model)
        {
            return await _repo.UpdateAsync(model);
        }

        public async Task<GetUserDropdownOption> GetByIdAsync(
            int optionId)
        {
            return await _repo.GetByIdAsync(optionId);
        }

        public async Task<IEnumerable<GetUserDropdownOption>>
            GetByFieldIdAsync(
                int userFieldId,
                int? userId)
        {
            return await _repo.GetByFieldIdAsync(
                userFieldId,
                userId);
        }
    }
}