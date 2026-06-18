using ahello_backend.DbContexts;
using ahello_backend.Models.Service;
using ahello_backend.Repositorys.Interfaces;
using Dapper;
using static ahello_backend.DbContexts.DbContext;

namespace ahello_backend.Repositorys.Classes
{
    public class ServiceCategoryFieldsRepository : IServiceCategoryFieldsRepository
    {
        private readonly DbContext _db;

        public ServiceCategoryFieldsRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<int> AddAsync(ServiceCategoryFields model)
        {
            using var connection = _db.GetConnection();
            // CHECK DUPLICATE FIELDNAME
            string duplicateQuery = @"
            SELECT COUNT(*)
            FROM ServiceCategoryFields
            WHERE  LOWER(FieldName) = LOWER(@FieldName)
            AND IsActive = 1
        ";

            var exists = await connection.ExecuteScalarAsync<int>(
                duplicateQuery,
                new
                {
                    model.FieldName
                });

            if (exists > 0)
            {
                throw new Exception("FieldName already exists for this service category.");
            }
            string query = @"
                INSERT INTO ServiceCategoryFields
                (
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
                    @FieldName,
                    @FieldCode,
                    @Placeholder,
                    @IsRequired,
                    @IsActive,
                    @DataTypeId,
                    @CreatedBy
                );

                SELECT LAST_INSERT_ID();
            ";

            return await connection.ExecuteScalarAsync<int>(query, model);
        }

        public async Task<int> UpdateAsync(ServiceCategoryFields model)
        {
            using var connection = _db.GetConnection();
            // CHECK DUPLICATE FIELDNAME
            string duplicateQuery = @"
        SELECT COUNT(*)
        FROM ServiceCategoryFields
        WHERE  LOWER(FieldName) = LOWER(@FieldName)
        AND ServiceCategoryFieldId != @ServiceCategoryFieldId
        AND IsActive = 1
    ";

            var exists = await connection.ExecuteScalarAsync<int>(
                duplicateQuery,
                new
                {
                    model.FieldName,
                    model.ServiceCategoryFieldId
                });

            if (exists > 0)
            {
                throw new Exception("FieldName already exists for this service category.");
            }
            string query = @"
                UPDATE ServiceCategoryFields
                SET
                    FieldName = @FieldName,
                    FieldCode = @FieldCode,
                    Placeholder = @Placeholder,
                    IsRequired = @IsRequired,
                    IsActive = @IsActive,
                    DataTypeId = @DataTypeId,
                    ModifiedBy = @ModifiedBy,
                    ModifiedAt = NOW()
                WHERE ServiceCategoryFieldId = @ServiceCategoryFieldId
            ";

            return await connection.ExecuteAsync(query, model);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var connection = _db.GetConnection();

            string query = @"
                DELETE FROM ServiceCategoryFields
                WHERE ServiceCategoryFieldId = @Id
            ";

            return await connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<ServiceCategoryFields?> GetByIdAsync(int id)
        {
            using var connection = _db.GetConnection();

            string query = @"
                SELECT *
                FROM ServiceCategoryFields
                WHERE ServiceCategoryFieldId = @Id
            ";

            return await connection.QueryFirstOrDefaultAsync<ServiceCategoryFields>(
                query,
                new { Id = id });
        }

        public async Task<IEnumerable<ServiceCategoryFields>> GetAllAsync()
        {
            using var connection = _db.GetConnection();

            string query = @"
                SELECT *
                FROM ServiceCategoryFields
                ORDER BY ServiceCategoryFieldId DESC
            ";

            return await connection.QueryAsync<ServiceCategoryFields>(query);
        }
    }
}
