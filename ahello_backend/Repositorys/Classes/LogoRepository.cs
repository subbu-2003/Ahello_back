using ahello_backend.DbContexts;
using ahello_backend.Models.Logos;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class LogoRepository : ILogoRepository
    {
        private readonly DbContext _db;

        public LogoRepository(DbContext db)
        {
            _db = db;
        }

        // GET ALL
        public async Task<IEnumerable<LogoGetResponse>> GetAllAsync()
        {
            using var connection = _db.GetConnection();

            var sql = @"
                SELECT
                    Id,
                    LogoName,
                    Logo,
                    Email,
                    Address,
                    Pincode,
                    MobileNumber,
                    IsActive,
                    CreatedBy,
                    CreatedAt,
                    ModifiedBy,
                    ModifiedAt
                FROM Logos
                ORDER BY Id DESC";

            return await connection.QueryAsync<LogoGetResponse>(sql);
        }
        public async Task<IEnumerable<LogoGetResponse>> GetByIsActiveAsync()
        {
            using var connection = _db.GetConnection();

            var sql = @"
        SELECT
            Id,
            LogoName,
            Logo,
            Email,
            Address,
            Pincode,
            MobileNumber,
            IsActive,
            CreatedBy,
            CreatedAt,
            ModifiedBy,
            ModifiedAt
        FROM Logos
        WHERE IsActive = 1
        ORDER BY Id DESC";

            return await connection.QueryAsync<LogoGetResponse>(sql);
        }
        // POST
        public async Task<int> CreateAsync(LogoPost model)
        {
            using var connection = _db.GetConnection();

            // If requesting active logo,
            // check whether an active logo already exists
            if (model.IsActive)
            {
                var activeExists = await connection.ExecuteScalarAsync<bool>(
                    @"SELECT EXISTS(
                SELECT 1
                FROM Logos
                WHERE IsActive = 1
            )");

                if (activeExists)
                {
                    throw new Exception(
                        "An active logo already exists. New logo must be inactive.");
                }
            }

            var sql = @"
        INSERT INTO Logos
        (
            LogoName,
            Logo,
            Email,
            Address,
            Pincode,
            MobileNumber,
            IsActive,
            CreatedBy,
            CreatedAt
        )
        VALUES
        (
            @LogoName,
            @Logo,
            @Email,
            @Address,
            @Pincode,
            @MobileNumber,
            @IsActive,
            @CreatedBy,
            NOW()
        );

        SELECT LAST_INSERT_ID();";

            var id = await connection.ExecuteScalarAsync<int>(
                sql,
                new
                {
                    model.LogoName,
                    Logo = model.LogoUrl,
                    model.Email,
                    model.Address,
                    model.Pincode,
                    model.MobileNumber,
                    model.IsActive,
                    model.CreatedBy
                });

            return id;
        }

        // PUT
        public async Task<bool> UpdateAsync(
         int id,
         LogoPut model)
        {
            using var connection = _db.GetConnection();

            var exists = await connection.ExecuteScalarAsync<bool>(
                @"SELECT EXISTS(
            SELECT 1
            FROM Logos
            WHERE Id = @Id
        )",
                new { Id = id });

            if (!exists)
            {
                return false;
            }

            // =========================================================
            // CHECK ACTIVE LOGO
            // =========================================================

            if (model.IsActive)
            {
                var anotherActiveExists =
                    await connection.ExecuteScalarAsync<bool>(
                        @"SELECT EXISTS(
                    SELECT 1
                    FROM Logos
                    WHERE IsActive = 1
                    AND Id <> @Id
                )",
                        new { Id = id });

                if (anotherActiveExists)
                {
                    throw new Exception(
                        "An active logo already exists. This logo must be inactive.");
                }
            }

            var sql = @"
        UPDATE Logos
        SET
            LogoName = @LogoName,
            Logo = @Logo,
            Email = @Email,
            Address = @Address,
            Pincode = @Pincode,
            MobileNumber = @MobileNumber,
            IsActive = @IsActive,
            ModifiedBy = @ModifiedBy,
            ModifiedAt = NOW()
        WHERE Id = @Id";

            var rows = await connection.ExecuteAsync(
                sql,
                new
                {
                    Id = id,
                    model.LogoName,
                    Logo = model.LogoUrl,
                    model.Email,
                    model.Address,
                    model.Pincode,
                    model.MobileNumber,
                    model.IsActive,
                    model.ModifiedBy
                });

            return rows > 0;
        }
        public async Task<bool> UpdateLogoIsActiveAsync(
        int id,
        LogoIsActivePut model)
        {
            using var connection = _db.GetConnection();

            var exists = await connection.ExecuteScalarAsync<bool>(
                @"SELECT EXISTS(
            SELECT 1
            FROM Logos
            WHERE Id = @Id
        )",
                new { Id = id });

            if (!exists)
            {
                return false;
            }

            // =========================================================
            // ACTIVATING
            // =========================================================

            if (model.IsActive)
            {
                var anotherActiveExists =
                    await connection.ExecuteScalarAsync<bool>(
                        @"SELECT EXISTS(
                    SELECT 1
                    FROM Logos
                    WHERE IsActive = 1
                    AND Id <> @Id
                )",
                        new { Id = id });

                if (anotherActiveExists)
                {
                    throw new Exception(
                        "An active logo already exists. Please deactivate it first.");
                }
            }

            // =========================================================
            // UPDATE
            // =========================================================

            var sql = @"
            UPDATE Logos
            SET
                IsActive = @IsActive,
                ModifiedBy = @ModifiedBy,
                ModifiedAt = NOW()
            WHERE Id = @Id";

            var rows = await connection.ExecuteAsync(
                sql,
                new
                {
                    Id = id,
                    model.IsActive,
                    model.ModifiedBy
                });

            return rows > 0;
        }
    }
}
