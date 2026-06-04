using ahello_backend.Models.Pagination;
using ahello_backend.Models.Users;

namespace ahello_backend.Services.Interfaces
{
    public interface IUsersService
    {
        Task<int> PostUserAsync(UserPost post);

        Task<UserRead> GetUserByIdAsync(int userId);

        Task<IEnumerable<UserRead>> GetAllUsersAsync();

        Task<bool> UpdateUserAsync(UserPut put);

        Task<bool> DeleteUserAsync(int userId);
        Task<PagedResult<UserServiceResponse>> GetUserServicesAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? search = null);
        Task<SearchResultDto> SearchUserServicesAsync(string? keyword, int pageNumber, int pageSize);
    }
}
