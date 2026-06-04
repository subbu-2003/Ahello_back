using ahello_backend.Models.User;
using ahello_backend.Models.Users;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class UserDynamicService : IUserDynamicService
    {
        private readonly IUserDynamicRepository _repository;

        public UserDynamicService(IUserDynamicRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<UserDynamicGetResponse>> GetAllAsync()
            => _repository.GetAllAsync();

        public Task<UserDynamicPaginationResponse> GetAllPaginationAsync(
            int pageNumber,
            int pageSize, string? search)
            => _repository.GetAllPaginationAsync(pageNumber, pageSize,search);

        public Task<UserDynamicGetResponse> GetByIdAsync(int userId)
            => _repository.GetByIdAsync(userId);
        public async Task<IEnumerable<UserDynamicGetResponse>> GetByCategoryIdAsync(int categoryId)
        {
            return await _repository.GetByCategoryIdAsync(categoryId);
        }

        public Task<int> CreateAsync(UserDynamicPost model)
            => _repository.CreateAsync(model);

        public Task<bool> UpdateAsync(int userId, UserDynamicPut model)
            => _repository.UpdateAsync(userId, model);

        public Task<bool> DeleteAsync(int userId)
            => _repository.DeleteAsync(userId);

        public async Task<UserProfileResponse> GetUserProfileAsync(
            int userId,
            int pageNumber,
            int pageSize, string search)
        {
            return await _repository.GetUserProfileAsync(
                userId,
                pageNumber,
                pageSize, search);
        }
    }
}