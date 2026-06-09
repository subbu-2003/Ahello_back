using ahello_backend.DbContexts;
using ahello_backend.Models.Form;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class FormFieldRepository : IFormFieldRepository
    {
        private readonly DbContext _db;

        public FormFieldRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(CreateFormField model)
        {
            using var connection = _db.GetConnection();

            var duplicateSql = @"
            SELECT COUNT(1)
            FROM formfields
            WHERE FormId = @FormId
            AND IsActive = 1
            AND LOWER(TRIM(FieldName)) = LOWER(TRIM(@FieldName))";

            var isExists = await connection.ExecuteScalarAsync<int>(
                duplicateSql,
                new
                {
                    model.FormId,
                    model.FieldName
                });

            if (isExists > 0)
            {
                throw new Exception("Field Name already exists");
            }

            var query = @"
                INSERT INTO formfields
                (
                    FormId,
                    FieldName,
                    FieldCode,
                    Placeholder,
                    Description,
                    IsRequired,
                    IsActive,
                    DataTypeId,
                    CreatedBy,
                    CreatedAt
                )
                VALUES
                (
                    @FormId,
                    @FieldName,
                    @FieldCode,
                    @Placeholder,
                    @Description,
                    @IsRequired,
                    @IsActive,
                    @DataTypeId,
                    @CreatedBy,
                    NOW()
                );

                SELECT LAST_INSERT_ID();
            ";

            return await connection.ExecuteScalarAsync<int>(
                query,
                model
            );
        }

        public async Task<bool> UpdateAsync(UpdateFormField model)
        {
            using var connection = _db.GetConnection();

            // Duplicate validation only if FieldName provided
            if (!string.IsNullOrWhiteSpace(model.FieldName))
            {
                var duplicateSql = @"
        SELECT COUNT(1)
        FROM formfields
        WHERE FormId = COALESCE(@FormId, FormId)
        AND IsActive = 1
        AND FormFieldId <> @FormFieldId
        AND LOWER(TRIM(FieldName)) = LOWER(TRIM(@FieldName))";

                var isExists = await connection.ExecuteScalarAsync<int>(
                    duplicateSql,
                    model
                );

                if (isExists > 0)
                {
                    throw new Exception("Field Name already exists");
                }
            }

            var query = @"
    UPDATE formfields
    SET
        FormId = COALESCE(@FormId, FormId),

        FieldName = COALESCE(@FieldName, FieldName),

        FieldCode = COALESCE(@FieldCode, FieldCode),

        Placeholder = COALESCE(@Placeholder, Placeholder),

        Description = COALESCE(@Description, Description),

        IsRequired = COALESCE(@IsRequired, IsRequired),

        IsActive = COALESCE(@IsActive, IsActive),

        DataTypeId = COALESCE(@DataTypeId, DataTypeId),

        ModifiedBy = COALESCE(@ModifiedBy, ModifiedBy),

        ModifiedAt = NOW()

        WHERE FormFieldId = @FormFieldId
    ";

            var result = await connection.ExecuteAsync(
                query,
                model
            );

            return result > 0;
        }

        public async Task<IEnumerable<FormField>> GetByFormAsync(int formId)
        {
            var query = @"
                SELECT *
                FROM formfields
                WHERE FormId = @FormId
            ";

            using var connection = _db.GetConnection();

            return await connection.QueryAsync<FormField>(
                query,
                new { FormId = formId }
            );
        }

        public async Task<FormField> GetByIdAsync(int formFieldId)
        {
            var query = @"
                SELECT *
                FROM formfields
                WHERE FormFieldId = @FormFieldId
            ";

            using var connection = _db.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<FormField>(
                query,
                new { FormFieldId = formFieldId }
            );
        }

        public async Task<bool> DeleteAsync(
            int formFieldId,
            string modifiedBy
        )
        {
            var query = @"
                DELETE FROM formfields
                WHERE FormFieldId = @FormFieldId
            ";

            using var connection = _db.GetConnection();

            var result = await connection.ExecuteAsync(
                query,
                new
                {
                    FormFieldId = formFieldId
                }
            );

            return result > 0;
        }
    }
}
