using ahello_backend.DbContexts;
using ahello_backend.Models.Meeting;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class MeetingChatMessageRepository
        : IMeetingChatMessageRepository
    {
        private readonly DbContext _db;

        public MeetingChatMessageRepository(
            DbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<MeetingChatMessage>>
            GetAllAsync()
        {
            var query = @"
                SELECT *
                FROM meetingchatmessages
                WHERE IsDeleted = 0
                ORDER BY ChatMessageId DESC";

            using var connection =
                _db.GetConnection();

            return await connection
                .QueryAsync<MeetingChatMessage>(
                    query);
        }

        public async Task<IEnumerable<MeetingChatMessageResponse>>
    GetByMeetingIdAsync(int meetingId)
        {
            var query = @"
SELECT
    cm.ChatMessageId,
    cm.MeetingId,
    cm.UserId,
    u.FullName AS UserName,
    u.ProfileUrl,
    cm.MessageText,
    cm.AttachmentUrl,
    cm.MessageType,
    cm.FormId,
    cm.IsRead,
    cm.SentAt
FROM meetingchatmessages cm
INNER JOIN users u
    ON u.UserId = cm.UserId
WHERE cm.MeetingId = @MeetingId
AND cm.IsDeleted = 0
ORDER BY cm.SentAt ASC;";

            using var connection = _db.GetConnection();

            return await connection.QueryAsync<MeetingChatMessageResponse>(
                query,
                new
                {
                    MeetingId = meetingId
                });
        }

        public async Task<IEnumerable<MeetingChatMessage>>
            GetByUserIdAsync(int userId)
        {
            var query = @"
                SELECT *
                FROM meetingchatmessages
                WHERE UserId = @UserId
                AND IsDeleted = 0
                ORDER BY ChatMessageId DESC";

            using var connection =
                _db.GetConnection();

            return await connection
                .QueryAsync<MeetingChatMessage>(
                    query,
                    new { UserId = userId });
        }

        public async Task<int> CreateAsync(
    MeetingChatMessageCreate model)
        {
            var query = @"
INSERT INTO meetingchatmessages
(
    MeetingId,
    UserId,
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
    @MeetingId,
    @UserId,
    @MessageText,
    @AttachmentUrl,
    @MessageType,
    @FormId,
    @IsRead,
    DATE_ADD(UTC_TIMESTAMP(), INTERVAL 330 MINUTE),
    DATE_ADD(UTC_TIMESTAMP(), INTERVAL 330 MINUTE)
);

SELECT LAST_INSERT_ID();";

            using var connection = _db.GetConnection();

            return await connection.ExecuteScalarAsync<int>(
                query,
                model);
        }

        public async Task<int> UpdateAsync(
            MeetingChatMessageUpdate model)
        {
            var query = @"
                UPDATE meetingchatmessages
                SET
    MeetingId = @MeetingId,
    UserId = @UserId,
    MessageText = @MessageText,
    AttachmentUrl = @AttachmentUrl,
    MessageType = @MessageType,
    FormId = @FormId,
    IsRead = @IsRead,
                    IsDeleted = @IsDeleted,
                    ReadAt = CASE
                        WHEN @IsRead = 1
                        THEN DATE_ADD(UTC_TIMESTAMP(), INTERVAL 330 MINUTE)
                        ELSE NULL
                    END
                WHERE ChatMessageId =
                    @ChatMessageId";

            using var connection =
                _db.GetConnection();

            return await connection
                .ExecuteAsync(query, model);
        }

        public async Task<int> DeleteAsync(
            int chatMessageId)
        {
            var query = @"
                UPDATE meetingchatmessages
                SET IsDeleted = 1
                WHERE ChatMessageId =
                    @ChatMessageId";

            using var connection =
                _db.GetConnection();

            return await connection
                .ExecuteAsync(
                    query,
                    new
                    {
                        ChatMessageId =
                            chatMessageId
                    });
        }
        public async Task<int> GetUnreadCountAsync(int meetingId, int userId)
        {
            var query = @"
SELECT COUNT(*)
FROM meetingchatmessages
WHERE MeetingId = @MeetingId
AND UserId <> @UserId
AND IsRead = 0
AND IsDeleted = 0;";

            using var connection = _db.GetConnection();

            return await connection.ExecuteScalarAsync<int>(
                query,
                new
                {
                    MeetingId = meetingId,
                    UserId = userId
                });
        }
        public async Task<int> MarkAsReadAsync(int meetingId, int userId)
        {
            var query = @"
UPDATE meetingchatmessages
SET
    IsRead = 1,
    ReadAt = DATE_ADD(UTC_TIMESTAMP(), INTERVAL 330 MINUTE)
WHERE MeetingId = @MeetingId
AND UserId <> @UserId
AND IsRead = 0
AND IsDeleted = 0;";

            using var connection = _db.GetConnection();

            return await connection.ExecuteAsync(
                query,
                new
                {
                    MeetingId = meetingId,
                    UserId = userId
                });
        }
    }
}