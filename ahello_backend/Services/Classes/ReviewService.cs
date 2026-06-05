using ahello_backend.Models.Reviews;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _repository;

        public ReviewService(IReviewRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Review>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Review> GetByIdAsync(int reviewId)
        {
            return await _repository.GetByIdAsync(reviewId);
        }

        public async Task<IEnumerable<Review>> GetByBookingIdAsync(int bookingId)
        {
            return await _repository.GetByBookingIdAsync(bookingId);
        }

        public async Task<IEnumerable<Review>> GetByUserIdAsync(int userId)
        {
            return await _repository.GetByUserIdAsync(userId);
        }

        public async Task<int> CreateAsync(ReviewRequest request)
        {
            return await _repository.CreateAsync(request);
        }

        public async Task<int> UpdateAsync(
            int reviewId,
            ReviewUpdateRequest request)
        {
            return await _repository.UpdateAsync(reviewId, request);
        }

        public async Task<int> DeleteAsync(int reviewId)
        {
            return await _repository.DeleteAsync(reviewId);
        }
    }
}