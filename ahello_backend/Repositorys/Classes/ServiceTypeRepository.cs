using ahello_backend.DbContexts;
using ahello_backend.Models.Pagination;
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
                WHERE IsActive = 1
                ORDER BY ServiceTypeId DESC";

            return await connection.QueryAsync<ServiceType>(sql);
        }
        public async Task<PagedResult<ServiceType>> GetPagedAsync(
    int pageNumber,
    int pageSize,
    string? search,
    DateTime? createdDate,
    bool? isActive = null)
        {
            using var connection = _db.GetConnection();

            var whereClause = " WHERE 1=1 ";

            if (!string.IsNullOrEmpty(search))
            {
                whereClause += " AND ServiceTypeName LIKE @Search ";
            }

            if (createdDate.HasValue)
            {
                whereClause += " AND DATE(CreatedAt) = @CreatedDate ";
            }

            if (isActive.HasValue)
            {
                whereClause += " AND IsActive = @IsActive ";
            }

            var parameters = new
            {
                Search = $"%{search}%",
                CreatedDate = createdDate?.Date,
                IsActive = isActive,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var totalQuery = $@"
        SELECT COUNT(*)
        FROM servicetypes
        {whereClause}";

            var totalRecords = await connection.ExecuteScalarAsync<int>(
                totalQuery,
                parameters);

            var sql = $@"
        SELECT
            ServiceTypeId,
            ServiceTypeName,
            IsActive,
            CreatedAt,
            CreatedBy,
            ModifiedAt,
            ModifiedBy
        FROM servicetypes
        {whereClause}
        ORDER BY ServiceTypeId DESC
        LIMIT @Offset,@PageSize";

            var data = await connection.QueryAsync<ServiceType>(
                sql,
                parameters);

            return new PagedResult<ServiceType>
            {
                Details = data.ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalRecords
            };
        }
        public async Task<ServiceType> GetByIdAsync(int serviceTypeId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                SELECT
                    ServiceTypeId,
                    ServiceTypeName,
                    IsActive,
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
            // CHECK DUPLICATE SERVICETYPENAME
            var duplicateSql = @"
        SELECT COUNT(*)
        FROM servicetypes
        WHERE LOWER(ServiceTypeName) = LOWER(@ServiceTypeName)";

            var exists = await connection.ExecuteScalarAsync<int>(
                duplicateSql,
                new
                {
                    model.ServiceTypeName
                });

            if (exists > 0)
            {
                throw new Exception("ServiceTypeName already exists.");
            }
            var sql = @"
                INSERT INTO servicetypes
                (
                    ServiceTypeName, IsActive,
                    CreatedAt,
                    CreatedBy
                )
                VALUES
                (
                    @ServiceTypeName, @IsActive,
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
            // CHECK DUPLICATE SERVICETYPENAME
            var duplicateSql = @"
            SELECT COUNT(*)
            FROM servicetypes
            WHERE LOWER(ServiceTypeName) = LOWER(@ServiceTypeName)
            AND ServiceTypeId != @ServiceTypeId";

            var exists = await connection.ExecuteScalarAsync<int>(
                duplicateSql,
                new
                {
                    model.ServiceTypeName,
                    ServiceTypeId = serviceTypeId
                });

            if (exists > 0)
            {
                throw new Exception("ServiceTypeName already exists.");
            }
            var sql = @"
                UPDATE servicetypes
                SET
                    ServiceTypeName = @ServiceTypeName,
                    IsActive = @IsActive,
                    ModifiedAt = NOW(),
                    ModifiedBy = @ModifiedBy
                WHERE ServiceTypeId = @ServiceTypeId";

            var rows = await connection.ExecuteAsync(
                sql,
                new
                {
                    ServiceTypeId = serviceTypeId,
                    model.ServiceTypeName,
                    model.IsActive,
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
        public async Task<bool> UpdateStatusAsync(int serviceTypeId,ServiceTypeStatusUpdate model)
        {
            using var connection = _db.GetConnection();

            var sql = @"
        UPDATE servicetypes
        SET
            IsActive = @IsActive,
            ModifiedAt = NOW(),
            ModifiedBy = @ModifiedBy
        WHERE ServiceTypeId = @ServiceTypeId";

            var rows = await connection.ExecuteAsync(
                sql,
                new
                {
                    ServiceTypeId = serviceTypeId,
                    model.IsActive,
                    model.ModifiedBy
                });

            return rows > 0;
        }
    }
}
