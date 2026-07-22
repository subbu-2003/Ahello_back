using ahello_backend.DbContexts;
using ahello_backend.Models.Meeting;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class InstantMeetingRepository : IInstantMeetingRepository
    {
        private readonly DbContext _db;

        public InstantMeetingRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(InstantMeeting model)
        {
            var query = @"
                INSERT INTO InstantMeetings
                (
                    RoomId,
                    RoomName,
                    HostKey,
                    CreatedAt
                )
                VALUES
                (
                    @RoomId,
                    @RoomName,
                    @HostKey,
                    DATE_ADD(UTC_TIMESTAMP(), INTERVAL 330 MINUTE)
                );

                SELECT LAST_INSERT_ID();";

            using var connection = _db.GetConnection();

            return await connection.ExecuteScalarAsync<int>(
                query,
                model);
        }

        public async Task<InstantMeeting?> GetByRoomNameAsync(string roomName)
        {
            var query = @"
                SELECT *
                FROM InstantMeetings
                WHERE RoomName = @RoomName
                LIMIT 1;";

            using var connection = _db.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<InstantMeeting>(
                query,
                new
                {
                    RoomName = roomName
                });
        }
        public async Task<int> CreateJoinRequestAsync(InstantMeetingJoinRequest model)
        {
            var query = @"
        INSERT INTO InstantMeetingJoinRequests
        (
            InstantMeetingId,
            UserName,
            Status,
            CreatedAt
        )
        VALUES
        (
            @InstantMeetingId,
            @UserName,
            @Status,
            DATE_ADD(UTC_TIMESTAMP(), INTERVAL 330 MINUTE)
        );

        SELECT LAST_INSERT_ID();";

            using var connection = _db.GetConnection();

            return await connection.ExecuteScalarAsync<int>(query, model);
        }
        public async Task<IEnumerable<InstantMeetingJoinRequest>> GetWaitingUsersAsync(
    int instantMeetingId)
        {
            var query = @"
        SELECT *
        FROM InstantMeetingJoinRequests
        WHERE InstantMeetingId = @InstantMeetingId
        AND Status = 'Waiting'
        ORDER BY CreatedAt;";

            using var connection = _db.GetConnection();

            return await connection.QueryAsync<InstantMeetingJoinRequest>(
                query,
                new
                {
                    InstantMeetingId = instantMeetingId
                });
        }
        public async Task<InstantMeetingJoinRequest?> GetJoinRequestByIdAsync(
    int requestId)
        {
            var query = @"
        SELECT *
        FROM InstantMeetingJoinRequests
        WHERE Id = @RequestId;";

            using var connection = _db.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<InstantMeetingJoinRequest>(
                query,
                new
                {
                    RequestId = requestId
                });
        }
        public async Task<int> UpdateJoinRequestStatusAsync(
    int requestId,
    string status)
        {
            var query = @"
        UPDATE InstantMeetingJoinRequests
        SET
            Status = @Status,
            UpdatedAt = DATE_ADD(UTC_TIMESTAMP(), INTERVAL 330 MINUTE)
        WHERE Id = @RequestId;";

            using var connection = _db.GetConnection();

            return await connection.ExecuteAsync(
                query,
                new
                {
                    RequestId = requestId,
                    Status = status
                });
        }
        public async Task<int> UpdateAllJoinRequestStatusAsync(
    int instantMeetingId,
    string status)
        {
            var query = @"
        UPDATE InstantMeetingJoinRequests
        SET
            Status = @Status,
            UpdatedAt = DATE_ADD(UTC_TIMESTAMP(), INTERVAL 330 MINUTE)
        WHERE InstantMeetingId = @InstantMeetingId
        AND Status = 'Waiting';";

            using var connection = _db.GetConnection();

            return await connection.ExecuteAsync(
                query,
                new
                {
                    InstantMeetingId = instantMeetingId,
                    Status = status
                });
        }
    }
}