using ahello_backend.DbContexts;
using ahello_backend.Models.Category;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class CategoryFieldRepository : ICategoryFieldRepository
    {
        private readonly DbContext _db;

        public CategoryFieldRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(CreateCategoryField model)
        {
            using var connection = _db.GetConnection();

            var duplicateSql = @"
        SELECT COUNT(1)
        FROM CategoryFields
        WHERE CategoryId = @CategoryId
        AND IsActive = 1
        AND LOWER(TRIM(FieldName)) = LOWER(TRIM(@FieldName))";

            var isExists = await connection.ExecuteScalarAsync<int>(
                duplicateSql,
                new
                {
                    model.CategoryId,
                    model.FieldName
                });

            if (isExists > 0)
            {
                throw new Exception("Field Name already exists");
            }

            var sql = @"INSERT INTO CategoryFields
                (
                    CategoryId,
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
                    @CategoryId,
                    @FieldName,
                    @FieldCode,
                    @Placeholder,
                    @IsRequired,
                    1,
                    @DataTypeId,
                    @CreatedBy
                );

                SELECT LAST_INSERT_ID();";

            return await connection.ExecuteScalarAsync<int>(sql, model);
        }

        public async Task<bool> UpdateAsync(UpdateCategoryField model)
        {
            using var connection = _db.GetConnection();

            var duplicateSql = @"
        SELECT COUNT(1)
        FROM CategoryFields
        WHERE CategoryId = @CategoryId
        AND IsActive = 1
        AND CategoryFieldId <> @CategoryFieldId
        AND LOWER(TRIM(FieldName)) = LOWER(TRIM(@FieldName))";

            var isExists = await connection.ExecuteScalarAsync<int>(
                duplicateSql,
                new
                {
                    model.CategoryFieldId,
                    model.CategoryId,
                    model.FieldName
                });

            if (isExists > 0)
            {
                throw new Exception("Field Name already exists");
            }

            var sql = @"UPDATE CategoryFields
                SET
                    CategoryId = @CategoryId,
                    FieldName = @FieldName,
                    FieldCode = @FieldCode,
                    Placeholder = @Placeholder,
                    IsRequired = @IsRequired,
                    IsActive = @IsActive,
                    DataTypeId = @DataTypeId,
                    ModifiedBy = @ModifiedBy
                WHERE CategoryFieldId = @CategoryFieldId";

            var rows = await connection.ExecuteAsync(sql, model);

            return rows > 0;
        }

        public async Task<IEnumerable<CategoryField>> GetByCategoryAsync(int categoryId)
        {
            var sql = @"SELECT *
                        FROM CategoryFields
                        WHERE CategoryId = @CategoryId
                        AND IsActive = 1
                        ORDER BY CategoryFieldId DESC";

            using var connection = _db.GetConnection();

            return await connection.QueryAsync<CategoryField>(
                sql,
                new { CategoryId = categoryId }
            );
        }

        public async Task<CategoryField> GetByIdAsync(int categoryFieldId)
        {
            var sql = @"SELECT *
                        FROM CategoryFields
                        WHERE CategoryFieldId = @CategoryFieldId";

            using var connection = _db.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<CategoryField>(
                sql,
                new { CategoryFieldId = categoryFieldId }
            );
        }

        public async Task<bool> DeleteAsync(int categoryFieldId, string modifiedBy)
        {
            var sql = @"UPDATE CategoryFields
                        SET
                            IsActive = 0,
                            ModifiedBy = @ModifiedBy
                        WHERE CategoryFieldId = @CategoryFieldId";

            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(sql,
                new
                {
                    CategoryFieldId = categoryFieldId,
                    ModifiedBy = modifiedBy
                });

            return rows > 0;
        }
    }
}
