using ahello_backend.DbContexts;
using ahello_backend.Models.Bookings;
using ahello_backend.Models.Meeting;
using ahello_backend.Models.Pagination;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

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
                var meetingStartTime = model.ScheduleDate.Add(model.StartTime);
                var meetingEndTime = model.ScheduleDate.Add(model.EndTime);

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

                string serviceName = service?.ServiceTitle ?? "the service";
                string formattedDate = model.ScheduleDate.ToString("dddd, MMMM dd yyyy");
                string formattedTime = meetingStartTime.ToString("hh:mm tt");

                await _emailRepository.SendBookingConfirmationEmailAsync(
                    client.Email, client.FullName, serviceName, formattedDate, formattedTime);

                await _emailRepository.SendMeetingInviteEmailAsync(
                    client.Email, client.FullName, meetingLink);

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