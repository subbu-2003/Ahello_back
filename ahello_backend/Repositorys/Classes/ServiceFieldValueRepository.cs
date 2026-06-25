using ahello_backend.DbContexts;
using ahello_backend.Models.Service;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class ServiceFieldValueRepository : IServiceFieldValueRepository
    {
        private readonly DbContext _db;

        public ServiceFieldValueRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(CreateServiceFieldValue model)
        {
            var sql = @"INSERT INTO servicefieldvalues
                        (
                            UserId,
                            FieldCode,
                            ServiceFieldId,
                            FieldValue,
                            CreatedDate,
                            CreatedBy,
                            CreatedAt
                        )
                        VALUES
                        (
                            @UserId,
                            @FieldCode,
                            @ServiceFieldId,
                            @FieldValue,
                            NOW(),
                            @CreatedBy,
                            CURRENT_TIMESTAMP
                        );

                        SELECT LAST_INSERT_ID();";

            using var connection = _db.GetConnection();

            return await connection.ExecuteScalarAsync<int>(sql, model);
        }

        public async Task<bool> UpdateAsync(UpdateServiceFieldValue model)
        {
            var sql = @"UPDATE servicefieldvalues
                        SET
                            FieldValue = @FieldValue,
                            ModifiedBy = @ModifiedBy,
                            ModifiedAt = CURRENT_TIMESTAMP
                        WHERE ServiceFieldValueId = @ServiceFieldValueId";

            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(sql, model);

            return rows > 0;
        }

        public async Task<IEnumerable<ServiceFieldValue>> GetByServiceAsync(int serviceId)
        {
            var sql = @"SELECT *
                        FROM servicefieldvalues
                        WHERE ServiceId = @ServiceId
                        ORDER BY ServiceFieldValueId DESC";

            using var connection = _db.GetConnection();

            return await connection.QueryAsync<ServiceFieldValue>(
                sql,
                new { ServiceId = serviceId }
            );
        }

        public async Task<ServiceFieldValue> GetByIdAsync(int serviceFieldValueId)
        {
            var sql = @"SELECT *
                        FROM servicefieldvalues
                        WHERE ServiceFieldValueId = @ServiceFieldValueId";

            using var connection = _db.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<ServiceFieldValue>(
                sql,
                new { ServiceFieldValueId = serviceFieldValueId }
            );
        }
    }
}