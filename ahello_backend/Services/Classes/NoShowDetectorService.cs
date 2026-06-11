using ahello_backend.DbContexts;
using ahello_backend.Models.UserSlots;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;
using Dapper;

namespace ahello_backend.Services.Classes
{
    public class NoShowDetectorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NoShowDetectorService> _logger;

        public NoShowDetectorService(
            IServiceScopeFactory scopeFactory,
            ILogger<NoShowDetectorService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[NoShowDetector] Background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DetectAndHandleNoShows();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[NoShowDetector] Unhandled error in detection cycle.");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task DetectAndHandleNoShows()
        {
            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<DbContext>();
            var bookingRepo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            var slotService = scope.ServiceProvider.GetRequiredService<ISlotService>();

            using var connection = db.GetConnection();

            var candidates = await connection.QueryAsync<NoShowCandidate>(@"
    SELECT
        b.BookingId,
        b.UserId,
        b.ClientId,
        b.ServiceId,
        b.ScheduleDate,
        b.StartTime,
        b.EndTime,
        s.AutoReschedule,
        cu.FullName AS ClientName,
        cu.Email AS ClientEmail,
        s.ServiceTitle
    FROM bookings b
    INNER JOIN services s ON b.ServiceId = s.ServiceId
    INNER JOIN users provider ON b.UserId = provider.UserId
    INNER JOIN users cu ON b.ClientId = cu.UserId
    WHERE b.Status = 'Pending'
      AND s.AutoReschedule = 1
      AND CONVERT_TZ(b.CreatedAt, '+05:30', '+00:00') >= DATE_SUB(NOW(), INTERVAL 1 DAY)
      AND TIMESTAMPADD(
      MINUTE,
      5,
      CONVERT_TZ(TIMESTAMP(b.ScheduleDate, b.EndTime), '+05:30', '+00:00')
    ) < NOW()");

            foreach (var booking in candidates)
            {
                try
                {
                    await ProcessNoShow(
                        booking,
                        connection,
                        bookingRepo,
                        slotService);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "[NoShowDetector] Failed to process BookingId:{BookingId}",
                        booking.BookingId);
                }
            }
        }

        private async Task ProcessNoShow(
            NoShowCandidate booking,
            System.Data.IDbConnection connection,
            IBookingRepository bookingRepo,
            ISlotService slotService)
        {
            // Atomic update prevents duplicate processing
            var affected = await connection.ExecuteAsync(@"
                UPDATE bookings
                SET Status = 'NoShow',
                    ModifiedAt = NOW()
                WHERE BookingId = @BookingId
                  AND Status = 'Pending'",
                new { booking.BookingId });

            if (affected == 0)
            {
                _logger.LogWarning(
                    "[NoShowDetector] BookingId:{BookingId} already processed. Skipping.",
                    booking.BookingId);
                return;
            }

            await connection.ExecuteAsync(@"
                UPDATE meetings
                SET Status = 'Missed',
                    ModifiedAt = NOW()
                WHERE BookingId = @BookingId",
                new { booking.BookingId });

            _logger.LogInformation(
                "[NoShowDetector] BookingId:{BookingId} marked as NoShow.",
                booking.BookingId);

            if (booking.AutoReschedule)
            {
                await AutoReschedule(
                    booking,
                    bookingRepo,
                    slotService);
            }
        }

        private async Task AutoReschedule(
            NoShowCandidate booking,
            IBookingRepository bookingRepo,
            ISlotService slotService)
        {
            var duration = await slotService.GetServiceDurationAsync(
                booking.UserId,
                booking.ServiceId);

            // Only check today's remaining slots
            var tryDate = DateTime.UtcNow.AddHours(5).AddMinutes(30).Date;

            var slots = await slotService.GetDaySlots(
                booking.UserId,
                booking.ServiceId,
                duration,
                tryDate);

            if (slots == null || !slots.Any())
            {
                _logger.LogWarning(
                    "[NoShowDetector] No slots available today for BookingId:{BookingId}.",
                    booking.BookingId);
                return;
            }

            foreach (var slot in slots)
            {
                var slotStartDateTime = slot.SlotDate.Date.Add(slot.StartTime);

                // Skip slots that have already passed
                var istNow = DateTime.UtcNow.AddHours(5).AddMinutes(30);
                if (slotStartDateTime <= istNow)
                    continue;

                try
                {
                    var newBookingId = await bookingRepo.RescheduleAsync(
                        oldBookingId: booking.BookingId,
                        newDate: slot.SlotDate,
                        newStart: slot.StartTime,
                        newEnd: slot.EndTime,
                        slotId: slot.SlotId,
                        rescheduledBy: "System",
                        reason: "NoShow");

                    _logger.LogInformation(
                        "[NoShowDetector] BookingId:{OldId} auto-rescheduled to NewBookingId:{NewId}",
                        booking.BookingId,
                        newBookingId);

                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "[NoShowDetector] Failed slot {SlotId} for BookingId:{BookingId}. Trying next slot.",
                        slot.SlotId,
                        booking.BookingId);
                }
            }

            _logger.LogWarning(
                "[NoShowDetector] No available slot found today for BookingId:{BookingId}.",
                booking.BookingId);
        }
    }

    internal class NoShowCandidate
    {
        public int BookingId { get; set; }

        public int UserId { get; set; }

        public int ClientId { get; set; }

        public int ServiceId { get; set; }

        public DateTime ScheduleDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public bool AutoReschedule { get; set; }

        public string? ClientName { get; set; }

        public string? ClientEmail { get; set; }

        public string? ServiceTitle { get; set; }
    }
}