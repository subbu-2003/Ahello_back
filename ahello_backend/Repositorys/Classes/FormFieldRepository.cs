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

            using var connection = _db.GetConnection();

            return await connection.ExecuteScalarAsync<int>(
                query,
                model
            );
        }

        public async Task<bool> UpdateAsync(UpdateFormField model)
        {
            var query = @"
                UPDATE formfields
                SET
                    FormId = @FormId,
                    FieldName = @FieldName,
                    FieldCode = @FieldCode,
                    Placeholder = @Placeholder,
                    Description = @Description,
                    IsRequired = @IsRequired,
                    IsActive = @IsActive,
                    DataTypeId = @DataTypeId,
                    ModifiedBy = @ModifiedBy,
                    ModifiedAt = NOW()
                WHERE FormFieldId = @FormFieldId
            ";

            using var connection = _db.GetConnection();

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
