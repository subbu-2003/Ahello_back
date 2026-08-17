using ahello_backend.DbContexts;
using ahello_backend.Models.WelcomeTour;
using ahello_backend.Repositorys.Interfaces;
using Dapper;
using Microsoft.AspNetCore.Connections;

namespace ahello_backend.Repositorys.Classes
{
    public class WelcomeTourRepository : IWelcomeTourRepository
    {
        private readonly DbContext _db;

        public WelcomeTourRepository(DbContext db)
        {
            _db = db;
        }

        // GET ALL
        public async Task<IEnumerable<WelcomeTour>> GetAsync()
        {
            var sql = @"
                SELECT
                    welcometourId,
                    UserId,
                    ItemType,
                    ItemKey,
                    IsActive,
                    modifiedby,
                    modifiedAt
                FROM welcometour
                ORDER BY welcometourId DESC";

            using var connection = _db.GetConnection();

            return await connection.QueryAsync<WelcomeTour>(sql);
        }

        // GET BY USER ID
        public async Task<IEnumerable<WelcomeTour>> GetByUserIdAsync(
            int userId)
        {
            var sql = @"
                SELECT
                    welcometourId,
                    UserId,
                    ItemType,
                    ItemKey,
                    IsActive,
                    modifiedby,
                    modifiedAt
                FROM welcometour
                WHERE UserId = @UserId
                ORDER BY welcometourId DESC";

            using var connection = _db.GetConnection();

            return await connection.QueryAsync<WelcomeTour>(
                sql,
                new { UserId = userId });
        }

        // POST
        public async Task<int> PostAsync(
            WelcomeTour model)
        {
            var sql = @"
                INSERT INTO welcometour
                (
                    UserId,
                    ItemType,
                    ItemKey,
                    IsActive,
                    modifiedby,
                    modifiedAt
                )
                VALUES
                (
                    @UserId,
                    @ItemType,
                    @ItemKey,
                    @IsActive,
                    @ModifiedBy,
                    NOW()
                );

                SELECT LAST_INSERT_ID();";

            using var connection = _db.GetConnection();

            return await connection.ExecuteScalarAsync<int>(
                sql,
                model);
        }

        // PUT - FULL UPDATE
        public async Task<bool> PutAsync(
            WelcomeTour model)
        {
            var sql = @"
                UPDATE welcometour
                SET
                    UserId = @UserId,
                    ItemType = @ItemType,
                    ItemKey = @ItemKey,
                    IsActive = @IsActive,
                    modifiedby = @ModifiedBy,
                    modifiedAt = NOW()
                WHERE welcometourId = @WelcomeTourId";

            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(
                sql,
                model);

            return rows > 0;
        }

        // PUT BY USER ID - ONLY ISACTIVE
        public async Task<bool> PutByUserIdAsync(
            int userId,
            bool isActive,
            int modifiedBy)
        {
            var sql = @"
                UPDATE welcometour
                SET
                    IsActive = @IsActive,
                    modifiedby = @ModifiedBy,
                    modifiedAt = NOW()
                WHERE UserId = @UserId";

            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(
                sql,
                new
                {
                    UserId = userId,
                    IsActive = isActive,
                    ModifiedBy = modifiedBy
                });

            return rows > 0;
        }
        // PUT BY USER ID + ITEM KEY - ONLY ISACTIVE
        public async Task<bool> PutByUserIdAndItemKeyAsync(
            int userId,
            string itemKey,
            bool isActive,
            int modifiedBy)
        {
            var sql = @"
        UPDATE welcometour
        SET
            IsActive = @IsActive,
            modifiedby = @ModifiedBy,
            modifiedAt = NOW()
        WHERE UserId = @UserId
            AND ItemKey = @ItemKey";

            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(
                sql,
                new
                {
                    UserId = userId,
                    ItemKey = itemKey,
                    IsActive = isActive,
                    ModifiedBy = modifiedBy
                });

            return rows > 0;
        }
    }
}
