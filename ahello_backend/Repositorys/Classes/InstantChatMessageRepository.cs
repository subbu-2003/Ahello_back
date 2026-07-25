using ahello_backend.DbContexts;
using ahello_backend.Models.Meeting;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class InstantChatMessageRepository
        : IInstantChatMessageRepository
    {
        private readonly DbContext _db;

        public InstantChatMessageRepository(
            DbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<InstantChatMessage>>GetAllAsync()
        {
            var query = @"
            SELECT *
            FROM instantchatmessages
            WHERE IsDeleted = 0
            ORDER BY InstantMessageId DESC";

            using var connection =
                _db.GetConnection();

            return await connection.QueryAsync<InstantChatMessage>(
                query);
        }

        public async Task<IEnumerable<InstantChatMessage>>GetByRoomIdAsync(string roomId)
        {
            var query = @"
            SELECT *
            FROM instantchatmessages
            WHERE RoomId = @RoomId
            AND IsDeleted = 0
            ORDER BY SentAt ASC";

            using var connection =
                _db.GetConnection();

            return await connection.QueryAsync<InstantChatMessage>(
                query,
                new { RoomId = roomId });
        }

        public async Task<int> CreateAsync(InstantChatMessageCreate model)
        {
            var query = @"
            INSERT INTO instantchatmessages
            (
                RoomId,
                PeerId,
                UserName,
                MessageText,
                AttachmentUrl,
                MessageType,
                FormId,
                IsRead,
                SentAt,
                CreatedAt
            )
            VALUES
            (
                @RoomId,
                @PeerId,
                @UserName,
                @MessageText,
                @AttachmentUrl,
                @MessageType,
                @FormId,
                @IsRead,
                DATE_ADD(UTC_TIMESTAMP(), INTERVAL 330 MINUTE),
                DATE_ADD(UTC_TIMESTAMP(), INTERVAL 330 MINUTE)
            );

            SELECT LAST_INSERT_ID();";

            using var connection =
                _db.GetConnection();

            return await connection.ExecuteScalarAsync<int>(
                query,
                model);
        }

        public async Task<int> UpdateAsync(InstantChatMessageUpdate model)
        {
            var query = @"
            UPDATE instantchatmessages
            SET
                RoomId = @RoomId,
                PeerId = @PeerId,
                UserName = @UserName,
                MessageText = @MessageText,
                AttachmentUrl = @AttachmentUrl,
                MessageType = @MessageType,
                FormId = @FormId,
                IsRead = @IsRead,
                IsDeleted = @IsDeleted,
UpdatedAt = DATE_ADD(UTC_TIMESTAMP(), INTERVAL 330 MINUTE),
                ReadAt = CASE
                    WHEN @IsRead = 1
                    THEN DATE_ADD(UTC_TIMESTAMP(), INTERVAL 330 MINUTE)
                    ELSE ReadAt
                END
            WHERE InstantMessageId = @InstantMessageId";

            using var connection = _db.GetConnection();

            return await connection.ExecuteAsync(query, model);
        }

        public async Task<int> DeleteAsync(int instantMessageId)
        {
            var query = @"
            UPDATE instantchatmessages
            SET IsDeleted = 1
            WHERE InstantMessageId =@InstantMessageId";

            using var connection =
                _db.GetConnection();

            return await connection.ExecuteAsync(
                query,
                new
                {
                    InstantMessageId =
                        instantMessageId
                });
        }

        public async Task<int> GetUnreadCountAsync(string roomId, string peerId)
        {
            var query = @"
            SELECT COUNT(*)
            FROM instantchatmessages
            WHERE RoomId = @RoomId
            AND PeerId <> @PeerId
            AND IsRead = 0
            AND IsDeleted = 0";

            using var connection = _db.GetConnection();

            return await connection.ExecuteScalarAsync<int>(
                query,
                new
                {
                    RoomId = roomId,
                    PeerId = peerId
                });
        }

        public async Task<int> MarkAsReadAsync(string roomId, string peerId)
        {
            var query = @"
            UPDATE instantchatmessages
            SET
                IsRead = 1,
                ReadAt =
                    DATE_ADD(
                        UTC_TIMESTAMP(),
                        INTERVAL 330 MINUTE)
            WHERE RoomId = @RoomId
            AND PeerId <> @PeerId
            AND IsRead = 0
            AND IsDeleted = 0";

            using var connection =
                _db.GetConnection();

            return await connection.ExecuteAsync(
                query,
                new
                {
                    RoomId = roomId,
                    PeerId = peerId
                });
        }
    }
}
