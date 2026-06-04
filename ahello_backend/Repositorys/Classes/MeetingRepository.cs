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
        int pageSize)
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

        WHERE b.UserId = @UserId

        ORDER BY m.MeetingId DESC

        LIMIT @PageSize OFFSET @Offset";

            var meetings =
                await connection.QueryAsync<Meeting>(
                    sql,
                    new
                    {
                        UserId = userId,
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

            var sql = @"
        SELECT * FROM meetings
        WHERE MeetingLink LIKE @RoomName";

            return await connection.QueryFirstOrDefaultAsync<Meeting>(
                sql,
                new { RoomName = $"%{roomName}%" });
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
        public async Task<bool> SendMeetingReminderMailAsync(Meeting meeting)
        {
            if (meeting == null ||
                string.IsNullOrWhiteSpace(meeting.ClientEmail))
                return false;

            var subject = "Meeting Reminder - Ahello";

            var body = $@"
<html>

<body style='margin:0;
             padding:0;
             background:#f4f6f9;
             font-family:Arial,sans-serif;'>

    <div style='max-width:600px;
                margin:40px auto;
                background:#ffffff;
                border-radius:12px;
                overflow:hidden;
                box-shadow:0 4px 10px rgba(0,0,0,0.1);'>

        <!-- Header -->
        <div style='background:#2d6cdf;
                    color:white;
                    padding:25px;
                    text-align:center;'>

            <h1 style='margin:0;
                       font-size:26px;'>
                Meeting Reminder
            </h1>

        </div>

        <!-- Body -->
        <div style='padding:30px;'>

            <h2 style='color:#333;'>
                Hello {meeting.ClientName},
            </h2>

            <p style='font-size:16px;
                      color:#555;
                      line-height:1.6;'>

                This is a reminder that your meeting
                will start within
                <strong style='color:#2d6cdf;'>
                    10 minutes
                </strong>.
            </p>

            <!-- Meeting Details -->
            <div style='background:#f8f9fc;
                        border-left:5px solid #2d6cdf;
                        padding:20px;
                        margin-top:25px;
                        border-radius:8px;'>

                <p style='margin:10px 0;
                          font-size:15px;'>
                    <strong>Meeting Time:</strong>
                    {meeting.StartTime:dd MMM yyyy hh:mm tt}
                </p>

                <p style='margin:10px 0;
                          font-size:15px;'>
                    <strong>Meeting Link:</strong>
                </p>

                <a href='{meeting.MeetingLink}'
                   style='display:inline-block;
                          margin-top:10px;
                          background:#2d6cdf;
                          color:white;
                          padding:12px 20px;
                          text-decoration:none;
                          border-radius:6px;
                          font-weight:bold;'>

                    Join Meeting

                </a>

            </div>

            <p style='margin-top:30px;
                      font-size:14px;
                      color:#777;'>

                Please join the meeting on time.

            </p>

            <br/>

            <p style='color:#333;'>
                Regards,
            </p>

            <strong style='color:#2d6cdf;'>
                Ahello Team
            </strong>

             </div>

            <!-- Footer -->
            <div style='background:#f1f1f1;
                    text-align:center;
                    padding:15px;
                    font-size:13px;
                    color:#888;'>

            © 2026 Ahello. All Rights Reserved.

             </div>

             </div>

            </body>

            </html>";

            await _emailRepository.SendEmailAsync(
                meeting.ClientEmail,
                subject,
                body);

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

            await connection.ExecuteAsync(sql,
                new { meetingId });
        }
    }
}