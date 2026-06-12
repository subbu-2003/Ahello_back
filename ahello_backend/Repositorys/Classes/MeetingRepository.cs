using ahello_backend.DbContexts;
using ahello_backend.Models.Meeting;
using ahello_backend.Models.Pagination;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class MeetingRepository : IMeetingRepository
    {
        private readonly DbContext _db;
        private readonly IEmailRepository _emailRepository;

        public MeetingRepository(DbContext db, IEmailRepository emailRepository)
        {
            _db = db;
            _emailRepository = emailRepository;
        }

        public async Task<IEnumerable<Meeting>> GetAllAsync()
        {
            using var connection = _db.GetConnection();

            var sql = @"
        SELECT
            m.MeetingId,
            m.UserId,
            u.FullName AS UserName,
            u.Email,

            b.ClientId,
            cu.FullName AS ClientName,
            cu.Email AS ClientEmail,

            m.BookingId,
            m.StartTime,
            m.EndTime,
            m.MeetingLink,
            m.Status,
            m.ReminderSent,
            m.LastReminderSent,
            m.CreatedAt,
            m.CreatedBy,
            m.ModifiedAt,
            m.ModifiedBy

        FROM meetings m

        INNER JOIN users u
            ON m.UserId = u.UserId

        INNER JOIN bookings b
            ON m.BookingId = b.BookingId

        INNER JOIN users cu
            ON b.ClientId = cu.UserId

        ORDER BY m.MeetingId DESC";

            return await connection.QueryAsync<Meeting>(sql);
        }

        public async Task<Meeting> GetByIdAsync(int meetingId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                SELECT
                    m.MeetingId,
                    m.UserId,
                    u.FullName AS UserName,
                    u.Email,
                    m.BookingId,
                    m.StartTime,
                    m.EndTime,
                    m.MeetingLink,
                    m.Status,
                    m.ReminderSent,
                    m.LastReminderSent,
                    m.CreatedAt,
                    m.CreatedBy,
                    m.ModifiedAt,
                    m.ModifiedBy
                FROM meetings m
                INNER JOIN users u
                    ON m.UserId = u.UserId
                WHERE m.MeetingId = @MeetingId";

            return await connection.QueryFirstOrDefaultAsync<Meeting>(
                sql,
                new { MeetingId = meetingId });
        }

        public async Task<IEnumerable<Meeting>> GetByBookingIdAsync(int bookingId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                SELECT
                    m.MeetingId,
                    m.UserId,
                    u.FullName AS UserName,
                    u.Email,
                    m.BookingId,
                    m.StartTime,
                    m.EndTime,
                    m.MeetingLink,
                    m.Status,
                    m.ReminderSent,
                    m.LastReminderSent
                FROM meetings m
                INNER JOIN users u
                    ON m.UserId = u.UserId
                WHERE m.BookingId = @BookingId";

            return await connection.QueryAsync<Meeting>(
                sql,
                new { BookingId = bookingId });
        }

        public async Task<PagedResult<Meeting>> GetByUserIdAsync(
        int userId,
        int pageNumber,
        int pageSize, string? status,
        DateTime? createdDate)
        {
            using var connection = _db.GetConnection();

            var countSql = @"
            SELECT COUNT(*)
            FROM meetings m
            INNER JOIN bookings b
            ON m.BookingId = b.BookingId
            WHERE b.UserId = @UserId";

            var totalCount =
                await connection.ExecuteScalarAsync<int>(
                    countSql,
                    new { UserId = userId });

            var sql = @"
                SELECT
                    m.MeetingId,

            b.UserId,
            u.FullName AS UserName,

            b.ClientId,
            cu.FullName AS ClientName,

            m.BookingId,
            s.ServiceId,
            s.ServiceTitle,

            s.ServiceCategoryId,
            sc.ServiceCategoryName,
            m.StartTime,
            m.EndTime,
            m.MeetingLink,
            m.Status

        FROM meetings m

        INNER JOIN bookings b
            ON m.BookingId = b.BookingId

        INNER JOIN users u
            ON b.UserId = u.UserId

        INNER JOIN users cu
            ON b.ClientId = cu.UserId
        INNER JOIN services s
            ON b.ServiceId = s.ServiceId

        LEFT JOIN servicecategorydynamic sc
            ON s.ServiceCategoryId = sc.ServiceCategoryId

        WHERE b.UserId = @UserId
        AND
        (
            @Status IS NULL
            OR @Status = ''
            OR m.Status = @Status
        )

        AND
        (
            @CreatedDate IS NULL
            OR DATE(m.CreatedAt) = DATE(@CreatedDate)
        )

        ORDER BY m.MeetingId DESC

        LIMIT @PageSize OFFSET @Offset";

            var meetings =
                await connection.QueryAsync<Meeting>(
                    sql,
                    new
                    {
                        UserId = userId,
                        Status = status,
                        CreatedDate = createdDate,
                        PageSize = pageSize,
                        Offset = (pageNumber - 1) * pageSize
                    });

            return new PagedResult<Meeting>
            {
                TotalCount = totalCount,
                Details = meetings
            };
        }

        public async Task<int> CreateAsync(MeetingPost model)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                INSERT INTO meetings
                (
                    UserId,
                    BookingId,
                    StartTime,
                    EndTime,
                    MeetingLink,
                    Status,
                    CreatedAt,
                    CreatedBy
                )
                VALUES
                (
                    @UserId,
                    @BookingId,
                    @StartTime,
                    @EndTime,
                    @MeetingLink,
                    @Status,
                    NOW(),
                    @CreatedBy
                );

                SELECT LAST_INSERT_ID();";

            return await connection.ExecuteScalarAsync<int>(
                sql,
                model);
        }

        public async Task<Meeting> GetByRoomNameAsync(string roomName)
        {
            using var connection = _db.GetConnection();

            // Step 1: Fetch the meeting by room name only (no time/status filter here)
            var sql = @"
        SELECT
            m.MeetingId,
            m.UserId,
            m.BookingId,
            m.StartTime,
            m.EndTime,
            m.MeetingLink,
            m.Status,
            u.FullName  AS UserName,
            u.Email,
            cu.FullName AS ClientName,
            cu.Email    AS ClientEmail
        FROM meetings m
        INNER JOIN bookings b  ON m.BookingId  = b.BookingId
        INNER JOIN users u     ON m.UserId     = u.UserId
        INNER JOIN users cu    ON b.ClientId   = cu.UserId
        WHERE m.MeetingLink LIKE @RoomName";

            var meeting = await connection.QueryFirstOrDefaultAsync<Meeting>(
                sql,
                new { RoomName = $"%{roomName}%" });

            if (meeting == null) return null;

            // Step 2: Auto-complete if EndTime has passed
            if (DateTime.Now > meeting.EndTime && meeting.Status != "Completed")
            {
                await connection.ExecuteAsync(@"
            UPDATE meetings
            SET Status = 'Completed', ModifiedAt = NOW()
            WHERE MeetingId = @MeetingId",
                    new { meeting.MeetingId });

                meeting.Status = "Completed"; // reflect locally
            }

            return meeting;
        }


        public async Task<bool> UpdateAsync(MeetingPut model)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                UPDATE meetings
                SET
                    UserId = @UserId,
                    BookingId = @BookingId,
                    StartTime = @StartTime,
                    EndTime = @EndTime,
                    MeetingLink = @MeetingLink,
                    Status = @Status,
                    ModifiedAt = NOW(),
                    ModifiedBy = @ModifiedBy
                WHERE MeetingId = @MeetingId";

            var rows = await connection.ExecuteAsync(sql, model);

            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int meetingId)
        {
            using var connection = _db.GetConnection();

            var sql = @"DELETE FROM meetings
                        WHERE MeetingId = @MeetingId";

            var rows = await connection.ExecuteAsync(
                sql,
                new { MeetingId = meetingId });

            return rows > 0;
        }
        // REMOVE SendMeetingReminderEmailAsync entirely from this file

        public async Task<bool> SendMeetingReminderMailAsync(Meeting meeting, int minutesLeft)
        {
            if (meeting == null || string.IsNullOrWhiteSpace(meeting.ClientEmail))
                return false;

            await _emailRepository.SendMeetingReminderEmailAsync(
                meeting.ClientEmail,
                meeting.ClientName,
                meeting.StartTime,
                meeting.MeetingLink,
                minutesLeft); // ✅ pass it

            return true;
        }

        public async Task UpdateReminderSentAsync(int meetingId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
        UPDATE meetings
        SET
            ReminderSent = 1,
            LastReminderSent = NOW()
        WHERE MeetingId = @meetingId";

            await connection.ExecuteAsync(sql, new { meetingId });
        }
        public async Task<IEnumerable<Meeting>> GetPendingRemindersAsync()
        {
            using var connection = _db.GetConnection();

            var sql = @"
        SELECT
            m.MeetingId,
            m.UserId,
            m.BookingId,
            m.StartTime,
            m.EndTime,
            m.MeetingLink,
            m.Status,
            m.ReminderSent,
            u.FullName  AS UserName,
            u.Email,
            cu.FullName AS ClientName,
            cu.Email    AS ClientEmail
        FROM meetings m
        INNER JOIN bookings b  ON m.BookingId = b.BookingId
        INNER JOIN users u     ON m.UserId    = u.UserId
        INNER JOIN users cu    ON b.ClientId  = cu.UserId
        WHERE m.ReminderSent = 0
          AND m.Status = 'Pending'
          AND m.StartTime BETWEEN DATE_ADD(NOW(), INTERVAL 1 MINUTE)
                              AND DATE_ADD(NOW(), INTERVAL 15 MINUTE)";

            return await connection.QueryAsync<Meeting>(sql);
        }
    }
}