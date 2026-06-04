using ahello_backend.DbContexts;
using ahello_backend.Models.Users;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class UserDropdownOptionRepository
        : IUserDropdownOptionRepository
    {
        private readonly DbContext _db;

        public UserDropdownOptionRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(
            CreateUserDropdownOption model)
        {
            var sql = @"
            INSERT INTO userdropdownoptions
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
                @IsActive,
                NOW(),
                @CreatedBy
            );";

            using var connection = _db.GetConnection();

            int rows = 0;

            foreach (var opt in model.Options)
            {
                rows += await connection.ExecuteAsync(
                    sql,
                    new
                    {
                        model.UserFieldId,
                        model.UserId,
                        opt.OptionValue,
                        opt.OptionLabel,
                        opt.IsActive,
                        model.CreatedBy
                    });
            }

            return rows;
        }

        public async Task<bool> UpdateAsync(
            UpdateUserDropdownOption model)
        {
            var sql = @"
            UPDATE userdropdownoptions
            SET
                OptionValue = @OptionValue,
                OptionLabel = @OptionLabel,
                IsActive = @IsActive,
                ModifiedDate = NOW(),
                ModifiedBy = @ModifiedBy
            WHERE UserDropDownId = @UserDropDownId";

            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(sql, model);

            return rows > 0;
        }

        public async Task<GetUserDropdownOption> GetByIdAsync(
            int optionId)
        {
            var sql = @"
            SELECT *
            FROM userdropdownoptions
            WHERE UserDropDownId = @optionId";

            using var connection = _db.GetConnection();

            return await connection
                .QueryFirstOrDefaultAsync<GetUserDropdownOption>(
                    sql,
                    new { optionId });
        }

        public async Task<IEnumerable<GetUserDropdownOption>>
            GetByFieldIdAsync(
                int userFieldId,
                int? userId)
        {
            var sql = @"
            SELECT *
            FROM userdropdownoptions
            WHERE UserFieldId = @userFieldId
            AND (@userId IS NULL OR UserId = @userId)
            AND IsActive = 1";

            using var connection = _db.GetConnection();

            return await connection
                .QueryAsync<GetUserDropdownOption>(
                    sql,
                    new
                    {
                        userFieldId,
                        userId
                    });
        }
    }
}
