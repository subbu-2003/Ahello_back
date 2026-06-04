using ahello_backend.DbContexts;
using ahello_backend.Models.Form;
using ahello_backend.Repositorys.Interfaces;
using Dapper;


namespace ahello_backend.Repositorys.Classes
{
    public class FormDropdownOptionRepository
        : IFormDropdownOptionRepository
    {
        private readonly DbContext _db;

        public FormDropdownOptionRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(
            CreateFormDropdownOption model)
        {
            var sql = @"
            INSERT INTO formdropdownoptions
            (
                FormFieldId,
                FormId,
                OptionValue,
                OptionLabel,
                IsActive,
                CreatedDate,
                CreatedBy
            )
            VALUES
            (
                @FormFieldId,
                @FormId,
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
                        model.FormFieldId,
                        model.FormId,
                        opt.OptionValue,
                        opt.OptionLabel,
                        opt.IsActive,
                        model.CreatedBy
                    });
            }

            return rows;
        }

        public async Task<bool> UpdateAsync(
            UpdateFormDropdownOption model)
        {
            var sql = @"
            UPDATE formdropdownoptions
            SET
                OptionValue = @OptionValue,
                OptionLabel = @OptionLabel,
                IsActive = @IsActive,
                ModifiedDate = NOW(),
                ModifiedBy = @ModifiedBy
            WHERE FormDropDownId = @FormDropDownId";

            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(
                sql,
                model);

            return rows > 0;
        }

        public async Task<GetFormDropdownOption> GetByIdAsync(
            int optionId)
        {
            var sql = @"
            SELECT *
            FROM formdropdownoptions
            WHERE FormDropDownId = @optionId";

            using var connection = _db.GetConnection();

            return await connection
                .QueryFirstOrDefaultAsync<GetFormDropdownOption>(
                    sql,
                    new { optionId });
        }

        public async Task<IEnumerable<GetFormDropdownOption>>
            GetByFieldIdAsync(
                int formFieldId,
                int? formId)
        {
            var sql = @"
            SELECT *
            FROM formdropdownoptions
            WHERE FormFieldId = @formFieldId
            AND (@formId IS NULL OR FormId = @formId)
            AND IsActive = 1";

            using var connection = _db.GetConnection();

            return await connection
                .QueryAsync<GetFormDropdownOption>(
                    sql,
                    new
                    {
                        formFieldId,
                        formId
                    });
        }
    }
}