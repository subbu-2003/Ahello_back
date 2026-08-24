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
            ApprovalStatus,
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
            'Draft',
            'Pending',
            @Price,
            1,
            @CreatedBy,
            NOW()
        );

        SELECT LAST_INSERT_ID();
    ";

            using var connection = _db.GetConnection();

            return await connection.ExecuteScalarAsync<int>(
                sql,
                digitalBook);
        }
        public async Task<IEnumerable<DigitalBook>> GetPendingAsync()
        {
            var sql = @"
        SELECT
            db.DigitalBookId,
            db.UserId,
            db.Title,
            db.Description,
            db.PreviewImage,
            db.PdfFile,
            db.Status,
            db.ApprovalStatus,
            db.ApprovedBy,
            db.ApprovedAt,
            db.RejectionReason,
            db.Price,
            db.IsActive,
            db.CreatedBy,
            db.CreatedAt,
            db.ModifiedBy,
            db.ModifiedAt
        FROM digitalbook db
        WHERE db.IsActive = 1
          AND db.ApprovalStatus = 'Pending'
        ORDER BY db.DigitalBookId DESC;
    ";

            using var connection = _db.GetConnection();

            return await connection.QueryAsync<DigitalBook>(sql);
        }
        public async Task<bool> UpdateApprovalStatusAsync(
    int digitalBookId,
    int adminId,
    DigitalBookApprovalStatus approvalStatus,
    string? rejectionReason)
        {
            var sql = @"
        UPDATE digitalbook
        SET
            ApprovalStatus = @ApprovalStatus,

            ApprovedBy =
                CASE
                    WHEN @ApprovalStatus = 'Approved'
                    THEN @AdminId
                    ELSE NULL
                END,

            ApprovedAt =
                CASE
                    WHEN @ApprovalStatus = 'Approved'
                    THEN NOW()
                    ELSE NULL
                END,

            RejectionReason =
                CASE
                    WHEN @ApprovalStatus = 'Rejected'
                    THEN @RejectionReason
                    ELSE NULL
                END,

            ModifiedBy = @AdminId,
            ModifiedAt = NOW()

        WHERE DigitalBookId = @DigitalBookId
          AND IsActive = 1
          AND Status = 'Draft'
          AND ApprovalStatus = 'Pending';
    ";

            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(
                sql,
                new
                {
                    DigitalBookId = digitalBookId,
                    AdminId = adminId,
                    ApprovalStatus = approvalStatus.ToString(),
                    RejectionReason = rejectionReason
                });

            return rows > 0;
        }
        public async Task<bool> PublishAsync(
    int digitalBookId,
    int userId)
        {
            var sql = @"
        UPDATE digitalbook
        SET
            Status = 'Published',
            ModifiedBy = @UserId,
            ModifiedAt = NOW()
        WHERE DigitalBookId = @DigitalBookId
          AND UserId = @UserId
          AND IsActive = 1
          AND Status = 'Draft'
          AND ApprovalStatus = 'Approved';
    ";

            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(
                sql,
                new
                {
                    DigitalBookId = digitalBookId,
                    UserId = userId
                });

            return rows > 0;
        }
    }
}