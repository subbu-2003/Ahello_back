using ahello_backend.DbContexts;
using ahello_backend.Models.Reviews;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Implementations
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly DbContext _db;

        public ReviewRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Review>> GetAllAsync()
        {
            var query = "SELECT * FROM reviews ORDER BY ReviewId DESC";

            using var connection = _db.GetConnection();

            return await connection.QueryAsync<Review>(query);
        }

        public async Task<Review> GetByIdAsync(int reviewId)
        {
            var query = "SELECT * FROM reviews WHERE ReviewId = @ReviewId";

            using var connection = _db.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<Review>(
                query,
                new { ReviewId = reviewId });
        }

        public async Task<IEnumerable<Review>> GetByBookingIdAsync(int bookingId)
        {
            var query = @"SELECT * 
                          FROM reviews 
                          WHERE BookingId = @BookingId
                          ORDER BY ReviewId DESC";

            using var connection = _db.GetConnection();

            return await connection.QueryAsync<Review>(
                query,
                new { BookingId = bookingId });
        }

        public async Task<int> CreateAsync(ReviewRequest request)
        {
            var query = @"
                INSERT INTO reviews
                (
                    BookingId,
                    Comments,
                    Rating,
                    CreatedAt,
                    CreatedBy
                )
                VALUES
                (
                    @BookingId,
                    @Comments,
                    @Rating,
                    NOW(),
                    @CreatedBy
                )";

            using var connection = _db.GetConnection();

            return await connection.ExecuteAsync(query, request);
        }

        public async Task<int> UpdateAsync(
            int reviewId,
            ReviewUpdateRequest request)
        {
            var query = @"
                UPDATE reviews
                SET
                    Comments = @Comments,
                    Rating = @Rating,
                    ModifiedAt = NOW(),
                    ModifiedBy = @ModifiedBy
                WHERE ReviewId = @ReviewId";

            using var connection = _db.GetConnection();

            return await connection.ExecuteAsync(
                query,
                new
                {
                    ReviewId = reviewId,
                    request.Comments,
                    request.Rating,
                    request.ModifiedBy
                });
        }

        public async Task<int> DeleteAsync(int reviewId)
        {
            var query = "DELETE FROM reviews WHERE ReviewId = @ReviewId";

            using var connection = _db.GetConnection();

            return await connection.ExecuteAsync(
                query,
                new { ReviewId = reviewId });
        }
    }
}