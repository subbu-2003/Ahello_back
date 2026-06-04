using ahello_backend.Models.Users;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class UserDynamicFieldFormService
        : IUserDynamicFieldFormService
    {
        private readonly IUserDynamicFieldFormRepository _repository;

        public UserDynamicFieldFormService(
            IUserDynamicFieldFormRepository repository)
        {
            _repository = repository;
        }

        public async Task<UserDynamicFieldFormResponse>
            GetUserDynamicFieldFormAsync(int userId)
        {
            return await _repository
                .GetUserDynamicFieldFormAsync(userId);
        }
    }
}