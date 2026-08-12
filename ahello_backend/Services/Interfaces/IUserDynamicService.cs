using ahello_backend.Models.User;
using ahello_backend.Models.Users;

namespace ahello_backend.Services.Interfaces
{
    public interface IUserDynamicService
    {
        Task<IEnumerable<UserDynamicGetResponse>> GetAllAsync();

        Task<UserDynamicPaginationResponse> GetAllPaginationAsync(
            int pageNumber,
            int pageSize,string? search);

        Task<UserDynamicGetResponse> GetByIdAsync(int userId);
        Task<IEnumerable<UserDynamicGetResponse>>GetByCategoryIdAsync(int categoryId);

        Task<UserProfileResponse> GetUserProfileBySlugAsync(
    string slug, int pageNumber, int pageSize, string search);

        Task<int> CreateAsync(UserDynamicPost model);

        Task<bool> UpdateAsync(int userId, UserDynamicPut model);

        Task<bool> DeleteAsync(int userId);
        
        Task<UserProfileResponse> GetUserProfileAsync(
                int userId,
                int pageNumber,
                int pageSize, string search);
        
    }

}
