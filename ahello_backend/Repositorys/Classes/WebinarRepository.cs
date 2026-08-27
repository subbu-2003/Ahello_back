using ahello_backend.DbContexts;
using ahello_backend.Models.Webinar;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class WebinarRepository : IWebinarRepository
    {
        private readonly DbContext _context;

        public WebinarRepository(DbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateAsync(Webinar webinar)
        {
            using var connection = _context.GetConnection();

            const string sql = @"
        INSERT INTO webinars
        (
            UserId,
            Title,
            Description,
            WebinarType,
            ScheduleDate,
            StartTime,
            EndTime,
            DurationMinutes,
            MaxParticipants,
            RegistrationFee,
            Status,
            VideoUrl,
            CreatedAt,
            CreatedBy
        )
        VALUES
        (
            @UserId,
            @Title,
            @Description,
            @WebinarType,
            @ScheduleDate,
            @StartTime,
            @EndTime,
            @DurationMinutes,
            @MaxParticipants,
            @RegistrationFee,
            'Draft',
            @VideoUrl,
            NOW(),
            @CreatedBy
        );

        SELECT LAST_INSERT_ID();
    ";

            return await connection.ExecuteScalarAsync<int>(
                sql,
                webinar);
        }


        public async Task<Webinar?> GetByIdAsync(int webinarId)
        {
            using var connection = _context.GetConnection();

            const string sql = @"
                SELECT *
                FROM webinars
                WHERE WebinarId = @WebinarId;
            ";

            return await connection.QueryFirstOrDefaultAsync<Webinar>(
                sql,
                new { WebinarId = webinarId });
        }


        public async Task<IEnumerable<Webinar>> GetAllAsync()
        {
            using var connection = _context.GetConnection();

            const string sql = @"
                SELECT *
                FROM webinars
                ORDER BY CreatedAt DESC;
            ";

            return await connection.QueryAsync<Webinar>(sql);
        }


        public async Task<bool> UpdateAsync(
            int webinarId,
            UpdateWebinarRequest request)
        {
            using var connection = _context.GetConnection();

            const string sql = @"
                UPDATE webinars
                SET
                    Title = @Title,
                    Description = @Description,
                    ScheduleDate = @ScheduleDate,
                    StartTime = @StartTime,
                    EndTime = @EndTime,
                    DurationMinutes = @DurationMinutes,
                    MaxParticipants = @MaxParticipants,
                    RegistrationFee = @RegistrationFee,
                    VideoUrl = @VideoUrl,
                    ModifiedAt = NOW(),
                    ModifiedBy = @ModifiedBy
                    WHERE WebinarId = @WebinarId
                     AND Status = 'Draft';
            ";

            var affected = await connection.ExecuteAsync(
                sql,
                new
                {
                    WebinarId = webinarId,
                    request.Title,
                    request.Description,
                    request.ScheduleDate,
                    request.StartTime,
                    request.EndTime,
                    request.DurationMinutes,
                    request.MaxParticipants,
                    request.RegistrationFee,
                    request.VideoUrl,
                    request.ModifiedBy
                });

            return affected > 0;
        }


        public async Task<bool> DeleteAsync(int webinarId)
        {
            using var connection = _context.GetConnection();

            const string sql = @"
                UPDATE webinars
                SET
                    Status = 'Cancelled',
                    ModifiedAt = NOW()
                WHERE WebinarId = @WebinarId;
            ";

            var affected = await connection.ExecuteAsync(
                sql,
                new { WebinarId = webinarId });

            return affected > 0;
        }


        //public async Task<bool> ApproveWebinarAsync(
        //    int webinarId,
        //    int adminId)
        //{
        //    using var connection = _context.GetConnection();

        //    const string sql = @"
        //        UPDATE webinars
        //        SET
        //            ApprovalStatus = 'Approved',
        //            Status = 'Approved',
        //            ApprovedBy = @AdminId,
        //            ApprovedAt = NOW(),
        //            RejectionReason = NULL,
        //            ModifiedAt = NOW(),
        //            ModifiedBy = @AdminId
        //        WHERE WebinarId = @WebinarId;
        //    ";

        //    var affected = await connection.ExecuteAsync(
        //        sql,
        //        new
        //        {
        //            WebinarId = webinarId,
        //            AdminId = adminId
        //        });

        //    return affected > 0;
        //}


        //public async Task<bool> RejectWebinarAsync(
        //    int webinarId,
        //    int adminId,
        //    string reason)
        //{
        //    using var connection = _context.GetConnection();

        //    const string sql = @"
        //        UPDATE webinars
        //        SET
        //            ApprovalStatus = 'Rejected',
        //            Status = 'Rejected',
        //            ApprovedBy = NULL,
        //            ApprovedAt = NULL,
        //            RejectionReason = @Reason,
        //            ModifiedAt = NOW(),
        //            ModifiedBy = @AdminId
        //        WHERE WebinarId = @WebinarId;
        //    ";

        //    var affected = await connection.ExecuteAsync(
        //        sql,
        //        new
        //        {
        //            WebinarId = webinarId,
        //            AdminId = adminId,
        //            Reason = reason
        //        });

        //    return affected > 0;
        //}


        //public async Task<bool> UploadVideoAsync(
        //    int webinarId,
        //    string videoUrl)
        //{
        //    using var connection = _context.GetConnection();

        //    const string sql = @"
        //        UPDATE webinars
        //        SET
        //            VideoUrl = @VideoUrl,
        //            VideoApprovalStatus = 'Pending',
        //            ModifiedAt = NOW()
        //        WHERE
        //            WebinarId = @WebinarId
        //            AND WebinarType = 'Recorded';
        //    ";

        //    var affected = await connection.ExecuteAsync(
        //        sql,
        //        new
        //        {
        //            WebinarId = webinarId,
        //            VideoUrl = videoUrl
        //        });

        //    return affected > 0;
        //}


        //public async Task<bool> ApproveVideoAsync(
        //    int webinarId,
        //    int adminId)
        //{
        //    using var connection = _context.GetConnection();

        //    const string sql = @"
        //        UPDATE webinars
        //        SET
        //            VideoApprovalStatus = 'Approved',
        //            VideoApprovedBy = @AdminId,
        //            VideoApprovedAt = NOW(),
        //            VideoRejectionReason = NULL,
        //            ModifiedAt = NOW(),
        //            ModifiedBy = @AdminId
        //        WHERE
        //            WebinarId = @WebinarId
        //            AND WebinarType = 'Recorded'
        //            AND VideoUrl IS NOT NULL;
        //    ";

        //    var affected = await connection.ExecuteAsync(
        //        sql,
        //        new
        //        {
        //            WebinarId = webinarId,
        //            AdminId = adminId
        //        });

        //    return affected > 0;
        //}


        //public async Task<bool> RejectVideoAsync(
        //    int webinarId,
        //    int adminId,
        //    string reason)
        //{
        //    using var connection = _context.GetConnection();

        //    const string sql = @"
        //        UPDATE webinars
        //        SET
        //            VideoApprovalStatus = 'Rejected',
        //            VideoApprovedBy = NULL,
        //            VideoApprovedAt = NULL,
        //            VideoRejectionReason = @Reason,
        //            ModifiedAt = NOW(),
        //            ModifiedBy = @AdminId
        //        WHERE WebinarId = @WebinarId
        //          AND WebinarType = 'Recorded';
        //    ";

        //    var affected = await connection.ExecuteAsync(
        //        sql,
        //        new
        //        {
        //            WebinarId = webinarId,
        //            AdminId = adminId,
        //            Reason = reason
        //        });

        //    return affected > 0;
        //}


        public async Task<bool> PublishAsync(int webinarId)
        {
            using var connection = _context.GetConnection();

            const string sql = @"
                UPDATE webinars
                SET
                    Status = 'Published',
                    ModifiedAt = NOW()
                WHERE
                    WebinarId = @WebinarId
                    AND ApprovalStatus = 'Approved'
                    AND
                    (
                        WebinarType = 'Live'
                        OR
                        (
                            WebinarType = 'Recorded'
                            AND VideoApprovalStatus = 'Approved'
                        )
                    );
            ";

            var affected = await connection.ExecuteAsync(
                sql,
                new { WebinarId = webinarId });

            return affected > 0;
        }


        public async Task<int> RegisterAsync(
            WebinarRegistration registration)
        {
            using var connection = _context.GetConnection();

            const string sql = @"
                INSERT INTO webinar_registrations
                (
                    WebinarId,
                    UserId,
                    RegistrationDate,
                    Status,
                    PaymentStatus,
                    PaymentAmount,
                    CreatedAt,
                    CreatedBy
                )
                VALUES
                (
                    @WebinarId,
                    @UserId,
                    NOW(),
                    @Status,
                    @PaymentStatus,
                    @PaymentAmount,
                    NOW(),
                    @CreatedBy
                );

                SELECT LAST_INSERT_ID();
            ";

            return await connection.ExecuteScalarAsync<int>(
                sql,
                registration);
        }


        public async Task<WebinarRegistration?> GetRegistrationAsync(
            int webinarId,
            int userId)
        {
            using var connection = _context.GetConnection();

            const string sql = @"
                SELECT *
                FROM webinar_registrations
                WHERE WebinarId = @WebinarId
                  AND UserId = @UserId;
            ";

            return await connection.QueryFirstOrDefaultAsync<WebinarRegistration>(
                sql,
                new
                {
                    WebinarId = webinarId,
                    UserId = userId
                });
        }


        public async Task<IEnumerable<WebinarRegistration>>
            GetRegistrationsAsync(int webinarId)
        {
            using var connection = _context.GetConnection();

            const string sql = @"
                SELECT *
                FROM webinar_registrations
                WHERE WebinarId = @WebinarId
                ORDER BY RegistrationDate DESC;
            ";

            return await connection.QueryAsync<WebinarRegistration>(
                sql,
                new { WebinarId = webinarId });
        }
    }
    }
