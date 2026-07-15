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
    }
}