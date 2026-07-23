using ahello_backend.DbContexts;
using ahello_backend.Models.Pagination;
using ahello_backend.Models.User;
using ahello_backend.Models.Users;
using ahello_backend.Repositorys.Interfaces;
using Dapper;
using System.Data;

namespace ahello_backend.Repositorys.Classes
{
    public class UserDynamicRepository : IUserDynamicRepository
    {
        private readonly DbContext _db;

        public UserDynamicRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<UserDynamicGetResponse>> GetAllAsync()
        {
            using var connection = _db.GetConnection();

            var users = (await connection.QueryAsync<UserDynamicGetResponse>(
                @"SELECT
                u.UserId,
                u.CategoryId,
                c.CategoryName,
                u.FullName,
                u.Email,
                u.ProfileUrl,
                u.MobileNumber,
                u.WhatsAppNumber,
                u.Gender,
                u.DateOfBirth,
                u.Address,
                u.City,
                u.State,
                u.Country,
                u.Pincode,
                u.Qualification,
                u.Occupation,
                u.CompanyName,
                u.Experience,
                u.SocialMediaLinks,
                u.WebsiteURL,
                u.Notes,
                u.CreatedBy,
                u.CreatedAt
            FROM users u
            LEFT JOIN categories c
                ON u.CategoryId = c.CategoryId
            ORDER BY u.UserId DESC")).ToList();

            foreach (var user in users)
            {
                var fields = await connection.QueryAsync<UserDynamicFieldResponse>(
                    @"SELECT
                        uf.UserFieldId,
                        uf.FieldName,
                        uf.FieldCode,
                        ufv.FieldValue
                      FROM userfieldvalues ufv
                      INNER JOIN userfields uf
                        ON ufv.UserFieldId = uf.UserFieldId
                      WHERE ufv.UserId = @UserId",
                    new { UserId = user.UserId });

                user.Fields = fields.ToList();
            }

            return users;
        }

        public async Task<UserDynamicPaginationResponse>
        GetAllPaginationAsync(
        int pageNumber,
        int pageSize,
        string? search)
        {
            using var connection = _db.GetConnection();

            int skip = (pageNumber - 1) * pageSize;

            var whereClause = "";

            if (!string.IsNullOrWhiteSpace(search))
            {
                whereClause = @"
            WHERE
                u.FullName LIKE @Search
                OR u.Email LIKE @Search
                OR u.MobileNumber LIKE @Search
                OR u.City LIKE @Search
                OR u.Country LIKE @Search";
            }

            var totalCount = await connection.ExecuteScalarAsync<int>(
                $@"SELECT COUNT(*)
           FROM users
           {whereClause}",
                new
                {
                    Search = $"%{search}%"
                });

            var users = (await connection.QueryAsync<UserDynamicGetResponse>(
               $@"SELECT
                    u.UserId,
                    u.CategoryId,
                    c.CategoryName,
                    u.FullName,
                    u.Email,
                    u.ProfileUrl,
                    u.MobileNumber,
                    u.WhatsAppNumber,
                    u.Gender,
                    u.DateOfBirth,
                    u.Address,
                    u.City,
                    u.State,
                    u.Country,
                    u.Pincode,
                    u.Qualification,
                    u.Occupation,
                    u.CompanyName,
                    u.Experience,
                    u.SocialMediaLinks,
                    u.WebsiteURL,
                    u.Notes,
                    u.CreatedBy,
                    u.CreatedAt
                FROM users u
                LEFT JOIN categories c
                    ON u.CategoryId = c.CategoryId
                {whereClause}
                ORDER BY u.UserId DESC
                LIMIT @PageSize OFFSET @Skip",
                new
                {
                    Search = $"%{search}%",
                    PageSize = pageSize,
                    Skip = skip
                })).ToList();

            foreach (var user in users)
            {
                var fields = await connection.QueryAsync<UserDynamicFieldResponse>(
                    @"SELECT
                uf.UserFieldId,
                uf.FieldName,
                uf.FieldCode,
                ufv.FieldValue
              FROM userfieldvalues ufv
              INNER JOIN userfields uf
                ON ufv.UserFieldId = uf.UserFieldId
              WHERE ufv.UserId = @UserId",
                    new { UserId = user.UserId });

                user.Fields = fields.ToList();

                var dropdownOptions =
                await connection.QueryAsync<UserDropdownOptionResponse>(
                @"SELECT DISTINCT
                    udo.UserDropDownId,
                    udo.UserFieldId,
                    udo.UserId,
                    udo.OptionValue,
                    udo.OptionLabel,
                    udo.IsActive

                FROM userdropdownoptions udo

                INNER JOIN userfieldvalues ufv
                    ON udo.UserFieldId = ufv.UserFieldId
                    AND udo.UserId = ufv.UserId

                WHERE udo.UserId = @UserId",
                new { UserId = user.UserId });

                user.DropdownOptions = dropdownOptions.ToList();
            }

            return new UserDynamicPaginationResponse
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = users
            };
        }

        public async Task<UserDynamicGetResponse> GetByIdAsync(int userId)
        {
            using var connection = _db.GetConnection();

            var user = await connection.QueryFirstOrDefaultAsync<UserDynamicGetResponse>(
                @"SELECT
                u.UserId,
                u.CategoryId,
                c.CategoryName,
                u.FullName,
                u.Email,
                u.ProfileUrl,
                u.MobileNumber,
                u.WhatsAppNumber,
                u.Gender,
                u.DateOfBirth,
                u.Address,
                u.City,
                u.State,
                u.Country,
                u.Pincode,
                u.Qualification,
                u.Occupation,
                u.CompanyName,
                u.Experience,
                u.SocialMediaLinks,
                u.WebsiteURL,
                u.Notes,
                u.CreatedBy,
                u.CreatedAt
            FROM users u
            LEFT JOIN categories c
                ON u.CategoryId = c.CategoryId
            WHERE u.UserId = @UserId",
                new { UserId = userId });

            if (user == null)
                return null;

            var fields = await connection.QueryAsync<UserDynamicFieldResponse>(
                @"SELECT
                    uf.UserFieldId,
                    uf.FieldName,
                    uf.FieldCode,
                    ufv.FieldValue
                  FROM userfieldvalues ufv
                  INNER JOIN userfields uf
                    ON ufv.UserFieldId = uf.UserFieldId
                  WHERE ufv.UserId = @UserId",
                new { UserId = userId });

            user.Fields = fields.ToList();

            var dropdownOptions =
            await connection.QueryAsync<UserDropdownOptionResponse>(
            @"SELECT
                UserDropDownId,
                UserFieldId,
                UserId,
                OptionValue,
                OptionLabel,
                IsActive
            FROM userdropdownoptions
            WHERE UserId = @UserId
            AND IsActive = 1",
            new { UserId = user.UserId });

            user.DropdownOptions = dropdownOptions.ToList();

            return user;
        }
        public async Task<IEnumerable<UserDynamicGetResponse>>
        GetByCategoryIdAsync(int categoryId)
        {
            using var connection = _db.GetConnection();

            var users = (await connection.QueryAsync<UserDynamicGetResponse>(
            @"SELECT
            u.UserId,
            u.CategoryId,
            c.CategoryName,
            u.FullName,
            u.Email,
            u.ProfileUrl,
            u.MobileNumber,
            u.WhatsAppNumber,
            u.Gender,
            u.DateOfBirth,
            u.Address,
            u.City,
            u.State,
            u.Country,
            u.Pincode,
            u.Qualification,
            u.Occupation,
            u.CompanyName,
            u.Experience,
            u.SocialMediaLinks,
            u.WebsiteURL,
            u.Notes,
            u.CreatedBy,
            u.CreatedAt
          FROM users u
          LEFT JOIN categories c
            ON u.CategoryId = c.CategoryId
          WHERE u.CategoryId = @CategoryId
          ORDER BY u.UserId DESC",
                new { CategoryId = categoryId })).ToList();

            foreach (var user in users)
            {
                var fields = await connection.QueryAsync<UserDynamicFieldResponse>(
                    @"SELECT
                uf.UserFieldId,
                uf.FieldName,
                uf.FieldCode,
                ufv.FieldValue
              FROM userfieldvalues ufv
              INNER JOIN userfields uf
                ON ufv.UserFieldId = uf.UserFieldId
              WHERE ufv.UserId = @UserId",
                    new { UserId = user.UserId });

                user.Fields = fields.ToList();
                var dropdownOptions =
                await connection.QueryAsync<UserDropdownOptionResponse>(
                @"SELECT DISTINCT
                    udo.UserDropDownId,
                    udo.UserFieldId,
                    udo.UserId,
                    udo.OptionValue,
                    udo.OptionLabel,
                    udo.IsActive

                FROM userdropdownoptions udo

                INNER JOIN userfieldvalues ufv
                    ON udo.UserFieldId = ufv.UserFieldId
                    AND udo.UserId = ufv.UserId

                WHERE udo.UserId = @UserId",
                new { UserId = user.UserId });

                user.DropdownOptions = dropdownOptions.ToList();
            }

            return users;
        }
        public async Task<int> CreateAsync(UserDynamicPost model)
        {
            using var connection = _db.GetConnection();

            connection.Open();

            using var tx = connection.BeginTransaction();

            try
            {
                await ValidateDuplicateFieldsAsync(
               connection,
               model.Email,
               model.MobileNumber,
               model.WhatsAppNumber,
               null,
               tx);
                var userSql = @"
                    INSERT INTO users
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

                var userId = await connection.ExecuteScalarAsync<int>(
                userSql,
                new
                {
                    CategoryId = model.CategoryId == 0 ? (int?)null : model.CategoryId,  // ✅ 0 → NULL
                    model.FullName,
                    model.Email,
                    model.ProfileUrl,
                    model.MobileNumber,
                    model.WhatsAppNumber,
                    Gender = string.IsNullOrWhiteSpace(model.Gender)
                                ? (object)DBNull.Value
                                : model.Gender,

                    DateOfBirth = model.DateOfBirth == null
                                ? (object)DBNull.Value
                                : model.DateOfBirth,
                    model.Address,
                    model.City,
                    model.State,
                    model.Country,
                    model.Pincode,
                    model.Qualification,
                    model.Occupation,
                    model.CompanyName,
                    model.Experience,
                    model.SocialMediaLinks,
                    model.WebsiteURL,
                    model.Notes,
                    model.CreatedBy
                },
                tx);

                if (model.Fields != null && model.Fields.Any())
                {
                    var fieldSql = @"
                        INSERT INTO userfieldvalues
                        (
                            UserId,
                            FieldCode,
                            UserFieldId,
                            FieldValue,
                            CreatedDate,
                            CreatedBy,
                            CreatedAt
                        )
                        SELECT
                            @UserId,
                            uf.FieldCode,
                            @UserFieldId,
                            @FieldValue,
                            NOW(),
                            @CreatedBy,
                            CURRENT_TIMESTAMP
                        FROM userfields uf
                        WHERE uf.UserFieldId = @UserFieldId;";

                    foreach (var field in model.Fields)
                    {
                        await connection.ExecuteAsync(
                            fieldSql,
                            new
                            {
                                UserId = userId,
                                UserFieldId = field.UserFieldId,
                                FieldValue = field.FieldValue,
                                model.CreatedBy
                            },
                            tx);
                    }
                }

                tx.Commit();
                return userId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<bool> UpdateAsync(int userId, UserDynamicPut model)
        {
            using var connection = _db.GetConnection();

            connection.Open();

            using var tx = connection.BeginTransaction();

            try
            {
                // Required field validation
                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    throw new Exception("Email is required.");
                }

                if (string.IsNullOrWhiteSpace(model.MobileNumber))
                {
                    throw new Exception("Mobile number is required.");
                }
                await ValidateDuplicateFieldsAsync(
                connection,
                model.Email,
                model.MobileNumber,
                model.WhatsAppNumber,
                userId,
                tx);
                var updateSql = @"
                    UPDATE users
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

                var rows = await connection.ExecuteAsync(
                    updateSql,
                    new
                    {
                        UserId = userId,
                        CategoryId = model.CategoryId == 0 ? (int?)null: model.CategoryId,
                        model.FullName,
                        model.Email,
                        model.ProfileUrl,
                        model.MobileNumber,
                        model.WhatsAppNumber,
                        model.Gender,
                        model.DateOfBirth,
                        model.Address,
                        model.City,
                        model.State,
                        model.Country,
                        model.Pincode,
                        model.Qualification,
                        model.Occupation,
                        model.CompanyName,
                        model.Experience,
                        model.SocialMediaLinks,
                        model.WebsiteURL,
                        model.Notes,
                        model.ModifiedBy
                    },
                    tx);

                if (rows <= 0)
                {
                    tx.Rollback();
                    return false;
                }

                //await connection.ExecuteAsync(
                //    @"DELETE FROM userfieldvalues
                //      WHERE UserId = @UserId",
                //    new { UserId = userId },
                //    tx);

                foreach (var field in model.Fields)
                {
                    var exists = await connection.ExecuteScalarAsync<int>(
                    @"SELECT COUNT(*)
                      FROM userfieldvalues
                      WHERE UserId = @UserId
                      AND UserFieldId = @UserFieldId",
                    new
                    {
                        UserId = userId,
                        UserFieldId = field.UserFieldId
                    },
                    tx);

                    if (exists > 0)
                    {
                        var fieldAffected = await connection.ExecuteAsync(
                        @"UPDATE userfieldvalues
                          SET FieldValue = @FieldValue
                          WHERE UserId = @UserId
                          AND UserFieldId = @UserFieldId",
                        new
                        {
                            UserId = userId,
                            UserFieldId = field.UserFieldId,
                            FieldValue = field.FieldValue
                        },
                        tx);

                        Console.WriteLine($"Field Updated Rows = {fieldAffected}");
                    }
                    else
                    {
                        await connection.ExecuteAsync(
                        @"INSERT INTO userfieldvalues
                          (
                              UserId,
                              UserFieldId,
                              FieldValue,
                              CreatedDate,
                              CreatedBy
                          )
                          VALUES
                          (
                              @UserId,
                              @UserFieldId,
                              @FieldValue,
                              NOW(),
                              @ModifiedBy
                          )",
                        new
                        {
                            UserId = userId,
                            UserFieldId = field.UserFieldId,
                            FieldValue = field.FieldValue,
                            model.ModifiedBy
                        },
                        tx);
                    }
                }
                //// Delete old dropdowns
                //await connection.ExecuteAsync(
                //@"DELETE FROM userdropdownoptions
                //    WHERE UserId = @UserId",
                //new { UserId = userId },
                //tx);

                // Insert new dropdowns
                foreach (var field in model.Fields)
                {
                    foreach (var option in field.DropDownOptions)
                    {
                        if (option.UserDropDownId.HasValue)
                        {
                           var affected = await connection.ExecuteAsync(
                            @"UPDATE userdropdownoptions
                            SET
                                OptionValue = @OptionValue,
                                OptionLabel = @OptionLabel,
                                IsActive = @IsActive,
                                ModifiedDate = NOW(),
                                ModifiedBy = @ModifiedBy
                            WHERE UserDropDownId = @UserDropDownId",
                            new
                            {
                                option.OptionValue,
                                option.OptionLabel,
                                option.IsActive,
                                option.UserDropDownId,
                                model.ModifiedBy
                            },
                            tx);
                        }
                        else
                        {
                            await connection.ExecuteAsync(
                            @"INSERT INTO userdropdownoptions
                              (
                                  UserFieldId,
                                  UserId,
                                  OptionValue,
                                  OptionLabel,
                                  IsActive,
                                  CreatedDate,
                                  CreatedBy
                              )
                              VALUES
                              (
                                  @UserFieldId,
                                  @UserId,
                                  @OptionValue,
                                  @OptionLabel,
                                  1,
                                  NOW(),
                                  @ModifiedBy
                              )",
                            new
                            {
                                UserFieldId = field.UserFieldId,
                                UserId = userId,
                                option.OptionValue,
                                option.OptionLabel,
                                model.ModifiedBy
                            },
                            tx);
                        }
                    }
                }
                tx.Commit();

                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            using var connection = _db.GetConnection();

            connection.Open();

            using var tx = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync(
                    @"DELETE FROM userfieldvalues
                      WHERE UserId = @UserId",
                    new { UserId = userId },
                    tx);

                var rows = await connection.ExecuteAsync(
                    @"DELETE FROM users
                      WHERE UserId = @UserId",
                    new { UserId = userId },
                    tx);

                tx.Commit();

                return rows > 0;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
        public async Task<UserProfileResponse> GetUserProfileAsync(
            int userId,
            int pageNumber,
            int pageSize,
            string search)
        {
            using var con = _db.GetConnection();

            // USER DETAILS + CATEGORY + RATINGS
            var userQuery = @"
                SELECT 
                    u.UserId,
                    u.CategoryId,
                    c.CategoryName,
                    u.FullName,
                    u.Email,
                    u.MobileNumber,
                    u.WhatsAppNumber,
                    u.Notes,
                    u.ProfileUrl,

                    IFNULL(AVG(CAST(r.Rating AS DECIMAL(10,2))), 0) AS AverageRating,
                    COUNT(r.ReviewId) AS TotalRatingCount

                FROM users u

                LEFT JOIN categories c
                    ON c.CategoryId = u.CategoryId

                LEFT JOIN services s
                    ON s.UserId = u.UserId

                LEFT JOIN bookings b
                    ON b.ServiceId = s.ServiceId

                LEFT JOIN reviews r
                    ON r.BookingId = b.BookingId

                WHERE u.UserId = @UserId

                GROUP BY
                    u.UserId,
                    u.CategoryId,
                    c.CategoryName,
                    u.FullName,
                    u.Email,
                    u.MobileNumber,
                    u.WhatsAppNumber,
                    u.Notes,
                    u.ProfileUrl;
            ";

            var response = await con.QueryFirstOrDefaultAsync<UserProfileResponse>(
                userQuery,
                new { UserId = userId });

            if (response == null)
                return null;

            // TOTAL SERVICE COUNT WITH SEARCH
            var totalCount = await con.ExecuteScalarAsync<int>(@"
                   SELECT COUNT(*)

                FROM services s

                LEFT JOIN servicecategorydynamic sc
                    ON sc.ServiceCategoryId = s.ServiceCategoryId

                WHERE s.UserId = @UserId
                AND s.IsActive = 1
                AND
                (
                    @Search IS NULL
                    OR @Search = ''
                    OR s.ServiceTitle LIKE CONCAT('%', @Search, '%')
                    OR s.ShortDescription LIKE CONCAT('%', @Search, '%')
                    OR s.Tags LIKE CONCAT('%', @Search, '%')
                    OR s.Language LIKE CONCAT('%', @Search, '%')
                    OR s.Status LIKE CONCAT('%', @Search, '%')
                    OR sc.ServiceCategoryName LIKE CONCAT('%', @Search, '%')
                );",
                new
                {
                    UserId = userId,
                    Search = search
                });

            // SERVICES WITH SEARCH + PAGINATION
            var services = await con.QueryAsync<UserProfileServiceItem>(@"
                SELECT
            s.ServiceId,
            s.ServiceTypeId,
            s.ServiceCategoryId,
            sc.ServiceCategoryName,
            s.ServiceTitle,
            s.Price,
            s.Duration,
            s.ShortDescription,
            s.FullDescription,
            s.Tags,
            s.Language,
            s.ThumbnailImage,
            s.BannerImage,
            s.IntroVideo,
            s.Status

        FROM services s

        LEFT JOIN servicecategorydynamic sc
            ON sc.ServiceCategoryId = s.ServiceCategoryId

        WHERE s.UserId = @UserId
        AND s.IsActive = 1
        AND
        (
            @Search IS NULL
            OR @Search = ''
            OR s.ServiceTitle LIKE CONCAT('%', @Search, '%')
            OR s.ShortDescription LIKE CONCAT('%', @Search, '%')
            OR s.Tags LIKE CONCAT('%', @Search, '%')
            OR s.Language LIKE CONCAT('%', @Search, '%')
            OR s.Status LIKE CONCAT('%', @Search, '%')
            OR sc.ServiceCategoryName LIKE CONCAT('%', @Search, '%')
        )

        ORDER BY s.ServiceId DESC

        LIMIT @PageSize OFFSET @Offset;",
                new
                {
                    UserId = userId,
                    Search = search,
                    PageSize = pageSize,
                    Offset = (pageNumber - 1) * pageSize
                });

            response.Services = new PagedResult<UserProfileServiceItem>
            {
                TotalCount = totalCount,
                Details = services
            };

            return response;
        }
        // ADD THIS METHOD INSIDE UserDynamicRepository.cs

        // ✅ KEEP ONLY THIS ONE — remove the other copy at the top
        private async Task ValidateDuplicateFieldsAsync(
            IDbConnection connection,
            string email,
            string mobileNumber,
            string whatsAppNumber,
            int? userId = null,
            IDbTransaction? tx = null)
        {
            var sql = @"
        SELECT
            UserId,
            Email,
            MobileNumber,
            WhatsAppNumber
        FROM users
        WHERE
        (
            LOWER(Email) = LOWER(@Email)
            OR MobileNumber = @MobileNumber
            OR WhatsAppNumber = @WhatsAppNumber
        )";

            // Exclude the current user on PUT (userId has value)
            if (userId.HasValue)
            {
                sql += " AND UserId != @UserId";
            }

            var users = await connection.QueryAsync<dynamic>(
                sql,
                new
                {
                    Email = email,
                    MobileNumber = mobileNumber,
                    WhatsAppNumber = whatsAppNumber,
                    UserId = userId
                },
                tx);

            foreach (var existingUser in users)
            {
                string existingEmail = existingUser.Email?.ToString() ?? "";
                string existingMobile = existingUser.MobileNumber?.ToString() ?? "";
                string existingWhatsapp = existingUser.WhatsAppNumber?.ToString() ?? "";

                if (!string.IsNullOrWhiteSpace(email) &&
                    existingEmail.Equals(email, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Email already exists.");
                }

                if (!string.IsNullOrWhiteSpace(mobileNumber) &&
                    existingMobile == mobileNumber)
                {
                    throw new Exception("Mobile number already exists.");
                }

                if (!string.IsNullOrWhiteSpace(whatsAppNumber) &&
                    existingWhatsapp == whatsAppNumber)
                {
                    throw new Exception("WhatsApp number already exists.");
                }
            }
        }
    }
}