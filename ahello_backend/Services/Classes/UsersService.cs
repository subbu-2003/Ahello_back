using ahello_backend.Models.Pagination;
using ahello_backend.Models.Users;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository _repository;

        public UsersService(IUsersRepository repository)
        {
            _repository = repository;

        }

        public async Task<int> PostUserAsync(UserPost post)
        {
            return await _repository.PostUserAsync(post);
        }

        public async Task<IEnumerable<UserRead>> GetAllUsersAsync()
        {
            return await _repository.GetAllUsersAsync();
        }

        public async Task<UserRead> GetUserByIdAsync(int userId)
        {
            return await _repository.GetUserByIdAsync(userId);
        }

        public async Task<bool> UpdateUserAsync(UserPut put)
        {
            return await _repository.PutUserAsync(put);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            return await _repository.DeleteUserAsync(userId);
        }
        public async Task<PagedResult<UserServiceResponse>> GetUserServicesAsync(
         int pageNumber = 1,
         int pageSize = 10,
         string? search = null)
        {
            return await _repository.GetUserServicesAsync(
                pageNumber,
                pageSize,
                search);
        }
        public async Task<SearchResultDto> SearchUserServicesAsync(
     string? keyword,
     int pageNumber,
     int pageSize)
        {
            return await _repository.SearchUserServicesAsync(keyword, pageNumber, pageSize);
        }
    }
}