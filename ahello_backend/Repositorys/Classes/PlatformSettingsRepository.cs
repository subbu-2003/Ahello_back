using ahello_backend.DbContexts;
using ahello_backend.Models.PlatformSettings;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class PlatformSettingsRepository : IPlatformSettingsRepository
    {
        private readonly DbContext _context;

        public PlatformSettingsRepository(DbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET ALL
        // ============================================================

        public async Task<IEnumerable<PlatformSettings>> GetAllAsync()
        {
            using var connection = _context.GetConnection();

            var query = @"
                SELECT
                    PlatformSettingId,
                    FeePercentage,
                    IsActive,
                    ModifiedAt,
                    ModifiedBy
                FROM PlatformSettings
                ORDER BY PlatformSettingId DESC;
            ";

            return await connection.QueryAsync<PlatformSettings>(query);
        }
        // ============================================================
        // GET ACTIVE PLATFORM SETTINGS
        // ============================================================

        public async Task<IEnumerable<PlatformSettings>> GetActiveAsync()
        {
            using var connection = _context.GetConnection();

            var query = @"
        SELECT
            PlatformSettingId,
            FeePercentage,
            IsActive,
            ModifiedAt,
            ModifiedBy
        FROM PlatformSettings
        WHERE IsActive = 1
        ORDER BY PlatformSettingId DESC;
    ";

            return await connection.QueryAsync<PlatformSettings>(query);
        }
        // ============================================================
        // GET BY PLATFORM SETTING ID
        // ============================================================

        public async Task<PlatformSettings?> GetByIdAsync(
            int platformSettingId)
        {
            using var connection = _context.GetConnection();

            var query = @"
                SELECT
                    PlatformSettingId,
                    FeePercentage,
                    IsActive,
                    ModifiedAt,
                    ModifiedBy
                FROM PlatformSettings
                WHERE PlatformSettingId = @PlatformSettingId;
            ";

            return await connection.QueryFirstOrDefaultAsync<PlatformSettings>(
                query,
                new
                {
                    PlatformSettingId = platformSettingId
                });
        }

        // ============================================================
        // POST
        // ============================================================

        public async Task<int> CreateAsync(
            CreatePlatformSettingsRequest request)
        {
            using var connection = _context.GetConnection();

            var query = @"
                INSERT INTO PlatformSettings
                (
                    FeePercentage,
                    IsActive,
                    ModifiedAt,
                    ModifiedBy
                )
                VALUES
                (
                    @FeePercentage,
                    1,
                    NOW(),
                    @ModifiedBy
                );

                SELECT LAST_INSERT_ID();
            ";

            return await connection.ExecuteScalarAsync<int>(
                query,
                request);
        }

        // ============================================================
        // PUT - UPDATE FEE PERCENTAGE
        // ============================================================

        public async Task<bool> UpdateAsync(
            int platformSettingId,
            UpdatePlatformSettingsRequest request)
        {
            using var connection = _context.GetConnection();

            var query = @"
                UPDATE PlatformSettings
                SET
                    FeePercentage = @FeePercentage,
                    ModifiedAt = NOW(),
                    ModifiedBy = @ModifiedBy
                WHERE PlatformSettingId = @PlatformSettingId;
            ";

            var rowsAffected = await connection.ExecuteAsync(
                query,
                new
                {
                    PlatformSettingId = platformSettingId,
                    request.FeePercentage,
                    request.ModifiedBy
                });

            return rowsAffected > 0;
        }

        // ============================================================
        // PUT - UPDATE IS ACTIVE
        // ============================================================

        public async Task<bool> UpdateIsActiveAsync(
            int platformSettingId,
            UpdatePlatformSettingsStatusRequest request)
        {
            using var connection = _context.GetConnection();

            var query = @"
                UPDATE PlatformSettings
                SET
                    IsActive = @IsActive,
                    ModifiedAt = NOW(),
                    ModifiedBy = @ModifiedBy
                WHERE PlatformSettingId = @PlatformSettingId;
            ";

            var rowsAffected = await connection.ExecuteAsync(
                query,
                new
                {
                    PlatformSettingId = platformSettingId,
                    request.IsActive,
                    request.ModifiedBy
                });

            return rowsAffected > 0;
        }
    }
}