using ahello_backend.DbContexts;
using ahello_backend.Models.Bookings;
using ahello_backend.Models.Meeting;
using ahello_backend.Models.Pagination;
using ahello_backend.Models.Reschedulerequest;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Classes;
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
        private readonly HundredMsService _hundredMsService;
        //private readonly IPdfService _pdfService;
        //private readonly IInvoiceRepository _invoiceRepository;

        public BookingRepository(
         DbContext db,
         DbContextConnection dbConn,
         IUserSlotRepository userSlotRepo,
         IServiceRepository serviceRepository,
         IEmailRepository emailRepository,
         HundredMsService hundredMsService)
        {
            _db = db;
            _dbConn = dbConn;
            _userSlotRepo = userSlotRepo;
            _serviceRepository = serviceRepository;
            _emailRepository = emailRepository;
            _hundredMsService = hundredMsService;
            
        }

        public async Task<int> CreateAsync(BookingPost model)
        {
            if (model.UserId == model.ClientId)
                throw new Exception("You cannot book your own service.");

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

            int bookingId = 0;
            string meetingLink = string.Empty;
            DateTime meetingStartTime = default;

            using var connection = _dbConn.GetMyConnection();
            await connection.OpenAsync();
            using var tx = await connection.BeginTransactionAsync();

            try
            {
                // ── NEW: Check slot isn't already booked before inserting ──
                var alreadyBooked = await connection.ExecuteScalarAsync<int>(@"
                SELECT COUNT(1)
                FROM bookedslots
                WHERE SlotId = @SlotId
                AND SlotDate = @SlotDate
                AND StartTime = @StartTime
                AND EndTime = @EndTime",
                    new
                    {
                        model.SlotId,
                        SlotDate = model.ScheduleDate,
                        StartTime = model.StartTime,
                        EndTime = model.EndTime
                    },
                    tx);

                if (alreadyBooked > 0)
                    throw new Exception("This slot has already been booked. Please choose another time.");

                var bookingSql = @"
                INSERT INTO bookings
                (UserId, ClientId, ServiceId, ScheduleDate, StartTime, EndTime, Status, CreatedAt, CreatedBy)
                VALUES
                (@UserId, @ClientId, @ServiceId, @ScheduleDate, @StartTime, @EndTime, @Status, NOW(), @CreatedBy);
                SELECT LAST_INSERT_ID();";

                bookingId = await connection.ExecuteScalarAsync<int>(bookingSql, model, tx);

                var roomId = await _hundredMsService.CreateRoomAsync(bookingId);
                var roomCodes = await _hundredMsService.CreateRoomCodesAsync(roomId);

                meetingLink = GenerateAhlloMeetingLink(bookingId);
                meetingStartTime = model.ScheduleDate.Date.Add(model.StartTime);
                var meetingEndTime = model.ScheduleDate.Date.Add(model.EndTime);

                await connection.ExecuteAsync(@"
                INSERT INTO meetings
                (
                    UserId, BookingId, StartTime, EndTime,
                    MeetingLink, RoomId, HostRoomCode, ClientRoomCode,
                    Status, CreatedAt, CreatedBy
                )
                VALUES
                (
                    @UserId, @BookingId, @StartTime, @EndTime,
                    @MeetingLink, @RoomId, @HostRoomCode, @ClientRoomCode,
                    'Pending', NOW(), @CreatedBy
                );",
                new
                {
                    model.UserId,
                    BookingId = bookingId,
                    StartTime = meetingStartTime,
                    EndTime = meetingEndTime,
                    MeetingLink = meetingLink,
                    RoomId = roomId,
                    HostRoomCode = roomCodes.HostCode,
                    ClientRoomCode = roomCodes.ClientCode,
                    model.CreatedBy
                }, tx);

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
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

            string clientEmail = client.Email;
            string clientFullName = client.FullName;
            string serviceName = service?.ServiceTitle ?? "the service";
            string formattedDate = model.ScheduleDate.ToString("dddd, MMMM dd yyyy");
            string formattedTime = model.StartTime.Hours >= 12
            ? $"{(model.StartTime.Hours > 12 ? model.StartTime.Hours - 12 : model.StartTime.Hours):00}:{model.StartTime.Minutes:00} PM"
            : $"{(model.StartTime.Hours == 0 ? 12 : model.StartTime.Hours):00}:{model.StartTime.Minutes:00} AM";
            string capturedMeetingLink = $"{meetingLink}?userId={model.ClientId}&email={Uri.EscapeDataString(clientEmail)}";

            _ = Task.Run(async () =>
            {
                try
                {
                
                    // Send booking confirmation WITHOUT invoice
                    //await _emailRepository.SendBookingConfirmationEmailAsync(
                    //    clientEmail,
                    //    clientFullName,
                    //    serviceName,
                    //    formattedDate,
                    //    formattedTime,
                    //    null);

                    // Send meeting invitation
                    await _emailRepository.SendMeetingInviteEmailAsync(
                        clientEmail,
                        clientFullName,
                        capturedMeetingLink);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EmailError] {ex.GetType().Name}: {ex.Message}");
                }
            });

            return bookingId;
        }

        // AHLLO MEETING LINK GENERATOR
        private string GenerateAhlloMeetingLink(int bookingId)
        {
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            var roomName = $"ahllo-{bookingId}-{uniqueId}";
            return $"https://ahllo.com/meeting/join/{roomName}";
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
                if (old.Status == "Rescheduled")
                    throw new Exception(
                        "This booking has already been rescheduled.");

                var slot = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
    SELECT
        SlotId,
        UserId,
        ServiceId,
        SlotDate,
        StartTime,
        EndTime,
        IsBooked,
        RecurrenceType,
        DayOfWeek,
        DayOfMonth
    FROM userslots
    WHERE SlotId = @SlotId
      AND UserId = @UserId
      AND ServiceId = @ServiceId
      AND IsBooked = 0
      AND
      (
          -- Specific date
          (
              RecurrenceType = 'SpecificDate'
              AND SlotDate = @SlotDate
          )

          OR

          -- Daily
          (
              RecurrenceType = 'Daily'
          )

          OR

          -- Weekly
          (
              RecurrenceType = 'Weekly'
              AND DayOfWeek = DAYOFWEEK(@SlotDate)
          )

          OR

          -- Monthly
          (
              RecurrenceType = 'Monthly'
              AND DayOfMonth = DAY(@SlotDate)
          )

          OR

          -- Custom
          (
              RecurrenceType = 'Custom'
              AND SlotDate = @SlotDate
          )
      )
      AND @StartTime >= StartTime
      AND @EndTime <= EndTime
        ",
        new
        {
            SlotId = slotId,
            UserId = old.UserId,
            ServiceId = old.ServiceId,
            SlotDate = newDate.Date,
            StartTime = newStart,
            EndTime = newEnd
        },
        tx);

                if (slot == null)
                {
                    throw new Exception(
                        "Selected slot is not available for this host/service/date/time.");
                }

                if (slot == null)
                {
                    throw new Exception(
                        "Selected slot is not available for this host.");
                }
                // ======================================================
                // 2. Check selected slot is still free
                // This protects other users from double booking
                // ======================================================
                var alreadyBooked = await connection.ExecuteScalarAsync<int>(@"
                SELECT COUNT(1)
                FROM bookedslots
                WHERE SlotId = @SlotId
                  AND SlotDate = @SlotDate
                  AND @StartTime < EndTime
                  AND @EndTime > StartTime
                    ",
                    new
                    {
                SlotId = slotId,
                SlotDate = newDate.Date,
                StartTime = newStart,
                EndTime = newEnd
                    },
                    tx);

                if (alreadyBooked > 0)
                {
                    throw new Exception(
                        "Selected time is already booked by someone else.");
                }
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
                // 6. Create 100ms room and fresh Ahllo meeting link
                // ======================================================
                var newRoomId = await _hundredMsService.CreateRoomAsync(newBookingId);
                var newRoomCodes = await _hundredMsService.CreateRoomCodesAsync(newRoomId);

                var newMeetingLink = GenerateAhlloMeetingLink(newBookingId);
                var newMeetingStart = newDate.Date.Add(newStart);
                var newMeetingEnd = newDate.Date.Add(newEnd);

                await connection.ExecuteAsync(@"
                INSERT INTO meetings
                (
                    UserId, BookingId, StartTime, EndTime,
                    MeetingLink, RoomId, HostRoomCode, ClientRoomCode,
                    Status, CreatedAt, CreatedBy
                )
                VALUES
                (
                    @UserId, @BookingId, @StartTime, @EndTime,
                    @MeetingLink, @RoomId, @HostRoomCode, @ClientRoomCode,
                    'Pending', NOW(), @CreatedBy
                );",
                new
                {
                    old.UserId,
                    BookingId = newBookingId,
                    StartTime = newMeetingStart,
                    EndTime = newMeetingEnd,
                    MeetingLink = newMeetingLink,
                    RoomId = newRoomId,
                    HostRoomCode = newRoomCodes.HostCode,
                    ClientRoomCode = newRoomCodes.ClientCode,
                    CreatedBy = rescheduledBy
                }, tx);

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
                        Console.WriteLine($"[RescheduleEmail] Sending to: {capturedEmail}");

                        await _emailRepository.SendRescheduleConfirmationEmailAsync(
                            capturedEmail,
                            capturedName,
                            capturedService,
                            capturedDate,
                            capturedTime);

                        Console.WriteLine("[RescheduleEmail] Confirmation email sent");

                        await _emailRepository.SendMeetingInviteEmailAsync(
                            capturedEmail,
                            capturedName,
                            capturedLink);

                        Console.WriteLine("[RescheduleEmail] Meeting invite email sent");
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

        public async Task<BookingModalGet> GetBookingModal(int serviceId)
        {
            var service = await _serviceRepository.GetById(serviceId);

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

            return await connection.QueryAsync<BookingRead>(sql);
        }

        public async Task<BookingRead> GetByIdAsync(int bookingId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                SELECT
                    b.BookingId,

                    b.UserId,
                    u.FullName AS UserName,

                    b.ClientId,
                    cu.FullName AS ClientName,
                    cu.Email AS ClientEmail,
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
                new { BookingId = bookingId });
        }

        public async Task<bool> UpdateAsync(BookingPut model)
        {
            using var connection = _db.GetConnection();

            if (model.UserId == model.ClientId)
            {
                throw new Exception("UserId and ClientId cannot be same.");
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

            var rows = await connection.ExecuteAsync(sql, model);

            // IF BOOKING REJECTED OR CANCELLED
            if (model.Status == "Rejected" || model.Status == "Cancelled")
            {
                await _userSlotRepo.MarkAsUnbooked(model.SlotId);
            }

            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int bookingId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                DELETE FROM bookings
                WHERE BookingId = @BookingId";

            var rows = await connection.ExecuteAsync(
                sql,
                new { BookingId = bookingId });

            return rows > 0;
        }

        // GET BY USER ID
        public async Task<PagedResult<BookingRead>> GetByUserIdAsync(
            int userId,
            int pageNumber,
            int pageSize,
            string? search, string? status,
            DateTime? scheduleDate)
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
                LEFT JOIN servicecategorydynamic sc
                     ON s.ServiceCategoryId = sc.ServiceCategoryId

                WHERE b.UserId = @UserId
                AND
                (
                    @Status IS NULL
                    OR @Status = ''
                    OR b.Status = @Status
                )
                AND
                (
                    @ScheduleDate IS NULL
                    OR DATE(b.ScheduleDate) = DATE(@ScheduleDate)
                )
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
                    OR sc.ServiceCategoryName
                        LIKE CONCAT('%', @Search, '%')
                );";

            var totalCount = await connection.ExecuteScalarAsync<int>(
                countSql,
                new
                {
                    UserId = userId,
                    Search = search,
                    Status = status,
                    ScheduleDate = scheduleDate
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
                    s.ServiceCategoryId,
                    sc.ServiceCategoryName,
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
                LEFT JOIN servicecategorydynamic sc
                    ON s.ServiceCategoryId = sc.ServiceCategoryId

                WHERE b.UserId = @UserId
                AND
                (
                    @Status IS NULL
                    OR @Status = ''
                    OR b.Status = @Status
                )

                AND
                (
                    @ScheduleDate IS NULL
                    OR DATE(b.ScheduleDate) = DATE(@ScheduleDate)
                )
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
                    OR sc.ServiceCategoryName
                        LIKE CONCAT('%', @Search, '%')
                )

                ORDER BY b.ScheduleDate DESC, b.StartTime DESC, b.BookingId DESC


                LIMIT @PageSize OFFSET @Offset;";

            var data = await connection.QueryAsync<BookingRead>(
                sql,
                new
                {
                    UserId = userId,
                    Search = search,
                    Status = status,
                    ScheduleDate = scheduleDate,
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
            string? search, string? status,
            DateTime? scheduleDate)
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
                LEFT JOIN servicecategorydynamic sc
                     ON s.ServiceCategoryId = sc.ServiceCategoryId

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
                    OR sc.ServiceCategoryName
                        LIKE CONCAT('%', @Search, '%')
                );";

            var totalCount = await connection.ExecuteScalarAsync<int>(
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
                    s.ServiceCategoryId,
                    sc.ServiceCategoryName,
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
                LEFT JOIN servicecategorydynamic sc
                     ON s.ServiceCategoryId = sc.ServiceCategoryId
                WHERE b.ClientId = @ClientId
                AND
                (
                    @Status IS NULL
                    OR @Status = ''
                    OR b.Status = @Status
                )

                AND
                (
                    @ScheduleDate IS NULL
                    OR DATE(b.ScheduleDate) = DATE(@ScheduleDate)
                )

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
                    OR sc.ServiceCategoryName
                        LIKE CONCAT('%', @Search, '%')
                )

               ORDER BY b.ScheduleDate DESC, b.StartTime DESC, b.BookingId DESC


                LIMIT @PageSize OFFSET @Offset;";

            var data = await connection.QueryAsync<BookingRead>(
                sql,
                new
                {
                    ClientId = clientId,
                    Search = search,
                    Status = status,
                    ScheduleDate = scheduleDate,
                    PageSize = pageSize,
                    Offset = (pageNumber - 1) * pageSize
                });

            return new PagedResult<BookingRead>
            {
                TotalCount = totalCount,
                Details = data
            };
        }
        public async Task<List<ServiceWiseClientGet>> GetClientsServiceWiseAsync(
        int userId,
        int pageNumber,
        int pageSize,
        string? search,
        string? bookingStatus,
        DateTime? lastBookingDate)
        {
            using var connection = _db.GetConnection();

            var sql = @"
        SELECT
            s.ServiceId,
            s.ServiceTitle,

            c.UserId AS ClientId,
            c.FullName AS ClientName,
            c.Email,

            MAX(b.ScheduleDate) AS LastBookingDate,

        SUBSTRING_INDEX(
            GROUP_CONCAT(
            b.StartTime
            ORDER BY b.ScheduleDate DESC, b.StartTime DESC
        ),
        ',',
        1
        ) AS StartTime,

        SUBSTRING_INDEX(
            GROUP_CONCAT(
            b.EndTime
            ORDER BY b.ScheduleDate DESC, b.StartTime DESC
        ),
        ',',
        1
        ) AS EndTime,

        SUBSTRING_INDEX(
        GROUP_CONCAT(
            b.Status
            ORDER BY b.ScheduleDate DESC, b.StartTime DESC
        ),
        ',',
        1
         ) AS BookingStatus

        FROM bookings b

        INNER JOIN services s
            ON s.ServiceId = b.ServiceId

        INNER JOIN users c
            ON c.UserId = b.ClientId

        WHERE b.UserId = @UserId

        AND (
            @Search IS NULL
            OR @Search = ''
            OR c.FullName LIKE CONCAT('%', @Search, '%')
            OR c.Email LIKE CONCAT('%', @Search, '%')
            OR s.ServiceTitle LIKE CONCAT('%', @Search, '%')
        )

        AND (
            @BookingStatus IS NULL
            OR @BookingStatus = ''
            OR b.Status = @BookingStatus
        )

        AND (
            @LastBookingDate IS NULL
            OR DATE(b.ScheduleDate) = DATE(@LastBookingDate)
        )

        GROUP BY
            s.ServiceId,
            s.ServiceTitle,
            c.UserId,
            c.FullName,
            c.Email

        ORDER BY
            s.ServiceId DESC,
            LastBookingDate DESC;
            ";

            var rows = await connection.QueryAsync<ServiceWiseClientRow>(
            sql,
            new
            {
                UserId = userId,
                Search = search,
                BookingStatus = bookingStatus,
                LastBookingDate = lastBookingDate
            });

                    var result = rows
            .GroupBy(x => new
            {
                x.ServiceId,
                x.ServiceTitle
            })
            .Select(g => new ServiceWiseClientGet
            {
                ServiceId = g.Key.ServiceId,
                ServiceTitle = g.Key.ServiceTitle,
                TotalClients = g.Count(),

                Clients = g.Select(x => new ServiceClientGet
                {
                    ClientId = x.ClientId,
                    ClientName = x.ClientName,
                    Email = x.Email,
                    LastBookingDate = x.LastBookingDate,
                    StartTime = string.IsNullOrEmpty(x.StartTime)
                        ? null
                        : TimeSpan.Parse(x.StartTime),
                    EndTime = string.IsNullOrEmpty(x.EndTime)
                        ? null
                        : TimeSpan.Parse(x.EndTime),
                    BookingStatus = x.BookingStatus
                }).ToList()
            })
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

            return result;
        }
        public async Task<int> CreateRescheduleRequestAsync(
    RescheduleRequestPost model)
        {
            using var connection = _dbConn.GetMyConnection();
            await connection.OpenAsync();

            using var tx = await connection.BeginTransactionAsync();

            try
            {
                // ============================================================
                // 1. Get current booking
                // ============================================================

                var booking = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT
                BookingId,
                UserId,
                ClientId,
                ServiceId,
                ScheduleDate,
                StartTime,
                EndTime,
                Status
            FROM bookings
            WHERE BookingId = @BookingId
        ",
                new
                {
                    model.BookingId
                },
                tx);

                if (booking == null)
                    throw new Exception("Booking not found.");

                // ============================================================
                // 2. Make sure logged-in client belongs to this booking
                // ============================================================

                // If you have ClientId from authentication,
                // it is better to pass it into this method.
                // For now, the booking itself is validated.

                if (booking.Status == "Cancelled")
                    throw new Exception(
                        "Cancelled booking cannot be rescheduled.");

                if (booking.Status == "Rejected")
                    throw new Exception(
                        "Rejected booking cannot be rescheduled.");

                if (booking.Status == "Rescheduled")
                    throw new Exception(
                        "This booking has already been rescheduled.");

                // ============================================================
                // 3. Validate requested slot belongs to host
                // ============================================================

                var slot = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                        SELECT
                            SlotId,
                            UserId,
                            ServiceId,
                            SlotDate,
                            StartTime,
                            EndTime,
                            IsBooked,
                            RecurrenceType,
                            DayOfWeek,
                            DayOfMonth
                        FROM userslots
                        WHERE SlotId = @SlotId
                          AND UserId = @UserId
                          AND ServiceId = @ServiceId
                          AND IsBooked = 0
                          AND
                          (
                              -- Specific date slot
                              (
                                  RecurrenceType = 'SpecificDate'
                                  AND SlotDate = @RequestedDate
                              )

                              OR

                              -- Daily slot
                              (
                                  RecurrenceType = 'Daily'
                              )

                              OR

                              -- Weekly slot
                              (
                                  RecurrenceType = 'Weekly'
                                  AND DayOfWeek = DAYOFWEEK(@RequestedDate)
                              )

                              OR

                              -- Monthly slot
                              (
                                  RecurrenceType = 'Monthly'
                                  AND DayOfMonth = DAY(@RequestedDate)
                              )

                              OR

                              -- Custom / fallback
                              (
                                  RecurrenceType = 'Custom'
                                  AND SlotDate = @RequestedDate
                              )
                          )
                          AND @RequestedStartTime >= StartTime
                          AND @RequestedEndTime <= EndTime
                    ",
                    new
                    {
                        model.SlotId,
                        UserId = booking.UserId,
                        ServiceId = booking.ServiceId,
                        RequestedDate = model.RequestedDate.Date,
                        model.RequestedStartTime,
                        model.RequestedEndTime
                    },
                    tx);

                if (slot == null)
                {
                    throw new Exception(
                        "Selected slot is not available for this host/service/date/time.");
                }
                // ============================================================
                // 4. Check slot is already booked
                // ============================================================

                var alreadyBooked = await connection.ExecuteScalarAsync<int>(@"
            SELECT COUNT(1)
            FROM bookedslots
            WHERE SlotId = @SlotId
            AND SlotDate = @RequestedDate
            AND StartTime = @RequestedStartTime
            AND EndTime = @RequestedEndTime
        ",
                new
                {
                    model.SlotId,
                    RequestedDate = model.RequestedDate.Date,
                    model.RequestedStartTime,
                    model.RequestedEndTime
                },
                tx);

                if (alreadyBooked > 0)
                    throw new Exception(
                        "Selected slot is already booked.");

                // ============================================================
                // 5. Check existing pending request
                // ============================================================

                var pendingRequest = await connection.ExecuteScalarAsync<int>(@"
            SELECT COUNT(1)
            FROM reschedulerequests
            WHERE BookingId = @BookingId
            AND Status = 'Pending'
        ",
                new
                {
                    model.BookingId
                },
                tx);

                if (pendingRequest > 0)
                    throw new Exception(
                        "A reschedule request is already pending for this booking.");

                // ============================================================
                // 6. Insert request
                // ============================================================

                var requestId = await connection.ExecuteScalarAsync<int>(@"
            INSERT INTO reschedulerequests
            (
                BookingId,
                UserId,
                ClientId,
                ServiceId,
                SlotId,
                RequestedDate,
                RequestedStartTime,
                RequestedEndTime,
                Reason,
                Status,
                CreatedAt,
                CreatedBy
            )
            VALUES
            (
                @BookingId,
                @UserId,
                @ClientId,
                @ServiceId,
                @SlotId,
                @RequestedDate,
                @RequestedStartTime,
                @RequestedEndTime,
                @Reason,
                'Pending',
                NOW(),
                @CreatedBy
            );

            SELECT LAST_INSERT_ID();
        ",
                new
                {
                    model.BookingId,
                    UserId = booking.UserId,
                    ClientId = booking.ClientId,
                    ServiceId = booking.ServiceId,
                    model.SlotId,
                    RequestedDate = model.RequestedDate.Date,
                    model.RequestedStartTime,
                    model.RequestedEndTime,
                    model.Reason,
                    model.CreatedBy
                },
                tx);

                await tx.CommitAsync();

                return requestId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
        public async Task<IEnumerable<RescheduleRequestRead>>
    GetRescheduleRequestsByUserIdAsync(int userId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
        SELECT
            r.RequestId,

            r.BookingId,

            r.UserId,
            u.FullName AS UserName,
            u.Email AS UserEmail,

            r.ClientId,
            c.FullName AS ClientName,
            c.Email AS ClientEmail,

            r.ServiceId,
            s.ServiceTitle,

            r.SlotId,

            r.RequestedDate,
            r.RequestedStartTime,
            r.RequestedEndTime,

            r.Reason,
            r.Status,

            r.CreatedAt,
            r.CreatedBy,

            r.ModifiedAt,
            r.ModifiedBy

        FROM reschedulerequests r

        INNER JOIN users u
            ON r.UserId = u.UserId

        INNER JOIN users c
            ON r.ClientId = c.UserId

        INNER JOIN services s
            ON r.ServiceId = s.ServiceId

        WHERE r.UserId = @UserId

        ORDER BY
            CASE
                WHEN r.Status = 'Pending' THEN 1
                WHEN r.Status = 'Accepted' THEN 2
                ELSE 3
            END,
            r.CreatedAt DESC;
    ";

            return await connection.QueryAsync<RescheduleRequestRead>(
                sql,
                new { UserId = userId });
        }
        public async Task<RescheduleRequestRead?>
    GetRescheduleRequestByIdAsync(int requestId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
        SELECT
            r.RequestId,

            r.BookingId,

            r.UserId,
            u.FullName AS UserName,
            u.Email AS UserEmail,

            r.ClientId,
            c.FullName AS ClientName,
            c.Email AS ClientEmail,

            r.ServiceId,
            s.ServiceTitle,

            r.SlotId,

            r.RequestedDate,
            r.RequestedStartTime,
            r.RequestedEndTime,

            r.Reason,
            r.Status,

            r.CreatedAt,
            r.CreatedBy,

            r.ModifiedAt,
            r.ModifiedBy

        FROM reschedulerequests r

        INNER JOIN users u
            ON r.UserId = u.UserId

        INNER JOIN users c
            ON r.ClientId = c.UserId

        INNER JOIN services s
            ON r.ServiceId = s.ServiceId

        WHERE r.RequestId = @RequestId;
    ";

            return await connection.QueryFirstOrDefaultAsync<RescheduleRequestRead>(
                sql,
                new { RequestId = requestId });
        }
        public async Task<bool> UpdateRescheduleRequestStatusAsync(
    int requestId,
    string status,
    string modifiedBy)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new Exception("Status is required.");

            status = status.Trim();

            if (status != "Accepted" && status != "Rejected")
                throw new Exception(
                    "Status must be either Accepted or Rejected.");

            // ============================================================
            // Get request first
            // ============================================================

            var request = await GetRescheduleRequestByIdAsync(requestId);

            if (request == null)
                throw new Exception("Reschedule request not found.");

            if (request.Status != "Pending")
                throw new Exception(
                    "Only pending reschedule requests can be accepted or rejected.");

            // ============================================================
            // REJECT
            // ============================================================

            if (status == "Rejected")
            {
                using var connection = _db.GetConnection();

                var rows = await connection.ExecuteAsync(@"
            UPDATE reschedulerequests
            SET
                Status = 'Rejected',
                ModifiedAt = NOW(),
                ModifiedBy = @ModifiedBy
            WHERE RequestId = @RequestId
            AND Status = 'Pending'
        ",
                new
                {
                    RequestId = requestId,
                    ModifiedBy = modifiedBy
                });

                return rows > 0;
            }

            // ============================================================
            // ACCEPT
            // ============================================================

            var newBookingId = await RescheduleAsync(
                request.BookingId,
                request.RequestedDate,
                request.RequestedStartTime,
                request.RequestedEndTime,
                request.SlotId,
                modifiedBy,
                request.Reason ?? "Client reschedule request");

            // ============================================================
            // Update request after successful reschedule
            // ============================================================

            using var connection2 = _db.GetConnection();

            var updated = await connection2.ExecuteAsync(@"
        UPDATE reschedulerequests
        SET
            Status = 'Accepted',
            ModifiedAt = NOW(),
            ModifiedBy = @ModifiedBy
        WHERE RequestId = @RequestId
        AND Status = 'Pending'
    ",
            new
            {
                RequestId = requestId,
                ModifiedBy = modifiedBy
            });

            return updated > 0;
        }
    }
}