using ahello_backend.DbContexts;
using ahello_backend.Models.DigitalBook;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class DigitalBookRepository : IDigitalBookRepository
    {
        private readonly DbContext _db;

        public DigitalBookRepository(DbContext db)
        {
            _db = db;
        }

        // GET ALL
        public async Task<IEnumerable<DigitalBook>> GetAllAsync()
        {
            var sql = @"
                SELECT
                    db.digitalbookId,
                    db.UserId,
                    db.Title,
                    db.Description,
                    db.PreviewImage,
                    db.PdfFile,
                    db.Status,
                    db.Price,
                    db.IsActive,
                    db.CreatedBy,
                    db.CreatedAt,
                    db.ModifiedBy,
                    db.ModifiedAt
                FROM digitalbook db
                WHERE db.IsActive = 1
                ORDER BY db.digitalbookId DESC";

            using var connection = _db.GetConnection();

            return await connection.QueryAsync<DigitalBook>(sql);
        }

        // GET BY USER ID
        public async Task<IEnumerable<DigitalBook>> GetByUserIdAsync(int userId)
        {
            var sql = @"
                SELECT
                    db.digitalbookId,
                    db.UserId,
                    db.Title,
                    db.Description,
                    db.PreviewImage,
                    db.PdfFile,
                    db.Status,
                    db.Price,
                    db.IsActive,
                    db.CreatedBy,
                    db.CreatedAt,
                    db.ModifiedBy,
                    db.ModifiedAt
                FROM digitalbook db
                WHERE db.UserId = @UserId
                  AND db.IsActive = 1
                ORDER BY db.digitalbookId DESC";

            using var connection = _db.GetConnection();

            return await connection.QueryAsync<DigitalBook>(
                sql,
                new { UserId = userId });
        }

        // CREATE
        public async Task<int> CreateAsync(DigitalBook digitalBook)
        {
            var sql = @"
                INSERT INTO digitalbook
                (
                    UserId,
                    Title,
                    Description,
                    PreviewImage,
                    PdfFile,
                    Status,
                    Price,
                    IsActive,
                    CreatedBy,
                    CreatedAt
                )
                VALUES
                (
                    @UserId,
                    @Title,
                    @Description,
                    @PreviewImage,
                    @PdfFile,
                    @Status,
                    @Price,
                    1,
                    @CreatedBy,
                    NOW()
                );

                SELECT LAST_INSERT_ID();";

            using var connection = _db.GetConnection();

            return await connection.ExecuteScalarAsync<int>(
                sql,
                digitalBook);
        }

        // UPDATE
        public async Task<bool> UpdateAsync(DigitalBook digitalBook)
        {
            var sql = @"
                UPDATE digitalbook
                SET
                    UserId = @UserId,
                    Title = @Title,
                    Description = @Description,
                    PreviewImage = COALESCE(@PreviewImage, PreviewImage),
                    PdfFile = COALESCE(@PdfFile, PdfFile),
                    Status = @Status,
                    Price = @Price,
                    ModifiedBy = @ModifiedBy,
                    ModifiedAt = NOW()
                WHERE digitalbookId = @DigitalBookId
                  AND IsActive = 1";

            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(sql, digitalBook);

            return rows > 0;
        }

        // DELETE (SOFT DELETE)
        public async Task<bool> DeactivateAsync(int digitalBookId, int modifiedBy)
        {
            var sql = @"
                UPDATE digitalbook
                SET
                    IsActive = 0,
                    ModifiedBy = @ModifiedBy,
                    ModifiedAt = NOW()
                WHERE digitalbookId = @DigitalBookId
                  AND IsActive = 1";

            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(
                sql,
                new
                {
                    DigitalBookId = digitalBookId,
                    ModifiedBy = modifiedBy
                });

            return rows > 0;
        }

        // GET PROFILE BY USER SLUG
        public async Task<IEnumerable<DigitalBook>> GetBySlugAsync(
    string slug)
        {
            var sql = @"
        SELECT
            db.digitalbookId,
            db.UserId,
            db.Title,
            db.Description,
            db.PreviewImage,
            db.PdfFile,
            db.Status,
            db.Price,
            db.IsActive,
            db.CreatedBy,
            db.CreatedAt,
            db.ModifiedBy,
            db.ModifiedAt,

            u.FullName,
            u.Email,
            u.MobileNumber,
            u.ProfileUrl,
            u.Slug

        FROM digitalbook db

        INNER JOIN users u
            ON db.UserId = u.UserId

        WHERE u.Slug = @Slug
          AND db.IsActive = 1

        ORDER BY db.digitalbookId DESC";

            using var connection = _db.GetConnection();

            return await connection.QueryAsync<DigitalBook>(
                sql,
                new { Slug = slug });
        }
    }
}