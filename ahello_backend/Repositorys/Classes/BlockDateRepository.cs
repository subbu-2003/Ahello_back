using ahello_backend.DbContexts;
using ahello_backend.Models.Blockdate;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class BlockDateRepository : IBlockDateRepository
    {
        private readonly DbContext _db;

        public BlockDateRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<BlockDateGet>> GetAll()
        {
            using var connection = _db.GetConnection();

            string query = @"
                SELECT
                    b.*,

                    u.FullName AS UserName,

                    s.ServiceTitle

                FROM blockdates b

                INNER JOIN users u
                    ON b.UserId = u.UserId

                INNER JOIN services s
                    ON b.ServiceId = s.ServiceId";

            return await connection.QueryAsync<BlockDateGet>(query);
        }

        public async Task<BlockDateGet> GetById(int id)
        {
            using var connection = _db.GetConnection();

            string query = @"
                SELECT
                    b.*,

                    u.FullName AS UserName,

                    s.ServiceTitle

                FROM blockdates b

                INNER JOIN users u
                    ON b.UserId = u.UserId

                INNER JOIN services s
                    ON b.ServiceId = s.ServiceId

                WHERE b.BlockDateId = @BlockDateId";

            return await connection.QueryFirstOrDefaultAsync<BlockDateGet>(
                query,
                new { BlockDateId = id });
        }

        public async Task<IEnumerable<BlockDateGet>> GetByUserId(int userId)
        {
            using var connection = _db.GetConnection();

            string query = @"
                SELECT
                    b.*,

                    u.FullName AS UserName,

                    s.ServiceTitle

                FROM blockdates b

                INNER JOIN users u
                    ON b.UserId = u.UserId

                INNER JOIN services s
                    ON b.ServiceId = s.ServiceId

                WHERE b.UserId = @UserId";

            return await connection.QueryAsync<BlockDateGet>(
                query,
                new { UserId = userId });
        }

        public async Task<IEnumerable<BlockDateGet>> GetByServiceId(int serviceId)
        {
            using var connection = _db.GetConnection();

            string query = @"
                SELECT
                    b.*,

                    u.FullName AS UserName,

                    s.ServiceTitle

                FROM blockdates b

                INNER JOIN users u
                    ON b.UserId = u.UserId

                INNER JOIN services s
                    ON b.ServiceId = s.ServiceId

                WHERE b.ServiceId = @ServiceId";

            return await connection.QueryAsync<BlockDateGet>(
                query,
                new { ServiceId = serviceId });
        }

        public async Task<int> Post(BlockDatePost model)
        {
            using var connection = _db.GetConnection();

            string query = @"
                INSERT INTO blockdates
                (
                    UserId,
                    ServiceId,
                    BlockDate,
                    StartTime,
                    EndTime,
                    Reason,
                    CreatedAt,
                    CreatedBy
                )
                VALUES
                (
                    @UserId,
                    @ServiceId,
                    @BlockDate,
                    @StartTime,
                    @EndTime,
                    @Reason,
                    NOW(),
                    @CreatedBy
                )";

            return await connection.ExecuteAsync(query, model);
        }

        public async Task<int> Update(BlockDatePut model)
        {
            using var connection = _db.GetConnection();

            string query = @"
                UPDATE blockdates
                SET
                    UserId = @UserId,
                    ServiceId = @ServiceId,
                    BlockDate = @BlockDate,
                    StartTime = @StartTime,
                    EndTime = @EndTime,
                    Reason = @Reason,
                    ModifiedAt = NOW(),
                    ModifiedBy = @ModifiedBy
                WHERE BlockDateId = @BlockDateId";

            return await connection.ExecuteAsync(query, model);
        }

        public async Task<int> Delete(int id)
        {
            using var connection = _db.GetConnection();

            string query = @"
                DELETE FROM blockdates
                WHERE BlockDateId = @BlockDateId";

            return await connection.ExecuteAsync(
                query,
                new { BlockDateId = id });
        }
    }
}