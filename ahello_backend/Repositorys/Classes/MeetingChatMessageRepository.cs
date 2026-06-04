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

        public async Task<IEnumerable<MeetingChatMessage>>
            GetByMeetingIdAsync(int meetingId)
        {
            var query = @"
                SELECT *
                FROM meetingchatmessages
                WHERE MeetingId = @MeetingId
                AND IsDeleted = 0
                ORDER BY SentAt ASC";

            using var connection =
                _db.GetConnection();

            return await connection
                .QueryAsync<MeetingChatMessage>(
                    query,
                    new { MeetingId = meetingId });
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
                    @IsRead,
                    NOW(),
                    NOW()
                )";

            using var connection =
                _db.GetConnection();

            return await connection
                .ExecuteAsync(query, model);
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
                    IsRead = @IsRead,
                    IsDeleted = @IsDeleted,
                    ReadAt = CASE
                        WHEN @IsRead = 1
                        THEN NOW()
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
    }
}