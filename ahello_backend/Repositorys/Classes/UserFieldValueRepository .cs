using ahello_backend.DbContexts;
using ahello_backend.Models.Users;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class UserFieldValueRepository : IUserFieldValueRepository
    {
        private readonly DbContext _db;

        public UserFieldValueRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(CreateUserFieldValue model)
        {
            var sql = @"INSERT INTO UserFieldValues
                        (
                            UserId,
                            FieldCode,
                            UserFieldId,
                            FieldValue,
                            CreatedDate,
                            CreatedBy,
                            CreatedAt
                        )
                        VALUES
                        (
                            @UserId,
                            @FieldCode,
                            @UserFieldId,
                            @FieldValue,
                            NOW(),
                            @CreatedBy,
                            CURRENT_TIMESTAMP
                        );

                        SELECT LAST_INSERT_ID();";

            using var connection = _db.GetConnection();

            return await connection.ExecuteScalarAsync<int>(sql, model);
        }

        public async Task<bool> UpdateAsync(UpdateUserFieldValue model)
        {
            var sql = @"UPDATE UserFieldValues
                        SET
                            FieldValue = @FieldValue,
                            ModifiedBy = @ModifiedBy,
                            ModifiedAt = CURRENT_TIMESTAMP
                        WHERE UserFieldValueId = @UserFieldValueId";

            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(sql, model);

            return rows > 0;
        }

        public async Task<IEnumerable<UserFieldValue>> GetByUserAsync(int userId)
        {
            var sql = @"SELECT *
                        FROM UserFieldValues
                        WHERE UserId = @UserId
                        ORDER BY UserFieldValueId DESC";

            using var connection = _db.GetConnection();

            return await connection.QueryAsync<UserFieldValue>(
                sql,
                new { UserId = userId }
            );
        }

        public async Task<UserFieldValue> GetByIdAsync(int userFieldValueId)
        {
            var sql = @"SELECT *
                        FROM UserFieldValues
                        WHERE UserFieldValueId = @UserFieldValueId";

            using var connection = _db.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<UserFieldValue>(
                sql,
                new { UserFieldValueId = userFieldValueId }
            );
        }
    }
}
