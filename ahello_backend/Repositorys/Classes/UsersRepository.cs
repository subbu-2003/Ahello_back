using ahello_backend.DbContexts;
using ahello_backend.Models.Pagination;
using ahello_backend.Models.Users;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class UsersRepository : IUsersRepository
    {
        private readonly DbContext _db;

        public UsersRepository(DbContext db)
        {
            _db = db;
        }

        // POST
        public async Task<int> PostUserAsync(UserPost post)
        {
            using var connection = _db.GetConnection();

            var sql = @"INSERT INTO users
                        (
                            CategoryId,
                            FullName,
                            Email,
                            ProfileUrl,
                            MobileNumber,
                            WhatsAppNumber,
                            Gender,
                            DateOfBirth,
                            Address,
                            City,
                            State,
                            Country,
                            Pincode,
                            Qualification,
                            Occupation,
                            CompanyName,
                            Experience,
                            SocialMediaLinks,
                            WebsiteURL,
                            Notes,
                            CreatedAt,
                            CreatedBy
                        )
                        VALUES
                        (
                            @CategoryId,
                            @FullName,
                            @Email,
                            @ProfileUrl,
                            @MobileNumber,
                            @WhatsAppNumber,
                            @Gender,
                            @DateOfBirth,
                            @Address,
                            @City,
                            @State,
                            @Country,
                            @Pincode,
                            @Qualification,
                            @Occupation,
                            @CompanyName,
                            @Experience,
                            @SocialMediaLinks,
                            @WebsiteURL,
                            @Notes,
                            NOW(),
                            @CreatedBy
                        );

                        SELECT LAST_INSERT_ID();";

            return await connection.ExecuteScalarAsync<int>(sql, post);
        }

        // GET ALL
        public async Task<IEnumerable<UserRead>> GetAllUsersAsync()
        {
            var sql = @"
            SELECT 
                u.*,
                c.CategoryName
            FROM users u
            LEFT JOIN categories c
                ON u.CategoryId = c.CategoryId
            ORDER BY u.UserId DESC";

            return await _db.GetConnection().QueryAsync<UserRead>(sql);
        }

        // GET BY ID
        public async Task<UserRead> GetUserByIdAsync(int userId)
        {
            var sql = @"
            SELECT 
                u.*,
                c.CategoryName
            FROM users u
            LEFT JOIN categories c
                ON u.CategoryId = c.CategoryId
            WHERE u.UserId = @UserId";
            return await _db.GetConnection()
                .QueryFirstOrDefaultAsync<UserRead>(sql, new { UserId = userId });
        }

        // GET BY EMAIL
        public async Task<UserRead?> GetUserByEmailAsync(string email)
        {
            var sql = @"
            SELECT 
                u.*,
                c.CategoryName
            FROM users u
            LEFT JOIN categories c
                ON u.CategoryId = c.CategoryId
            WHERE u.Email = @Email";

            return await _db.GetConnection()
                .QueryFirstOrDefaultAsync<UserRead>(sql, new { Email = email });
        }

        // UPDATE
        public async Task<bool> PutUserAsync(UserPut put)
        {
            var sql = @"UPDATE users
                        SET
                            CategoryId = @CategoryId,
                            FullName = @FullName,
                            Email = @Email,
                            ProfileUrl = @ProfileUrl,
                            MobileNumber = @MobileNumber,
                            WhatsAppNumber = @WhatsAppNumber,
                            Gender = @Gender,
                            DateOfBirth = @DateOfBirth,
                            Address = @Address,
                            City = @City,
                            State = @State,
                            Country = @Country,
                            Pincode = @Pincode,
                            Qualification = @Qualification,
                            Occupation = @Occupation,
                            CompanyName = @CompanyName,
                            Experience = @Experience,
                            SocialMediaLinks = @SocialMediaLinks,
                            WebsiteURL = @WebsiteURL,
                            Notes = @Notes,
                            ModifiedAt = NOW(),
                            ModifiedBy = @ModifiedBy
                        WHERE UserId = @UserId";

            var rows = await _db.GetConnection().ExecuteAsync(sql, put);

            return rows > 0;
        }

        // DELETE
        public async Task<bool> DeleteUserAsync(int userId)
        {
            var sql = @"DELETE FROM users
                        WHERE UserId = @UserId";

            var rows = await _db.GetConnection()
                .ExecuteAsync(sql, new { UserId = userId });

            return rows > 0;
        }
        public async Task<PagedResult<UserServiceResponse>> GetUserServicesAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? search = null)
        {
            if (pageSize > 10)
                pageSize = 10;

            if (pageSize <= 0)
                pageSize = 10;

            var offset = (pageNumber - 1) * pageSize;

            search = string.IsNullOrWhiteSpace(search)
                ? null
                : search.Trim();

            using var connection = _db.GetConnection();

            var sql = @"

                SELECT
                    u.UserId,
                    u.FullName,
                    u.ProfileUrl,
                    c.CategoryId,
                    c.CategoryName,

                    s.ServiceId,
                    st.ServiceTypeId,
                    st.ServiceTypeName,
                    s.ServiceCategoryId,
                    sc.ServiceCategoryName,
                    s.ServiceTitle,
                    s.ShortDescription,
                    s.Price,
                    s.Duration,
                    s.IntroVideo,
                    s.ThumbnailImage,

                    IFNULL(AVG(CAST(r.Rating AS DECIMAL(10,2))),0)
                        AS AverageRating

                FROM services s

                INNER JOIN users u
                    ON u.UserId = s.UserId

                LEFT JOIN categories c
                    ON c.CategoryId = u.CategoryId
                LEFT JOIN servicetypes st
                    ON st.ServiceTypeId = s.ServiceTypeId

                LEFT JOIN servicecategorydynamic sc
                ON sc.ServiceCategoryId = s.ServiceCategoryId

                LEFT JOIN bookings b
                    ON b.ServiceId = s.ServiceId

                LEFT JOIN reviews r
                    ON r.BookingId = b.BookingId

                WHERE
                (
                    @search IS NULL
                    OR LOWER(u.FullName)
                        LIKE LOWER(CONCAT('%', @search, '%'))

                    OR LOWER(s.ServiceTitle)
                        LIKE LOWER(CONCAT('%', @search, '%'))

                    OR LOWER(c.CategoryName)
                        LIKE LOWER(CONCAT('%', @search, '%'))
                     
                     OR LOWER(st.ServiceTypeName)
                        LIKE LOWER(CONCAT('%', @search, '%'))
                     
                      
                     OR s.ServiceCategoryId IN
                    (
                        SELECT ServiceCategoryId
                        FROM servicecategorydynamic
                        WHERE LOWER(ServiceCategoryName)
                        LIKE LOWER(CONCAT('%', @search, '%'))
                    )
                )

                GROUP BY
                    u.UserId,
                    u.FullName,
                    u.ProfileUrl,
                    c.CategoryId,
                    c.CategoryName,
                    s.ServiceId,
                    st.ServiceTypeId,
                    st.ServiceTypeName,
                    s.ServiceCategoryId,
                    sc.ServiceCategoryName,
                    s.ServiceTitle,
                    s.ShortDescription,
                    s.Price,
                    s.Duration,
                    s.IntroVideo,
                    s.ThumbnailImage

                ORDER BY u.UserId DESC;

                SELECT COUNT(DISTINCT u.UserId)

                FROM users u

                INNER JOIN services s
                    ON s.UserId = u.UserId

                LEFT JOIN categories c
                    ON c.CategoryId = u.CategoryId
                LEFT JOIN servicetypes st
                    ON st.ServiceTypeId = s.ServiceTypeId

                  LEFT JOIN servicecategorydynamic sc
                     ON sc.ServiceCategoryId = s.ServiceCategoryId

                WHERE
                (
                    @search IS NULL
                    OR LOWER(u.FullName)
                        LIKE LOWER(CONCAT('%', @search, '%'))

                    OR LOWER(s.ServiceTitle)
                        LIKE LOWER(CONCAT('%', @search, '%'))

                    OR LOWER(c.CategoryName)
                        LIKE LOWER(CONCAT('%', @search, '%'))

                    OR LOWER(st.ServiceTypeName)
                        LIKE LOWER(CONCAT('%', @search, '%'))
 
                   OR LOWER(sc.ServiceCategoryName)
                LIKE LOWER(CONCAT('%', @search, '%'))
                );
                ";

            var multi = await connection.QueryMultipleAsync(
                sql,
                new
                {
                    search
                });

            var rawData =
                (await multi.ReadAsync<UserServiceRaw>())
                .ToList();

            var totalCount =
                await multi.ReadFirstOrDefaultAsync<int>();

            var groupedData = rawData
                .GroupBy(x => new
                {
                    x.UserId,
                    x.FullName,
                    x.ProfileUrl,
                    x.CategoryId,
                    x.CategoryName
                })
                .Select(g => new UserServiceResponse
                {
                    UserId = g.Key.UserId,
                    FullName = g.Key.FullName,
                    ProfileUrl = g.Key.ProfileUrl,
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,

                Services = g.Take(1).Select(s => new UserServiceItem
                {
                    ServiceId = s.ServiceId,
                    ServiceTypeId = s.ServiceTypeId,
                    ServiceTypeName = s.ServiceTypeName,
                    ServiceCategoryId = s.ServiceCategoryId,
                    ServiceCategoryName = s.ServiceCategoryName,
                    ServiceTitle = s.ServiceTitle,
                    ShortDescription = s.ShortDescription,
                    Price = s.Price,
                    Duration = s.Duration,
                    IntroVideo = s.IntroVideo,
                    ThumbnailImage = s.ThumbnailImage,
                    AverageRating = s.AverageRating
                })
                .ToList()
                })
                .Skip(offset)
                .Take(pageSize)
                .ToList();

            return new PagedResult<UserServiceResponse>
            {
                TotalCount = totalCount,
                Details = groupedData
            };
        }
        public async Task<SearchResultDto> SearchUserServicesAsync(
          string? keyword,
          int pageNumber,
          int pageSize)
        {
            using var conn = _db.GetConnection();
            var sql = @"
                -- Result 1: Services (match on service fields only)
                SELECT
                    u.FullName,
                    c.CategoryName,
                    st.ServiceTypeName,
                    sc.ServiceCategoryName,
                    s.ServiceTitle,
                    s.Price
                FROM services s
                INNER JOIN users u ON u.UserId = s.UserId
                LEFT JOIN categories c ON c.CategoryId = u.CategoryId
                LEFT JOIN servicetypes st ON st.ServiceTypeId = s.ServiceTypeId
                LEFT JOIN servicecategorydynamic sc ON sc.ServiceCategoryId = s.ServiceCategoryId
                WHERE s.IsActive = 1
                AND
                (
                   @Keyword IS NULL
                   OR LOWER(s.ServiceTitle) LIKE LOWER(CONCAT('%', @Keyword, '%'))
                   OR LOWER(sc.ServiceCategoryName) LIKE LOWER(CONCAT('%', @Keyword, '%'))
                   OR LOWER(st.ServiceTypeName) LIKE LOWER(CONCAT('%', @Keyword, '%'))
                )
                ORDER BY s.ServiceId DESC
                LIMIT @PageSize OFFSET @Offset;

                -- Result 2: Experts (one row per expert)
                SELECT
                    u.FullName,
                    MAX(sc.ServiceCategoryName) AS ServiceCategoryName
                FROM services s
                INNER JOIN users u ON u.UserId = s.UserId
                LEFT JOIN servicecategorydynamic sc ON sc.ServiceCategoryId = s.ServiceCategoryId
                WHERE s.IsActive = 1
                AND @Keyword IS NOT NULL
                AND LOWER(u.FullName) LIKE LOWER(CONCAT('%', @Keyword, '%'))
                GROUP BY u.UserId, u.FullName
                LIMIT 5;

                -- Result 3: Total count for services
                SELECT COUNT(*)
                FROM services s
                INNER JOIN users u ON u.UserId = s.UserId
                LEFT JOIN categories c ON c.CategoryId = u.CategoryId
                LEFT JOIN servicetypes st ON st.ServiceTypeId = s.ServiceTypeId
                LEFT JOIN servicecategorydynamic sc ON sc.ServiceCategoryId = s.ServiceCategoryId
                WHERE s.IsActive = 1
                AND
                (
                   @Keyword IS NULL
                   OR LOWER(s.ServiceTitle) LIKE LOWER(CONCAT('%', @Keyword, '%'))
                   OR LOWER(sc.ServiceCategoryName) LIKE LOWER(CONCAT('%', @Keyword, '%'))
                   OR LOWER(st.ServiceTypeName) LIKE LOWER(CONCAT('%', @Keyword, '%'))
                )
                ";

            var multi = await conn.QueryMultipleAsync(sql, new
            {
                Keyword = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim(),
                PageSize = pageSize,
                Offset = (pageNumber - 1) * pageSize
            });

            var services = (await multi.ReadAsync<UserSearchDto>()).ToList();
            var experts = (await multi.ReadAsync<ExpertSearchDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return new SearchResultDto          // ← return typed DTO, not anonymous object
            {
                TotalCount = totalCount,
                Details = services,
                Experts = experts
            };
        }
    }
}