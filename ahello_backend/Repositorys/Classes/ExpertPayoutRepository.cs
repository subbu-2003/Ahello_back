using Dapper;
using ahello_backend.DbContexts;
using ahello_backend.Models.Payments;
using ahello_backend.Repositorys.Interfaces;

namespace ahello_backend.Repositorys.Classes
{
    public class ExpertPayoutRepository : IExpertPayoutRepository
    {
        private readonly DbContext _db;

        public ExpertPayoutRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<ExpertPayoutAccount?> GetByUserIdAsync(int userId)
        {
            var query = @"
                SELECT *
                FROM ExpertPayoutAccounts
                WHERE UserId = @UserId
                LIMIT 1";

            using var connection = _db.GetConnection();

            return await connection.QuerySingleOrDefaultAsync<ExpertPayoutAccount>(
                query,
                new { UserId = userId });
        }

        public async Task<int> InsertAsync(ExpertPayoutAccount account)
        {
            var query = @"
                INSERT INTO ExpertPayoutAccounts
                (
                    UserId,
                    BusinessType,
                    CustomerFacingBusinessName,
                    AccountStatus,
                    CreatedAt,
                    CreatedBy
                )
                VALUES
                (
                    @UserId,
                    @BusinessType,
                    @CustomerFacingBusinessName,
                    'NOT_CREATED',
                    NOW(),
                    @CreatedBy
                );

                SELECT LAST_INSERT_ID();";

            using var connection = _db.GetConnection();

            return await connection.ExecuteScalarAsync<int>(query, account);
        }

        public async Task UpdateAccountCreatedAsync(
            int userId,
            string accountId,
            string responseJson)
        {
            var query = @"
                UPDATE ExpertPayoutAccounts
                SET
                    RazorpayAccountId = @AccountId,
                    RazorpayAccountResponseJson = @ResponseJson,
                    AccountStatus = 'ACCOUNT_CREATED',
                    FailureReason = NULL,
                    ModifiedAt = NOW()
                WHERE UserId = @UserId";

            using var connection = _db.GetConnection();

            await connection.ExecuteAsync(query, new
            {
                UserId = userId,
                AccountId = accountId,
                ResponseJson = responseJson
            });
        }

        public async Task UpdateStakeholderCreatedAsync(
            int userId,
            string stakeholderId,
            string responseJson)
        {
            var query = @"
                UPDATE ExpertPayoutAccounts
                SET
                    RazorpayStakeholderId = @StakeholderId,
                    RazorpayStakeholderResponseJson = @ResponseJson,
                    AccountStatus = 'STAKEHOLDER_CREATED',
                    FailureReason = NULL,
                    ModifiedAt = NOW()
                WHERE UserId = @UserId";

            using var connection = _db.GetConnection();

            await connection.ExecuteAsync(query, new
            {
                UserId = userId,
                StakeholderId = stakeholderId,
                ResponseJson = responseJson
            });
        }

        public async Task UpdateProductRequestedAsync(
            int userId,
            string productId,
            string responseJson)
        {
            var query = @"
                UPDATE ExpertPayoutAccounts
                SET
                    RazorpayProductId = @ProductId,
                    RazorpayProductResponseJson = @ResponseJson,
                    AccountStatus = 'PRODUCT_REQUESTED',
                    FailureReason = NULL,
                    ModifiedAt = NOW()
                WHERE UserId = @UserId";

            using var connection = _db.GetConnection();

            await connection.ExecuteAsync(query, new
            {
                UserId = userId,
                ProductId = productId,
                ResponseJson = responseJson
            });
        }

        public async Task UpdateProductActivatedAsync(
            int userId,
            string responseJson)
        {
            var query = @"
                UPDATE ExpertPayoutAccounts
                SET
                    RazorpayProductResponseJson = @ResponseJson,
                    AccountStatus = 'ACTIVE',
                    FailureReason = NULL,
                    ModifiedAt = NOW()
                WHERE UserId = @UserId";

            using var connection = _db.GetConnection();

            await connection.ExecuteAsync(query, new
            {
                UserId = userId,
                ResponseJson = responseJson
            });
        }

        public async Task SetFailedAsync(int userId, string failureReason)
        {
            var query = @"
                UPDATE ExpertPayoutAccounts
                SET
                    AccountStatus = 'FAILED',
                    FailureReason = @FailureReason,
                    ModifiedAt = NOW()
                WHERE UserId = @UserId";

            using var connection = _db.GetConnection();

            await connection.ExecuteAsync(query, new
            {
                UserId = userId,
                FailureReason = failureReason
            });
        }
    }
}
