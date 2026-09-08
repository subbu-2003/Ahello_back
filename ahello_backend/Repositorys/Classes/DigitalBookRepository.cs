using ahello_backend.DbContexts;
using ahello_backend.Models.DigitalBook;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class DigitalBookRepository : IDigitalBookRepository
    {
        private readonly DbContext _db;
        private readonly IEmailRepository _emailRepository;

        public DigitalBookRepository(DbContext db, IEmailRepository emailRepository)
        {
            _db = db;
            _emailRepository = emailRepository;
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
db.ApprovalStatus,
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
          AND db.IsActive = 1 AND db.Status = 'Published'

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
        public async Task<DigitalBookAdminResponse> GetAdminDigitalBooksAsync(
     string? search,
     string? approvalStatus,
     int pageNumber,
     int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;

            pageSize = pageSize < 1 ? 10 : pageSize;

            if (pageSize > 100)
                pageSize = 100;

            var offset = (pageNumber - 1) * pageSize;

            using var connection = _db.GetConnection();

            var whereConditions = new List<string>
    {
        "db.IsActive = 1"
    };

            var parameters = new DynamicParameters();

            // SEARCH FILTER
            if (!string.IsNullOrWhiteSpace(search))
            {
                whereConditions.Add(@"
            (
                db.Title LIKE @Search
                OR db.Description LIKE @Search
            )");

                parameters.Add(
                    "Search",
                    $"%{search.Trim()}%");
            }

            // APPROVAL STATUS FILTER
            if (!string.IsNullOrWhiteSpace(approvalStatus))
            {
                whereConditions.Add(
                    "db.ApprovalStatus = @ApprovalStatus");

                parameters.Add(
                    "ApprovalStatus",
                    approvalStatus.Trim());
            }

            var whereClause =
                string.Join(" AND ", whereConditions);

            // TOTAL COUNT
            var countSql = $@"
        SELECT COUNT(*)
        FROM digitalbook db
        WHERE {whereClause};
    ";

            var totalRecords =
                await connection.ExecuteScalarAsync<int>(
                    countSql,
                    parameters);

            // DATA
            var dataSql = $@"
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
        WHERE {whereClause}
        ORDER BY db.DigitalBookId DESC
        LIMIT @PageSize OFFSET @Offset;
    ";

            parameters.Add("PageSize", pageSize);
            parameters.Add("Offset", offset);

            var data =
                await connection.QueryAsync<DigitalBook>(
                    dataSql,
                    parameters);

            var totalPages =
                (int)Math.Ceiling(
                    totalRecords / (double)pageSize);

            return new DigitalBookAdminResponse
            {
                Data = data,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
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

            if (rows > 0)
            {
                // Fetch book title + owner's email/name for the notification
                var owner = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT
                db.Title,
                u.Email,
                u.FullName
            FROM digitalbook db
            INNER JOIN users u ON db.UserId = u.UserId
            WHERE db.DigitalBookId = @DigitalBookId",
                    new { DigitalBookId = digitalBookId });

                if (owner != null)
                {
                    string toEmail = owner.Email;
                    string userName = owner.FullName;
                    string bookTitle = owner.Title;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            if (approvalStatus == DigitalBookApprovalStatus.Approved)
                            {
                                await _emailRepository.SendDigitalBookApprovedEmailAsync(
                                    toEmail, userName, bookTitle);

                                Console.WriteLine(
                                    $"[Email] Digital book approved - DigitalBookId:{digitalBookId}");
                            }
                            else if (approvalStatus == DigitalBookApprovalStatus.Rejected)
                            {
                                await _emailRepository.SendDigitalBookRejectedEmailAsync(
                                    toEmail, userName, bookTitle, rejectionReason ?? "No reason provided.");

                                Console.WriteLine(
                                    $"[Email] Digital book rejected - DigitalBookId:{digitalBookId}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(
                                $"[EmailError] Digital book approval status - DigitalBookId:{digitalBookId} - {ex.Message}");
                        }
                    });
                }
            }

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
        // GET PURCHASED (RELEASED) DIGITAL BOOKS BY CLIENT ID
        public async Task<IEnumerable<dynamic>> GetPurchasedDigitalBooksByClientIdAsync(
            int clientId)
        {
            var sql = @"
        SELECT
            ep.DigitalBookPaymentId,
            ep.DigitalBookId,
            b.Title,
            b.Description,
            b.PreviewImage,
            b.PdfFile,
            b.Price,
            ep.TotalAmount,
            ep.Currency,
            ep.Status,
            ep.PaidAt,
            ep.ReleasedAt
        FROM digitalbookpayments ep
        INNER JOIN digitalbook b
            ON b.DigitalBookId = ep.DigitalBookId
        WHERE ep.ClientId = @ClientId
          AND ep.Status = 'RELEASED'
        ORDER BY ep.ReleasedAt DESC";

            using var connection = _db.GetConnection();

            return await connection.QueryAsync(
                sql,
                new { ClientId = clientId });
        }
    }
}