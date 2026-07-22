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
                    FeeType,
                    FeePercentage,
                    FeeAmount,
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
                    FeeType,
                    FeePercentage,
                    FeeAmount,
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
                    FeeType,
                    FeePercentage,
                    FeeAmount,
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
            ValidateFee(request.FeeType, request.FeePercentage, request.FeeAmount);

            using var connection = _context.GetConnection();

            var query = @"
                INSERT INTO PlatformSettings
                (
                    FeeType,
                    FeePercentage,
                    FeeAmount,
                    IsActive,
                    ModifiedAt,
                    ModifiedBy
                )
                VALUES
                (
                    @FeeType,
                    @FeePercentage,
                    @FeeAmount,
                    1,
                    NOW(),
                    @ModifiedBy
                );

                SELECT LAST_INSERT_ID();
            ";

            return await connection.ExecuteScalarAsync<int>(
                query,
                new
                {
                    request.FeeType,
                    request.FeePercentage,
                    request.FeeAmount,
                    request.ModifiedBy
                });
        }

        // ============================================================
        // PUT - UPDATE FEE PERCENTAGE
        // ============================================================

        public async Task<bool> UpdateAsync(
            int platformSettingId,
            UpdatePlatformSettingsRequest request)
        {
            ValidateFee(request.FeeType, request.FeePercentage, request.FeeAmount);

            using var connection = _context.GetConnection();

            var query = @"
                UPDATE PlatformSettings
                SET
                    FeeType = @FeeType,
                    FeePercentage = @FeePercentage,
                    FeeAmount = @FeeAmount,
                    ModifiedAt = NOW(),
                    ModifiedBy = @ModifiedBy
                WHERE PlatformSettingId = @PlatformSettingId;
            ";

            var rowsAffected = await connection.ExecuteAsync(
                query,
                new
                {
                    PlatformSettingId = platformSettingId,
                    request.FeeType,
                    request.FeePercentage,
                    request.FeeAmount,
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
        public async Task<PlatformSettings?> GetActiveSettingAsync()
        {
            using var connection = _context.GetConnection();

            var query = @"
        SELECT
            PlatformSettingId,
            FeeType,
            FeePercentage,
            FeeAmount,
            IsActive,
            ModifiedAt,
            ModifiedBy
        FROM PlatformSettings
        WHERE IsActive = 1
        ORDER BY PlatformSettingId DESC
        LIMIT 1;
    ";

            return await connection.QueryFirstOrDefaultAsync<PlatformSettings>(query);
        }
        // ============================================================
        // VALIDATE FEE
        // ============================================================

        private static void ValidateFee(
            string feeType,
            decimal? feePercentage,
            decimal? feeAmount)
        {
            if (string.IsNullOrWhiteSpace(feeType))
            {
                throw new ArgumentException(
                    "FeeType is required.");
            }

            feeType = feeType.ToLower();

            if (feeType != "percentage" && feeType != "amount")
            {
                throw new ArgumentException(
                    "FeeType must be either 'percentage' or 'amount'.");
            }

            if (feeType == "percentage")
            {
                if (!feePercentage.HasValue)
                {
                    throw new ArgumentException(
                        "FeePercentage is required when FeeType is 'percentage'.");
                }

                if (feePercentage < 0)
                {
                    throw new ArgumentException(
                        "FeePercentage cannot be negative.");
                }

                if (feeAmount.HasValue)
                {
                    throw new ArgumentException(
                        "FeeAmount must be null when FeeType is 'percentage'.");
                }
            }

            if (feeType == "amount")
            {
                if (!feeAmount.HasValue)
                {
                    throw new ArgumentException(
                        "FeeAmount is required when FeeType is 'amount'.");
                }

                if (feeAmount < 0)
                {
                    throw new ArgumentException(
                        "FeeAmount cannot be negative.");
                }

                if (feePercentage.HasValue)
                {
                    throw new ArgumentException(
                        "FeePercentage must be null when FeeType is 'amount'.");
                }
            }
        }
    }
}