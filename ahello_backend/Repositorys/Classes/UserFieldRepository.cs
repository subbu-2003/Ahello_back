using ahello_backend.DbContexts;
using ahello_backend.Models.Users;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class UserFieldRepository : IUserFieldRepository
    {
        private readonly DbContext _db;

        public UserFieldRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(CreateUserField model)
        {
            var sql = @"INSERT INTO UserFields
                        (
                            UserId,
                            FieldName,
                            FieldCode,
                            Placeholder,
                            IsRequired,
                            IsActive,
                            DataTypeId,
                            CreatedBy
                        )
                        VALUES
                        (
                            @UserId,
                            @FieldName,
                            @FieldCode,
                            @Placeholder,
                            @IsRequired,
                            1,
                            @DataTypeId,
                            @CreatedBy
                        );

                        SELECT LAST_INSERT_ID();";

            using var connection = _db.GetConnection();

            return await connection.ExecuteScalarAsync<int>(sql, model);
        }

        public async Task<bool> UpdateAsync(UpdateUserField model)
        {
            var sql = @"UPDATE UserFields
                        SET
                            UserId = @UserId,
                            FieldName = @FieldName,
                            FieldCode = @FieldCode,
                            Placeholder = @Placeholder,
                            IsRequired = @IsRequired,
                            IsActive = @IsActive,
                            DataTypeId = @DataTypeId,
                            ModifiedBy = @ModifiedBy
                        WHERE UserFieldId = @UserFieldId";

            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(sql, model);

            return rows > 0;
        }

        public async Task<IEnumerable<UserField>>
       GetByUserAsync(int userId)
        {
            var sql = @"

    SELECT

        uf.UserFieldId,
        uf.UserId,
        uf.FieldName,
        uf.FieldCode,
        uf.Placeholder,
        uf.IsRequired,
        uf.IsActive,
        uf.DataTypeId,
        uf.CreatedBy,
        uf.CreatedAt,
        uf.ModifiedBy,
        uf.ModifiedAt,

        udo.UserDropDownId,
        udo.UserFieldId,
        udo.UserId,
        udo.OptionValue,
        udo.OptionLabel,
        udo.IsActive,
        udo.CreatedDate,
        udo.CreatedBy,
        udo.ModifiedDate,
        udo.ModifiedBy

    FROM userfields uf

    LEFT JOIN userdropdownoptions udo
        ON uf.UserFieldId = udo.UserFieldId
        AND udo.IsActive = 1

    WHERE uf.UserId = @UserId
    AND uf.IsActive = 1

    ORDER BY uf.UserFieldId DESC";

            using var connection = _db.GetConnection();

            var dictionary =
                new Dictionary<int, UserField>();

            var result = await connection.QueryAsync
            <
                UserField,
                GetUserDropdownOption,
                UserField
            >
            (
                sql,
                (field, dropdown) =>
                {
                    if (!dictionary.TryGetValue(
                            field.UserFieldId,
                            out var existingField))
                    {
                        existingField = field;

                        existingField.DropdownOptions =
                            new List<GetUserDropdownOption>();

                        dictionary.Add(
                            existingField.UserFieldId,
                            existingField);
                    }

                    if (dropdown != null &&
                        dropdown.UserDropDownId > 0)
                    {
                        existingField.DropdownOptions
                            .Add(dropdown);
                    }

                    return existingField;
                },
                new { UserId = userId },
                splitOn: "UserDropDownId"
            );

            return dictionary.Values;
        }

        public async Task<UserField> GetByIdAsync(int userFieldId)
        {
            var sql = @"SELECT *
                        FROM UserFields
                        WHERE UserFieldId = @UserFieldId";

            using var connection = _db.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<UserField>(
                sql,
                new { UserFieldId = userFieldId }
            );
        }

        public async Task<bool> DeleteAsync(int userFieldId, string modifiedBy)
        {
            var sql = @"UPDATE UserFields
                        SET
                            IsActive = 0,
                            ModifiedBy = @ModifiedBy
                        WHERE UserFieldId = @UserFieldId";

            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(sql,
                new
                {
                    UserFieldId = userFieldId,
                    ModifiedBy = modifiedBy
                });

            return rows > 0;
        }
        public async Task<IEnumerable<UserFieldWithOptions>>
    GetFieldsWithOptionsAsync(int userId)
        {
            var sql = @"

    SELECT

        uf.UserFieldId,
        uf.UserId,
        uf.FieldName,
        uf.FieldCode,
        uf.Placeholder,
        uf.IsRequired,
        uf.IsActive,
        uf.DataTypeId,

        udo.UserDropDownId,
        udo.OptionValue,
        udo.OptionLabel,
        udo.IsActive AS OptionIsActive

    FROM userfields uf

    LEFT JOIN userdropdownoptions udo
        ON uf.UserFieldId = udo.UserFieldId

    WHERE uf.UserId = @UserId
    AND uf.IsActive = 1

    ORDER BY uf.UserFieldId DESC;
    ";

            using var connection = _db.GetConnection();

            var data = await connection.QueryAsync<
                UserFieldWithOptions,
                UserDropdownOptionItemRead,
                UserFieldWithOptions>(
                sql,
                (field, option) =>
                {
                    field.Options ??= new List<UserDropdownOptionItemRead>();

                    if (option != null)
                    {
                        field.Options.Add(option);
                    }

                    return field;
                },
                new { UserId = userId },
                splitOn: "UserDropDownId"
            );

            var result = data
                .GroupBy(x => x.UserFieldId)
                .Select(g =>
                {
                    var first = g.First();

                    first.Options = g
                        .SelectMany(x => x.Options)
                        .Where(x => x != null)
                        .ToList();

                    return first;
                });

            return result;
        }
    }
}