using ahello_backend.Models.Users;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class UserFieldValueService : IUserFieldValueService
    {
        private readonly IUserFieldValueRepository _repository;

        public UserFieldValueService(
            IUserFieldValueRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CreateAsync(CreateUserFieldValue model)
            => _repository.CreateAsync(model);

        public Task<bool> UpdateAsync(UpdateUserFieldValue model)
            => _repository.UpdateAsync(model);

        public Task<IEnumerable<UserFieldValue>> GetByUserAsync(int userId)
            => _repository.GetByUserAsync(userId);

        public Task<UserFieldValue> GetByIdAsync(int userFieldValueId)
            => _repository.GetByIdAsync(userFieldValueId);
    }
}