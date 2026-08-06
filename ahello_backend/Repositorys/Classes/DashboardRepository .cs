using ahello_backend.DbContexts;
using ahello_backend.Models;
using ahello_backend.Repositorys.Interfaces;
using System.Data;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly DbContext _db;

        public DashboardRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<DashboardGet> GetByUserIdAsync(int userId)
        {
            using var connection = _db.GetConnection();

            var statistics = await GetStatisticsAsync(connection, userId);
            var services = await GetServicesAsync(connection, userId);
            var recentBookings = await GetRecentBookingsAsync(connection, userId);

            return new DashboardGet
            {
                UserId = userId,
                Statistics = statistics,
                Services = services,
                RecentBookings = recentBookings
            };
        }

        // ======================================================
        // STATISTICS (single round-trip using correlated subqueries)
        // ======================================================
        private async Task<DashboardStatistics> GetStatisticsAsync(IDbConnection connection, int userId)
        {
            var sql = @"
        SELECT
            (SELECT COUNT(*) FROM services WHERE UserId = @UserId) AS TotalServices,

            (SELECT COUNT(*) FROM bookings WHERE UserId = @UserId) AS TotalBookings,
            (SELECT COUNT(*) FROM bookings WHERE UserId = @UserId AND Status = 'Confirmed') AS ConfirmedBookings,
            (SELECT COUNT(*) FROM bookings WHERE UserId = @UserId AND Status = 'Pending') AS PendingBookings,
            (SELECT COUNT(*) FROM bookings WHERE UserId = @UserId AND Status = 'Completed') AS CompletedBookings,
            (SELECT COUNT(*) FROM bookings WHERE UserId = @UserId AND Status = 'Cancelled') AS CancelledBookings,
            (SELECT COUNT(*) FROM bookings WHERE UserId = @UserId AND Status = 'Rejected') AS RejectedBookings,
            (SELECT COUNT(*) FROM bookings WHERE UserId = @UserId AND Status = 'Rescheduled') AS RescheduledBookings,
            (SELECT COUNT(*) FROM bookings WHERE UserId = @UserId AND Status = 'NoShow') AS NoShowBookings,

            (SELECT COUNT(*)
             FROM invoices i
             INNER JOIN bookings b ON i.BookingId = b.BookingId
             WHERE b.UserId = @UserId) AS TotalInvoices,

            (SELECT COUNT(*)
             FROM meetings m
             INNER JOIN bookings b ON m.BookingId = b.BookingId
             WHERE b.UserId = @UserId) AS TotalMeetings,

            (SELECT COUNT(*)
             FROM meetings m
             INNER JOIN bookings b ON m.BookingId = b.BookingId
             WHERE b.UserId = @UserId
             AND m.Status = 'Pending'
             AND m.StartTime >= NOW()) AS UpcomingMeetings,

            (SELECT COUNT(*)
             FROM meetings m
             INNER JOIN bookings b ON m.BookingId = b.BookingId
             WHERE b.UserId = @UserId
             AND m.Status = 'Completed') AS CompletedMeetings,

            (SELECT COUNT(*)
             FROM meetings m
             INNER JOIN bookings b ON m.BookingId = b.BookingId
             WHERE b.UserId = @UserId
             AND m.Status = 'Cancelled') AS CancelledMeetings,

            (SELECT COALESCE(ROUND(AVG(r.Rating), 1), 0)
             FROM reviews r
             INNER JOIN bookings b ON r.BookingId = b.BookingId
             WHERE b.UserId = @UserId) AS AverageRating,

            (SELECT COUNT(*)
             FROM reviews r
             INNER JOIN bookings b ON r.BookingId = b.BookingId
             WHERE b.UserId = @UserId) AS TotalReviews,

            -- escrowpayments already has UserId directly, no bookings join needed
            (SELECT COALESCE(SUM(ep.ExpertAmount), 0)
             FROM escrowpayments ep
             WHERE ep.UserId = @UserId
             AND ep.Status IN ('HELD', 'RELEASED')) AS TotalRevenue,

            (SELECT COALESCE(SUM(ep.ExpertAmount), 0)
             FROM escrowpayments ep
             WHERE ep.UserId = @UserId
             AND ep.Status = 'RELEASED') AS ReleasedAmount,

            (SELECT COALESCE(SUM(ep.ExpertAmount), 0)
             FROM escrowpayments ep
             WHERE ep.UserId = @UserId
             AND ep.Status = 'HELD') AS PendingAmount;";

            var stats = await connection.QueryFirstOrDefaultAsync<DashboardStatistics>(
                sql,
                new { UserId = userId });

            return stats ?? new DashboardStatistics();
        }

        // ======================================================
        // PER-SERVICE BREAKDOWN
        // ======================================================
        private async Task<List<DashboardServiceGet>> GetServicesAsync(IDbConnection connection, int userId)
        {
            var sql = @"
        SELECT
            s.ServiceTitle AS ServiceName,
            s.Price,
            s.Status,

            COUNT(DISTINCT b.BookingId) AS TotalBookings,
            SUM(CASE WHEN b.Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedBookings,
            SUM(CASE WHEN b.Status = 'Pending' THEN 1 ELSE 0 END) AS PendingBookings,
            SUM(CASE WHEN b.Status = 'Cancelled' THEN 1 ELSE 0 END) AS CancelledBookings,

            COALESCE(ROUND(AVG(r.Rating), 1), 0) AS AverageRating,
            COUNT(DISTINCT r.ReviewId) AS TotalReviews,

            -- FIXED: revenue now comes from actual captured payments, not booking status
            COALESCE(SUM(CASE WHEN ep.Status IN ('HELD','RELEASED') THEN ep.ExpertAmount ELSE 0 END), 0) AS Revenue

        FROM services s
        LEFT JOIN bookings b ON b.ServiceId = s.ServiceId
        LEFT JOIN reviews r ON r.BookingId = b.BookingId
        LEFT JOIN escrowpayments ep ON ep.BookingId = b.BookingId

        WHERE s.UserId = @UserId

        GROUP BY s.ServiceId, s.ServiceTitle, s.Price, s.Status
        ORDER BY s.ServiceId DESC;";

            var rows = await connection.QueryAsync<DashboardServiceGet>(sql, new { UserId = userId });
            return rows.ToList();
        }

        // ======================================================
        // RECENT BOOKINGS (latest 10)
        // ======================================================
        private async Task<List<DashboardRecentBookingGet>> GetRecentBookingsAsync(IDbConnection connection, int userId)
        {
            var sql = @"
                SELECT
                    b.BookingId,
                    s.ServiceTitle AS ServiceName,
                    cu.FullName AS ClientName,
                    b.ScheduleDate,
                    b.StartTime,
                    b.Status

                FROM bookings b
                INNER JOIN services s ON b.ServiceId = s.ServiceId
                INNER JOIN users cu ON b.ClientId = cu.UserId

                WHERE b.UserId = @UserId

                ORDER BY b.ScheduleDate DESC, b.StartTime DESC, b.BookingId DESC
                LIMIT 10;";

            var rows = await connection.QueryAsync<DashboardRecentBookingGet>(sql, new { UserId = userId });
            return rows.ToList();
        }
    }
}
