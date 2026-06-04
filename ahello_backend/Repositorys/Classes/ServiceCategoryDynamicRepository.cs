using ahello_backend.DbContexts;
using ahello_backend.Models.Service;
using ahello_backend.Repositorys.Interfaces;
using Dapper;
using static ahello_backend.DbContexts.DbContext;

namespace ahello_backend.Repositorys.Classes
{
    public class ServiceCategoryDynamicRepository : IServiceCategoryDynamicRepository
    {
        private readonly DbContext _db;

        public ServiceCategoryDynamicRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<int> AddAsync(ServiceCategoryDynamic model)
        {
            using var connection = _db.GetConnection();

            string query = @"
                INSERT INTO ServiceCategoryDynamic
                (
                    ServiceCategoryName,
                    CreatedBy,
                    CreatedAt
                )
                VALUES
                (
                    @ServiceCategoryName,
                    @CreatedBy,
                    NOW()
                );

                SELECT LAST_INSERT_ID();
            ";

            return await connection.ExecuteScalarAsync<int>(query, model);
        }

        public async Task<int> UpdateAsync(ServiceCategoryDynamic model)
        {
            using var connection = _db.GetConnection();

            string query = @"
                UPDATE ServiceCategoryDynamic
                SET
                    ServiceCategoryName = @ServiceCategoryName,
                    ModifiedBy = @ModifiedBy,
                    ModifiedAt = NOW()
                WHERE ServiceCategoryId = @ServiceCategoryId
            ";

            return await connection.ExecuteAsync(query, model);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var connection = _db.GetConnection();

            string query = @"
                DELETE FROM ServiceCategoryDynamic
                WHERE ServiceCategoryId = @Id
            ";

            return await connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<ServiceCategoryDynamic?> GetByIdAsync(int id)
        {
            using var connection = _db.GetConnection();

            string query = @"
                SELECT *
                FROM ServiceCategoryDynamic
                WHERE ServiceCategoryId = @Id
            ";

            return await connection.QueryFirstOrDefaultAsync<ServiceCategoryDynamic>(
                query,
                new { Id = id });
        }

        public async Task<IEnumerable<ServiceCategoryDynamic>> GetAllAsync(int pageNumber, int pageSize)
        {
            using var connection = _db.GetConnection();

            int offset = (pageNumber - 1) * pageSize;

            string query = @"
                SELECT *
                FROM ServiceCategoryDynamic
                ORDER BY ServiceCategoryId DESC
                LIMIT @PageSize OFFSET @Offset
            ";

            return await connection.QueryAsync<ServiceCategoryDynamic>(
                query,
                new
                {
                    PageSize = pageSize,
                    Offset = offset
                });
        }
    }
}
