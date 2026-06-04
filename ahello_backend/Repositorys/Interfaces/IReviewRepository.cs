using ahello_backend.Models.Reviews;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IReviewRepository
    {
        Task<IEnumerable<Review>> GetAllAsync();

        Task<Review> GetByIdAsync(int reviewId);

        Task<IEnumerable<Review>> GetByBookingIdAsync(int bookingId);

        Task<int> CreateAsync(ReviewRequest request);

        Task<int> UpdateAsync(int reviewId, ReviewUpdateRequest request);

        Task<int> DeleteAsync(int reviewId);
    }
}
