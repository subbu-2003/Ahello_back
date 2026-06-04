using ahello_backend.DbContexts;
using ahello_backend.Models.Category;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class CategoryFieldValueRepository : ICategoryFieldValueRepository
    {
        private readonly DbContext _db;

        public CategoryFieldValueRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(CreateCategoryFieldValue model)
        {
            var sql = @"INSERT INTO CategoryFieldValues
                        (
                            CategoryId,
                            FieldCode,
                            CategoryFieldId,
                            FieldValue,
                            CreatedDate,
                            CreatedBy,
                            CreatedAt
                        )
                        VALUES
                        (
                            @CategoryId,
                            @FieldCode,
                            @CategoryFieldId,
                            @FieldValue,
                            NOW(),
                            @CreatedBy,
                            CURRENT_TIMESTAMP
                        );

                        SELECT LAST_INSERT_ID();";

            using var connection = _db.GetConnection();

            return await connection.ExecuteScalarAsync<int>(sql, model);
        }

        public async Task<bool> UpdateAsync(UpdateCategoryFieldValue model)
        {
            var sql = @"UPDATE CategoryFieldValues
                        SET
                            FieldValue = @FieldValue,
                            ModifiedBy = @ModifiedBy,
                            ModifiedAt = CURRENT_TIMESTAMP
                        WHERE CategoryFieldValueId = @CategoryFieldValueId";

            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(sql, model);

            return rows > 0;
        }

        public async Task<IEnumerable<CategoryFieldValue>> GetByCategoryAsync(int categoryId)
        {
            var sql = @"SELECT *
                        FROM CategoryFieldValues
                        WHERE CategoryId = @CategoryId
                        ORDER BY CategoryFieldValueId DESC";

            using var connection = _db.GetConnection();

            return await connection.QueryAsync<CategoryFieldValue>(
                sql,
                new { CategoryId = categoryId }
            );
        }

        public async Task<CategoryFieldValue> GetByIdAsync(int categoryFieldValueId)
        {
            var sql = @"SELECT *
                        FROM CategoryFieldValues
                        WHERE CategoryFieldValueId = @CategoryFieldValueId";

            using var connection = _db.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<CategoryFieldValue>(
                sql,
                new { CategoryFieldValueId = categoryFieldValueId }
            );
        }
    }
}