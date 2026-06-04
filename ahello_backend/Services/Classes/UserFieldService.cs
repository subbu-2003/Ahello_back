using ahello_backend.Models.Users;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class UserFieldService : IUserFieldService
    {
        private readonly IUserFieldRepository _repository;

        public UserFieldService(IUserFieldRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CreateAsync(CreateUserField model)
            => _repository.CreateAsync(model);

        public Task<bool> UpdateAsync(UpdateUserField model)
            => _repository.UpdateAsync(model);

        public Task<IEnumerable<UserField>> GetByUserAsync(int userId)
            => _repository.GetByUserAsync(userId);

        public Task<UserField> GetByIdAsync(int userFieldId)
            => _repository.GetByIdAsync(userFieldId);

        public Task<bool> DeleteAsync(int userFieldId, string modifiedBy)
            => _repository.DeleteAsync(userFieldId, modifiedBy);
        public async Task<IEnumerable<UserFieldWithOptions>>GetFieldsWithOptionsAsync(int userId)
        {
            return await _repository.GetFieldsWithOptionsAsync(userId);
        }
    }
}