using ahello_backend.DbContexts;
using ahello_backend.Models.Service;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class ServiceCategoryFieldValuesRepository : IServiceCategoryFieldValuesRepository
    {
        private readonly DbContext _db;

        public ServiceCategoryFieldValuesRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<int> AddAsync(ServiceCategoryFieldValues model)
        {
            using var connection = _db.GetConnection();

            string query = @"
                INSERT INTO ServiceCategoryFieldValues
                (
                    ServiceCategoryId,
                    FieldCode,
                    ServiceCategoryFieldId,
                    FieldValue,
                    CreatedDate,
                    CreatedBy
                )
                VALUES
                (
                    @ServiceCategoryId,
                    @FieldCode,
                    @ServiceCategoryFieldId,
                    @FieldValue,
                    @CreatedDate,
                    @CreatedBy
                );

                SELECT LAST_INSERT_ID();
            ";

            return await connection.ExecuteScalarAsync<int>(query, model);
        }

        public async Task<int> UpdateAsync(ServiceCategoryFieldValues model)
        {
            using var connection = _db.GetConnection();

            string query = @"
                UPDATE ServiceCategoryFieldValues
                SET
                    ServiceCategoryId = @ServiceCategoryId,
                    FieldCode = @FieldCode,
                    ServiceCategoryFieldId = @ServiceCategoryFieldId,
                    FieldValue = @FieldValue,
                    ModifiedBy = @ModifiedBy,
                    ModifiedAt = NOW()
                WHERE ServiceCategoryFieldValueId = @ServiceCategoryFieldValueId
            ";

            return await connection.ExecuteAsync(query, model);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var connection = _db.GetConnection();

            string query = @"
                DELETE FROM ServiceCategoryFieldValues
                WHERE ServiceCategoryFieldValueId = @Id
            ";

            return await connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<ServiceCategoryFieldValues?> GetByIdAsync(int id)
        {
            using var connection = _db.GetConnection();

            string query = @"
                SELECT *
                FROM ServiceCategoryFieldValues
                WHERE ServiceCategoryFieldValueId = @Id
            ";

            return await connection.QueryFirstOrDefaultAsync<ServiceCategoryFieldValues>(
                query,
                new { Id = id });
        }

        public async Task<IEnumerable<ServiceCategoryFieldValues>> GetAllAsync()
        {
            using var connection = _db.GetConnection();

            string query = @"
                SELECT *
                FROM ServiceCategoryFieldValues
                ORDER BY ServiceCategoryFieldValueId DESC
            ";

            return await connection.QueryAsync<ServiceCategoryFieldValues>(query);
        }

    }
}
