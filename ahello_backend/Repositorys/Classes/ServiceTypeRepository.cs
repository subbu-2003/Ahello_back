using ahello_backend.DbContexts;
using ahello_backend.Models.Servicetype;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class ServiceTypeRepository : IServiceTypeRepository
    {
        private readonly DbContext _db;

        public ServiceTypeRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<ServiceType>> GetAllAsync()
        {
            using var connection = _db.GetConnection();

            var sql = @"
                SELECT
                    ServiceTypeId,
                    ServiceTypeName,
                    CreatedAt,
                    CreatedBy,
                    ModifiedAt,
                    ModifiedBy
                FROM servicetypes
                ORDER BY ServiceTypeId DESC";

            return await connection.QueryAsync<ServiceType>(sql);
        }

        public async Task<ServiceType> GetByIdAsync(int serviceTypeId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                SELECT
                    ServiceTypeId,
                    ServiceTypeName,
                    CreatedAt,
                    CreatedBy,
                    ModifiedAt,
                    ModifiedBy
                FROM servicetypes
                WHERE ServiceTypeId = @ServiceTypeId";

            return await connection.QueryFirstOrDefaultAsync<ServiceType>(
                sql,
                new
                {
                    ServiceTypeId = serviceTypeId
                });
        }

        public async Task<int> CreateAsync(ServiceTypePost model)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                INSERT INTO servicetypes
                (
                    ServiceTypeName,
                    CreatedAt,
                    CreatedBy
                )
                VALUES
                (
                    @ServiceTypeName,
                    NOW(),
                    @CreatedBy
                );

                SELECT LAST_INSERT_ID();";

            return await connection.ExecuteScalarAsync<int>(
                sql,
                model);
        }

        public async Task<bool> UpdateAsync(
            int serviceTypeId,
            ServiceTypePut model)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                UPDATE servicetypes
                SET
                    ServiceTypeName = @ServiceTypeName,
                    ModifiedAt = NOW(),
                    ModifiedBy = @ModifiedBy
                WHERE ServiceTypeId = @ServiceTypeId";

            var rows = await connection.ExecuteAsync(
                sql,
                new
                {
                    ServiceTypeId = serviceTypeId,
                    model.ServiceTypeName,
                    model.ModifiedBy
                });

            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int serviceTypeId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                DELETE FROM servicetypes
                WHERE ServiceTypeId = @ServiceTypeId";

            var rows = await connection.ExecuteAsync(
                sql,
                new
                {
                    ServiceTypeId = serviceTypeId
                });

            return rows > 0;
        }
    }
}
