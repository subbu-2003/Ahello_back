using ahello_backend.DbContexts;
using ahello_backend.Models.Bookings;
using ahello_backend.Models.Meeting;
using ahello_backend.Models.Pagination;
using ahello_backend.Repositorys.Interfaces;
using Dapper;
using System.Globalization;

namespace ahello_backend.Repositorys.Classes
{
    public class BookingRepository : IBookingRepository
    {
        private readonly DbContext _db;
        private readonly DbContextConnection _dbConn;
        private readonly IEmailRepository _emailRepository;
        private readonly IUserSlotRepository _userSlotRepo;
        private readonly IServiceRepository _serviceRepository;

        public BookingRepository(
         DbContext db,
         DbContextConnection dbConn,
         IUserSlotRepository userSlotRepo,
         IServiceRepository serviceRepository,
         IEmailRepository emailRepository)
        {
            _db = db;
            _dbConn = dbConn;
        _userSlotRepo =  userSlotRepo;
        _serviceRepository = serviceRepository;
        _emailRepository = emailRepository;
        }

        public async Task<int> CreateAsync(BookingPost model)
        {
            if (model.UserId == model.ClientId)
                throw new Exception("UserId and ClientId cannot be same.");

            // STEP 1 — PARALLEL READS using TWO separate connections
            using var clientConn = _dbConn.GetMyConnection();
            using var serviceConn = _dbConn.GetMyConnection();

            await Task.WhenAll(clientConn.OpenAsync(), serviceConn.OpenAsync());

            var clientTask = clientConn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT FullName, Email FROM users WHERE UserId = @ClientId",
                new { model.ClientId });

            var serviceTask = serviceConn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT ServiceTitle FROM services WHERE ServiceId = @ServiceId",
                new { model.ServiceId });

            await Task.WhenAll(clientTask, serviceTask);

            var client = clientTask.Result;
            var service = serviceTask.Result;

            // STEP 2 — TRANSACTION WRITES
            using var connection = _dbConn.GetMyConnection();
            await connection.OpenAsync();
            using var tx = await connection.BeginTransactionAsync();

            try
            {
                var bookingSql = @"
            INSERT INTO bookings
            (UserId, ClientId, ServiceId, ScheduleDate, StartTime, EndTime, Status, CreatedAt, CreatedBy)
            VALUES
            (@UserId, @ClientId, @ServiceId, @ScheduleDate, @StartTime, @EndTime, @Status, NOW(), @CreatedBy);
            SELECT LAST_INSERT_ID();";

                var bookingId = await connection.ExecuteScalarAsync<int>(bookingSql, model, tx);

                var meetingLink = GenerateMiroTalkLink(bookingId);
                var meetingStartTime = model.ScheduleDate.Date.Add(model.StartTime);
                var meetingEndTime = model.ScheduleDate.Date.Add(model.EndTime);

                await connection.ExecuteAsync(@"
            INSERT INTO meetings
            (UserId, BookingId, StartTime, EndTime, MeetingLink, Status, CreatedAt, CreatedBy)
            VALUES
            (@UserId, @BookingId, @StartTime, @EndTime, @MeetingLink, 'Pending', NOW(), @CreatedBy);",
                    new
                    {
                        model.UserId,
                        BookingId = bookingId,
                        StartTime = meetingStartTime,
                        EndTime = meetingEndTime,
                        MeetingLink = meetingLink,
                        model.CreatedBy
                    }, tx);

                // INSERT INTO BOOKEDSLOTS
                var bookedSlotSql = @"
            INSERT INTO bookedslots
            (SlotId, UserId, ServiceId, BookingId, SlotDate, StartTime, EndTime, CreatedAt)
            VALUES
            (@SlotId, @UserId, @ServiceId, @BookingId, @SlotDate, @StartTime, @EndTime, NOW())";

                await connection.ExecuteAsync(bookedSlotSql, new
                {
                    model.SlotId,
                    model.UserId,
                    model.ServiceId,
                    BookingId = bookingId,
                    SlotDate = model.ScheduleDate,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime
                }, tx);

                await tx.CommitAsync();

                // ✅ capture values
                string clientEmail = client.Email;
                string clientFullName = client.FullName;
                string serviceName = service?.ServiceTitle ?? "the service";
                string formattedDate = model.ScheduleDate.ToString("dddd, MMMM dd yyyy");
                string formattedTime = meetingStartTime.ToString("hh:mm tt", CultureInfo.InvariantCulture);
                string capturedMeetingLink = $"{meetingLink}?userId={model.ClientId}&email={Uri.EscapeDataString(clientEmail)}";

                // ✅ await directly but with timeout — won't cause double commit
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

                    await _emailRepository.SendBookingConfirmationEmailAsync(
                        clientEmail, clientFullName, serviceName, formattedDate, formattedTime);

                    await _emailRepository.SendMeetingInviteEmailAsync(
                        clientEmail, clientFullName, capturedMeetingLink);

                    Console.WriteLine($"[Email] Both emails sent to {clientEmail}");
                }
                catch (Exception ex)
                {
                    // ✅ email failure never affects booking — already committed
                    Console.WriteLine($"[EmailError] {ex.GetType().Name}: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                }

                return bookingId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // MIROTALK LINK GENERATOR
        private string GenerateMiroTalkLink(
            int bookingId)
        {
            var uniqueId =
                Guid.NewGuid().ToString("N")[..8];

            var roomName =
                $"ahllo-{bookingId}-{uniqueId}";

            return
                $"https://ahllo.com/meeting/join/{roomName}";
        }



        public async Task<int> RescheduleAsync(
    int oldBookingId,
    DateTime newDate,
    TimeSpan newStart,
    TimeSpan newEnd,
    int slotId,
    string rescheduledBy,
    string reason)
        {
            using var connection = _dbConn.GetMyConnection();
            await connection.OpenAsync();
            using var tx = await connection.BeginTransactionAsync();

            try
            {
                // ======================================================
                // 1. Fetch old booking
                // ======================================================
                var old = await connection.QueryFirstOrDefaultAsync<BookingRead>(@"
            SELECT
                b.BookingId,
                b.UserId,
                b.ClientId,
                b.ServiceId,
                b.ScheduleDate,
                b.StartTime,
                b.EndTime,
                b.Status,
                b.CreatedBy,
                cu.FullName AS ClientName,
                cu.Email AS ClientEmail,
                s.ServiceTitle
            FROM bookings b
            INNER JOIN users cu ON b.ClientId = cu.UserId
            INNER JOIN services s ON b.ServiceId = s.ServiceId
            WHERE b.BookingId = @Id",
                    new { Id = oldBookingId },
                    tx);

                if (old == null)
                    throw new Exception("Booking not found.");

                if (old.Status == "Cancelled" || old.Status == "Rejected")
                    throw new Exception("Cancelled or rejected booking cannot be rescheduled.");

                // ======================================================
                // 2. Check selected slot is still free
                // This protects other users from double booking
                // ======================================================
                var alreadyBooked = await connection.ExecuteScalarAsync<int>(@"
            SELECT COUNT(1)
            FROM bookedslots
            WHERE SlotId = @SlotId
              AND SlotDate = @SlotDate
              AND StartTime = @StartTime
              AND EndTime = @EndTime",
                    new
                    {
                        SlotId = slotId,
                        SlotDate = newDate.Date,
                        StartTime = newStart,
                        EndTime = newEnd
                    },
                    tx);

                if (alreadyBooked > 0)
                    throw new Exception("Selected slot is already booked by someone else.");

                // ======================================================
                // 3. Mark old meeting as Rescheduled
                // ======================================================
                await connection.ExecuteAsync(@"
            UPDATE meetings
            SET Status = 'Rescheduled',
                ModifiedAt = NOW()
            WHERE BookingId = @BookingId",
                    new { BookingId = oldBookingId },
                    tx);

                // ======================================================
                // 4. Mark old booking as Rescheduled
                // For auto no-show, reason is stored in reschedules table
                // ======================================================
                await connection.ExecuteAsync(@"
            UPDATE bookings
            SET Status = 'Rescheduled',
                ModifiedAt = NOW(),
                ModifiedBy = @ModifiedBy
            WHERE BookingId = @BookingId",
                    new
                    {
                        BookingId = oldBookingId,
                        ModifiedBy = rescheduledBy
                    },
                    tx);

                // ======================================================
                // 5. Create new booking
                // ======================================================
                var newBookingId = await connection.ExecuteScalarAsync<int>(@"
            INSERT INTO bookings
            (
                UserId,
                ClientId,
                ServiceId,
                ScheduleDate,
                StartTime,
                EndTime,
                Status,
                CreatedAt,
                CreatedBy
            )
            VALUES
            (
                @UserId,
                @ClientId,
                @ServiceId,
                @ScheduleDate,
                @StartTime,
                @EndTime,
                'Pending',
                NOW(),
                @CreatedBy
            );
            SELECT LAST_INSERT_ID();",
                    new
                    {
                        old.UserId,
                        old.ClientId,
                        old.ServiceId,
                        ScheduleDate = newDate.Date,
                        StartTime = newStart,
                        EndTime = newEnd,
                        CreatedBy = rescheduledBy
                    },
                    tx);

                // ======================================================
                // 6. Create fresh meeting link
                // ======================================================
                var newMeetingLink = GenerateMiroTalkLink(newBookingId);
                var newMeetingStart = newDate.Date.Add(newStart);
                var newMeetingEnd = newDate.Date.Add(newEnd);

                await connection.ExecuteAsync(@"
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
                'Pending',
                NOW(),
                @CreatedBy
            );",
                    new
                    {
                        old.UserId,
                        BookingId = newBookingId,
                        StartTime = newMeetingStart,
                        EndTime = newMeetingEnd,
                        MeetingLink = newMeetingLink,
                        CreatedBy = rescheduledBy
                    },
                    tx);

                // ======================================================
                // 7. Insert new booked slot
                // ======================================================
                await connection.ExecuteAsync(@"
            INSERT INTO bookedslots
            (
                SlotId,
                UserId,
                ServiceId,
                BookingId,
                SlotDate,
                StartTime,
                EndTime,
                CreatedAt
            )
            VALUES
            (
                @SlotId,
                @UserId,
                @ServiceId,
                @BookingId,
                @SlotDate,
                @StartTime,
                @EndTime,
                NOW()
            )",
                    new
                    {
                        SlotId = slotId,
                        old.UserId,
                        old.ServiceId,
                        BookingId = newBookingId,
                        SlotDate = newDate.Date,
                        StartTime = newStart,
                        EndTime = newEnd
                    },
                    tx);

                // ======================================================
                // 8. Insert reschedule history
                // reason = Manual or NoShow
                // ======================================================
                await connection.ExecuteAsync(@"
            INSERT INTO reschedules
            (
                OldBookingId,
                NewBookingId,
                Reason,
                RescheduledBy,
                CreatedAt
            )
            VALUES
            (
                @OldBookingId,
                @NewBookingId,
                @Reason,
                @RescheduledBy,
                NOW()
            )",
                    new
                    {
                        OldBookingId = oldBookingId,
                        NewBookingId = newBookingId,
                        Reason = reason,
                        RescheduledBy = rescheduledBy
                    },
                    tx);

                await tx.CommitAsync();

                // ======================================================
                // 9. Send email after commit
                // ======================================================
                string capturedEmail = old.ClientEmail ?? "";
                string capturedName = old.ClientName ?? "Client";
                string capturedService = old.ServiceTitle ?? "the service";
                string capturedDate = newDate.ToString("dddd, MMMM dd yyyy");
                string capturedTime = newMeetingStart.ToString("hh:mm tt", CultureInfo.InvariantCulture);
                string capturedLink = $"{newMeetingLink}?userId={old.ClientId}&email={Uri.EscapeDataString(capturedEmail)}";

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailRepository.SendBookingConfirmationEmailAsync(
                            capturedEmail,
                            capturedName,
                            capturedService,
                            capturedDate,
                            capturedTime);

                        await _emailRepository.SendMeetingInviteEmailAsync(
                            capturedEmail,
                            capturedName,
                            capturedLink);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[RescheduleEmailError] OldBookingId:{oldBookingId} NewBookingId:{newBookingId} - {ex.Message}");
                    }
                });

                return newBookingId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }


        public async Task<BookingModalGet>
    GetBookingModal(
        int serviceId
    )
        {
            var service =
                await _serviceRepository
                    .GetById(serviceId);

            if (service == null)
            {
                return null;
            }

            return new BookingModalGet
            {
                ServiceId = service.ServiceId,
                Title = service.ServiceTitle,
                ShortDescription = service.ShortDescription,
                DurationMinutes = int.TryParse(service.Duration, out var d) ? d : 30,
                Price = service.Price,
                UserId = service.UserId,
                ExpertName = service.FullName,
                ExpertRole = service.CategoryName,
                ExpertImage = service.ProfileUrl
            };
        }


        public async Task<IEnumerable<BookingRead>> GetAllAsync()
        {
            using var connection = _db.GetConnection();

            var sql = @"
                SELECT
                    b.BookingId,

                    b.UserId,
                    u.FullName AS UserName,

                    b.ClientId,
                    cu.FullName AS ClientName,

                    b.ServiceId,
                    s.ServiceTitle,

                    b.ScheduleDate,
                    b.StartTime,
                    b.EndTime,

                    b.Status,
                    b.CreatedAt,
                    b.CreatedBy

                FROM bookings b

                INNER JOIN users u
                    ON b.UserId = u.UserId

                INNER JOIN users cu
                    ON b.ClientId = cu.UserId

                INNER JOIN services s
                    ON b.ServiceId = s.ServiceId

                ORDER BY b.BookingId DESC";

            return await connection.QueryAsync<BookingRead>(
                sql);
        }

        public async Task<BookingRead> GetByIdAsync(
            int bookingId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                SELECT
                    b.BookingId,

                    b.UserId,
                    u.FullName AS UserName,

                    b.ClientId,
                    cu.FullName AS ClientName,

                    b.ServiceId,
                    s.ServiceTitle,

                    b.ScheduleDate,
                    b.StartTime,
                    b.EndTime,

                    b.Status,
                    b.CreatedAt,
                    b.CreatedBy

                FROM bookings b

                INNER JOIN users u
                    ON b.UserId = u.UserId

                INNER JOIN users cu
                    ON b.ClientId = cu.UserId

                INNER JOIN services s
                    ON b.ServiceId = s.ServiceId

                WHERE b.BookingId = @BookingId";

            return await connection.QueryFirstOrDefaultAsync<BookingRead>(
                sql,
                new
                {
                    BookingId = bookingId
                });
        }

        public async Task<bool> UpdateAsync(
            BookingPut model)
        {
            using var connection = _db.GetConnection();

            if (model.UserId == model.ClientId)
            {
                throw new Exception(
                    "UserId and ClientId cannot be same.");
            }

            var sql = @"
                UPDATE bookings
                SET
                    UserId = @UserId,
                    ClientId = @ClientId,
                    ServiceId = @ServiceId,

                    ScheduleDate = @ScheduleDate,
                    StartTime = @StartTime,
                    EndTime = @EndTime,

                    Status = @Status,

                    ModifiedAt = NOW(),
                    ModifiedBy = @ModifiedBy

                WHERE BookingId = @BookingId";

            var rows =
                await connection.ExecuteAsync(
                    sql,
                    model);
            // IF BOOKING REJECTED OR CANCELLED
            if (
                model.Status == "Rejected"
                || model.Status == "Cancelled"
            )
            {
                await _userSlotRepo.MarkAsUnbooked(
                    model.SlotId
                );
            }

            return rows > 0;
        }

        public async Task<bool> DeleteAsync(
            int bookingId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                DELETE FROM bookings
                WHERE BookingId = @BookingId";

            var rows =
                await connection.ExecuteAsync(
                    sql,
                    new
                    {
                        BookingId = bookingId
                    });

            return rows > 0;
        }

        // GET BY USER ID
        public async Task<PagedResult<BookingRead>> GetByUserIdAsync(
            int userId,
            int pageNumber,
            int pageSize,
            string? search)
        {
            using var connection = _db.GetConnection();

            var countSql = @"
                SELECT COUNT(*)

                FROM bookings b

                INNER JOIN users u
                    ON b.UserId = u.UserId

                LEFT JOIN categories c
                    ON u.CategoryId = c.CategoryId

                INNER JOIN users cu
                    ON b.ClientId = cu.UserId

                INNER JOIN services s
                    ON b.ServiceId = s.ServiceId

                WHERE b.UserId = @UserId

                AND
                (
                    @Search IS NULL
                    OR @Search = ''

                    OR u.FullName
                        LIKE CONCAT('%', @Search, '%')

                    OR cu.FullName
                        LIKE CONCAT('%', @Search, '%')

                    OR s.ServiceTitle
                        LIKE CONCAT('%', @Search, '%')

                    OR c.CategoryName
                        LIKE CONCAT('%', @Search, '%')

                    OR b.Status
                        LIKE CONCAT('%', @Search, '%')
                );";

            var totalCount =
                await connection.ExecuteScalarAsync<int>(
                    countSql,
                    new
                    {
                        UserId = userId,
                        Search = search
                    });

            var sql = @"
                SELECT
                    b.BookingId,

                    b.UserId,
                    u.FullName AS UserName,

                    c.CategoryId,
                    c.CategoryName,

                    b.ClientId,
                    cu.FullName AS ClientName,

                    b.ServiceId,
                    s.ServiceTitle,

                    b.ScheduleDate,
                    b.StartTime,
                    b.EndTime,

                    b.Status,
                    b.CreatedAt,
                    b.CreatedBy

                FROM bookings b

                INNER JOIN users u
                    ON b.UserId = u.UserId

                LEFT JOIN categories c
                    ON u.CategoryId = c.CategoryId

                INNER JOIN users cu
                    ON b.ClientId = cu.UserId

                INNER JOIN services s
                    ON b.ServiceId = s.ServiceId

                WHERE b.UserId = @UserId

                AND
                (
                    @Search IS NULL
                    OR @Search = ''

                    OR u.FullName
                        LIKE CONCAT('%', @Search, '%')

                    OR cu.FullName
                        LIKE CONCAT('%', @Search, '%')

                    OR s.ServiceTitle
                        LIKE CONCAT('%', @Search, '%')

                    OR c.CategoryName
                        LIKE CONCAT('%', @Search, '%')

                    OR b.Status
                        LIKE CONCAT('%', @Search, '%')
                )

                ORDER BY b.BookingId DESC

                LIMIT @PageSize OFFSET @Offset;";

            var data =
                await connection.QueryAsync<BookingRead>(
                    sql,
                    new
                    {
                        UserId = userId,
                        Search = search,
                        PageSize = pageSize,
                        Offset = (pageNumber - 1) * pageSize
                    });

            return new PagedResult<BookingRead>
            {
                TotalCount = totalCount,
                Details = data
            };
        }

        // GET BY CLIENT ID
        public async Task<PagedResult<BookingRead>> GetByClientIdAsync(
            int clientId,
            int pageNumber,
            int pageSize,
            string? search)
        {
            using var connection = _db.GetConnection();

            var countSql = @"
                SELECT COUNT(*)

                FROM bookings b

                INNER JOIN users u
                    ON b.UserId = u.UserId

                LEFT JOIN categories c
                    ON u.CategoryId = c.CategoryId

                INNER JOIN users cu
                    ON b.ClientId = cu.UserId

                INNER JOIN services s
                    ON b.ServiceId = s.ServiceId

                WHERE b.ClientId = @ClientId

                AND
                (
                    @Search IS NULL
                    OR @Search = ''

                    OR u.FullName
                        LIKE CONCAT('%', @Search, '%')

                    OR cu.FullName
                        LIKE CONCAT('%', @Search, '%')

                    OR s.ServiceTitle
                        LIKE CONCAT('%', @Search, '%')

                    OR c.CategoryName
                        LIKE CONCAT('%', @Search, '%')

                    OR b.Status
                        LIKE CONCAT('%', @Search, '%')
                );";

            var totalCount =
                await connection.ExecuteScalarAsync<int>(
                    countSql,
                    new
                    {
                        ClientId = clientId,
                        Search = search
                    });

            var sql = @"
                SELECT
                    b.BookingId,

                    b.UserId,
                    u.FullName AS UserName,

                    c.CategoryId,
                    c.CategoryName,

                    b.ClientId,
                    cu.FullName AS ClientName,

                    b.ServiceId,
                    s.ServiceTitle,

                    b.ScheduleDate,
                    b.StartTime,
                    b.EndTime,

                    b.Status,
                    b.CreatedAt,
                    b.CreatedBy

                FROM bookings b

                INNER JOIN users u
                    ON b.UserId = u.UserId

                LEFT JOIN categories c
                    ON u.CategoryId = c.CategoryId

                INNER JOIN users cu
                    ON b.ClientId = cu.UserId

                INNER JOIN services s
                    ON b.ServiceId = s.ServiceId

                WHERE b.ClientId = @ClientId

                AND
                (
                    @Search IS NULL
                    OR @Search = ''

                    OR u.FullName
                        LIKE CONCAT('%', @Search, '%')

                    OR cu.FullName
                        LIKE CONCAT('%', @Search, '%')

                    OR s.ServiceTitle
                        LIKE CONCAT('%', @Search, '%')

                    OR c.CategoryName
                        LIKE CONCAT('%', @Search, '%')

                    OR b.Status
                        LIKE CONCAT('%', @Search, '%')
                )

                ORDER BY b.BookingId DESC

                LIMIT @PageSize OFFSET @Offset;";

            var data =
                await connection.QueryAsync<BookingRead>(
                    sql,
                    new
                    {
                        ClientId = clientId,
                        Search = search,
                        PageSize = pageSize,
                        Offset = (pageNumber - 1) * pageSize
                    });

            return new PagedResult<BookingRead>
            {
                TotalCount = totalCount,
                Details = data
            };
        }
    }
}