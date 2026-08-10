using ahello_backend.DbContexts;
using ahello_backend.Models.Admin;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class AdminDashboardRepository : IAdminDashboardRepository
    {
        private readonly DbContext _db;

        public AdminDashboardRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<AdminDashboardModel> GetAdminDashboardAsync()
        {
            using var connection = _db.GetConnection();

            var dashboard = new AdminDashboardModel();

            // =========================================================
            // 1. USERS, SERVICES, CATEGORIES, SERVICE TYPES
            // =========================================================

            const string basicCountsSql = @"
                SELECT
                    (SELECT COUNT(*) FROM users) AS TotalUsers,
                    (SELECT COUNT(*) FROM services) AS TotalServices,
                    (SELECT COUNT(*) FROM categories) AS TotalCategories,
                    (SELECT COUNT(*) FROM servicetypes) AS TotalServiceTypes;
            ";

            var basicCounts = await connection.QueryFirstOrDefaultAsync<AdminDashboardModel>(
                basicCountsSql);

            if (basicCounts != null)
            {
                dashboard.TotalUsers = basicCounts.TotalUsers;
                dashboard.TotalServices = basicCounts.TotalServices;
                dashboard.TotalCategories = basicCounts.TotalCategories;
                dashboard.TotalServiceTypes = basicCounts.TotalServiceTypes;
            }


            // =========================================================
            // 2. BOOKING COUNTS
            // =========================================================

            const string bookingCountsSql = @"
                SELECT
                    COUNT(*) AS TotalBookings,

                    SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END)
                        AS PendingBookings,

                    SUM(CASE WHEN Status = 'Confirmed' THEN 1 ELSE 0 END)
                        AS ConfirmedBookings,

                    SUM(CASE WHEN Status = 'NoShow' THEN 1 ELSE 0 END)
                        AS NoShowBookings,

                    SUM(CASE WHEN Status = 'Cancelled' THEN 1 ELSE 0 END)
                        AS CancelledBookings,

                    SUM(CASE WHEN Status = 'Rejected' THEN 1 ELSE 0 END)
                        AS RejectedBookings,

                    SUM(CASE WHEN Status = 'Rescheduled' THEN 1 ELSE 0 END)
                        AS RescheduledBookings

                FROM bookings;
            ";

            var bookingCounts = await connection.QueryFirstOrDefaultAsync<AdminDashboardModel>(
                bookingCountsSql);

            if (bookingCounts != null)
            {
                dashboard.TotalBookings = bookingCounts.TotalBookings;
                dashboard.PendingBookings = bookingCounts.PendingBookings;
                dashboard.ConfirmedBookings = bookingCounts.ConfirmedBookings;
                dashboard.NoShowBookings = bookingCounts.NoShowBookings;
                dashboard.CancelledBookings = bookingCounts.CancelledBookings;
                dashboard.RejectedBookings = bookingCounts.RejectedBookings;
                dashboard.RescheduledBookings = bookingCounts.RescheduledBookings;
            }


            // =========================================================
            // 3. LATEST 5 BOOKINGS
            // =========================================================

            const string latestBookingsSql = @"
                SELECT
                    b.BookingId AS BookingId,
                    COALESCE(u.FullName, '') AS Customer,
                    COALESCE(s.ServiceTitle, '') AS Service,

                    DATE_FORMAT(
                        b.ScheduleDate,
                        '%d %b %Y'
                    ) AS ScheduleDate,

                    DATE_FORMAT(
                        b.StartTime,
                        '%h:%i %p'
                    ) AS StartTime,

                    DATE_FORMAT(
                        b.EndTime,
                        '%h:%i %p'
                    ) AS EndTime,

                    b.Status AS Status

                FROM bookings b

                LEFT JOIN users u
                    ON b.ClientId = u.UserId

                LEFT JOIN services s
                    ON b.ServiceId = s.ServiceId

                ORDER BY b.CreatedAt DESC

                LIMIT 5;
            ";

            dashboard.LatestBookings =
                (await connection.QueryAsync<AdminDashboardBookingModel>(
                    latestBookingsSql)).ToList();


            // =========================================================
            // 4. MEETING COUNTS
            // =========================================================

            const string meetingCountsSql = @"
                SELECT
                    COUNT(*) AS TotalMeetings,

                    SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END)
                        AS PendingMeetings,

                    SUM(CASE WHEN Status = 'Missed' THEN 1 ELSE 0 END)
                        AS MissedMeetings,

                    SUM(CASE WHEN Status = 'Rescheduled' THEN 1 ELSE 0 END)
                        AS RescheduledMeetings,

                    SUM(CASE WHEN Status = 'Cancelled' THEN 1 ELSE 0 END)
                        AS CancelledMeetings,

                    SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END)
                        AS CompletedMeetings

                FROM meetings;
            ";

            var meetingCounts = await connection.QueryFirstOrDefaultAsync<AdminDashboardModel>(
                meetingCountsSql);

            if (meetingCounts != null)
            {
                dashboard.TotalMeetings = meetingCounts.TotalMeetings;
                dashboard.PendingMeetings = meetingCounts.PendingMeetings;
                dashboard.MissedMeetings = meetingCounts.MissedMeetings;
                dashboard.RescheduledMeetings = meetingCounts.RescheduledMeetings;
                dashboard.CancelledMeetings = meetingCounts.CancelledMeetings;
                dashboard.CompletedMeetings = meetingCounts.CompletedMeetings;
            }


            // =========================================================
            // 5. REVIEWS
            // =========================================================

            const string reviewSql = @"
                SELECT
                    COUNT(*) AS TotalReviews,
                    COALESCE(AVG(CAST(Rating AS DECIMAL(10,2))), 0) AS AverageRating
                FROM reviews;
            ";

            var review = await connection.QueryFirstOrDefaultAsync<AdminDashboardModel>(
                reviewSql);

            if (review != null)
            {
                dashboard.TotalReviews = review.TotalReviews;
                dashboard.AverageRating = Math.Round(review.AverageRating, 1);
            }


            // =========================================================
            // 6. INVOICE COUNTS
            // =========================================================

            const string invoiceCountsSql = @"
                SELECT
                    COUNT(*) AS TotalInvoices,

                    SUM(
                        CASE
                            WHEN Status = 'Issued'
                            THEN 1
                            ELSE 0
                        END
                    ) AS IssuedInvoices,

                    SUM(
                        CASE
                            WHEN Status = 'Cancelled'
                            THEN 1
                            ELSE 0
                        END
                    ) AS CancelledInvoices

                FROM invoices;
            ";

            var invoiceCounts =
                await connection.QueryFirstOrDefaultAsync<AdminDashboardModel>(
                    invoiceCountsSql);

            if (invoiceCounts != null)
            {
                dashboard.TotalInvoices = invoiceCounts.TotalInvoices;
                dashboard.IssuedInvoices = invoiceCounts.IssuedInvoices;
                dashboard.CancelledInvoices = invoiceCounts.CancelledInvoices;
            }


            // =========================================================
            // 7. LATEST 5 INVOICES
            // =========================================================

            const string latestInvoicesSql = @"
                SELECT
                    i.InvoiceId AS InvoiceId,
                    i.InvoiceNumber AS InvoiceNumber,
                    COALESCE(u.FullName, '') AS Customer,
                    i.TotalAmount AS Total,

                    DATE_FORMAT(
                        i.IssuedAt,
                        '%d %b %Y'
                    ) AS Date

                FROM invoices i

                LEFT JOIN users u
                    ON i.ClientId = u.UserId

                ORDER BY i.IssuedAt DESC

                LIMIT 5;
            ";

            dashboard.LatestInvoices =
                (await connection.QueryAsync<AdminDashboardInvoiceModel>(
                    latestInvoicesSql)).ToList();


            // =========================================================
            // 8. PAYMENT COUNTS
            // =========================================================

            const string paymentCountsSql = @"
                SELECT
                    COUNT(*) AS TotalPayments,

                    SUM(
                        CASE
                            WHEN Status = 'CREATED'
                            THEN 1
                            ELSE 0
                        END
                    ) AS CreatedPayments,

                    SUM(
                        CASE
                            WHEN Status = 'PAYMENT_VERIFIED'
                            THEN 1
                            ELSE 0
                        END
                    ) AS PaymentVerified,

                    SUM(
                        CASE
                            WHEN Status = 'PAYMENT_FAILED'
                            THEN 1
                            ELSE 0
                        END
                    ) AS PaymentFailed,

                    SUM(
                        CASE
                            WHEN Status = 'HELD'
                            THEN 1
                            ELSE 0
                        END
                    ) AS HeldPayments,

                    SUM(
                        CASE
                            WHEN Status = 'RELEASED'
                            THEN 1
                            ELSE 0
                        END
                    ) AS ReleasedPayments,

                    SUM(
                        CASE
                            WHEN Status = 'REFUNDED'
                            THEN 1
                            ELSE 0
                        END
                    ) AS RefundedPayments,

                    SUM(
                        CASE
                            WHEN Status = 'CANCELLED'
                            THEN 1
                            ELSE 0
                        END
                    ) AS CancelledPayments,

                    SUM(
                        CASE
                            WHEN Status = 'ERROR'
                            THEN 1
                            ELSE 0
                        END
                    ) AS ErrorPayments

                FROM escrowpayments;
            ";

            var paymentCounts =
                await connection.QueryFirstOrDefaultAsync<AdminDashboardModel>(
                    paymentCountsSql);

            if (paymentCounts != null)
            {
                dashboard.TotalPayments = paymentCounts.TotalPayments;
                dashboard.CreatedPayments = paymentCounts.CreatedPayments;
                dashboard.PaymentVerified = paymentCounts.PaymentVerified;
                dashboard.PaymentFailed = paymentCounts.PaymentFailed;
                dashboard.HeldPayments = paymentCounts.HeldPayments;
                dashboard.ReleasedPayments = paymentCounts.ReleasedPayments;
                dashboard.RefundedPayments = paymentCounts.RefundedPayments;
                dashboard.CancelledPayments = paymentCounts.CancelledPayments;
                dashboard.ErrorPayments = paymentCounts.ErrorPayments;
            }


            // =========================================================
            // 9. LATEST 5 PAYMENTS
            // =========================================================

            const string latestPaymentsSql = @"
                SELECT
                    ep.EscrowPaymentId AS EscrowPaymentId,
                    ep.BookingId AS BookingId,
                    COALESCE(u.FullName, '') AS Customer,

                    ep.TotalAmount AS TotalAmount,
                    ep.PlatformFee AS PlatformFee,
                    ep.ExpertAmount AS ExpertAmount,

                    ep.Status AS Status,

                    DATE_FORMAT(
                        ep.CreatedAt,
                        '%d %b %Y'
                    ) AS Date

                FROM escrowpayments ep

                LEFT JOIN users u
                    ON ep.ClientId = u.UserId

                ORDER BY ep.CreatedAt DESC

                LIMIT 5;
            ";

            dashboard.LatestPayments =
                (await connection.QueryAsync<AdminDashboardPaymentModel>(
                    latestPaymentsSql)).ToList();


            // =========================================================
            // 10. REVENUE
            // =========================================================

            const string revenueSql = @"
                SELECT

                    -- Successful payment revenue
                    COALESCE(
                        SUM(
                            CASE
                                WHEN Status IN
                                (
                                    'PAYMENT_VERIFIED',
                                    'HELD',
                                    'RELEASED'
                                )
                                THEN TotalAmount
                                ELSE 0
                            END
                        ),
                        0
                    ) AS TotalRevenue,

                    COALESCE(
                        SUM(
                            CASE
                                WHEN Status IN
                                (
                                    'PAYMENT_VERIFIED',
                                    'HELD',
                                    'RELEASED'
                                )
                                THEN PlatformFee
                                ELSE 0
                            END
                        ),
                        0
                    ) AS TotalPlatformFee,

                    COALESCE(
                        SUM(
                            CASE
                                WHEN Status IN
                                (
                                    'PAYMENT_VERIFIED',
                                    'HELD',
                                    'RELEASED'
                                )
                                THEN TaxAmount
                                ELSE 0
                            END
                        ),
                        0
                    ) AS TotalTaxAmount,

                    COALESCE(
                        SUM(
                            CASE
                                WHEN Status IN
                                (
                                    'PAYMENT_VERIFIED',
                                    'HELD',
                                    'RELEASED'
                                )
                                THEN ExpertAmount
                                ELSE 0
                            END
                        ),
                        0
                    ) AS TotalExpertAmount,

                    -- Released expert amount
                    COALESCE(
                        SUM(
                            CASE
                                WHEN Status = 'RELEASED'
                                THEN ExpertAmount
                                ELSE 0
                            END
                        ),
                        0
                    ) AS ReleasedAmount,

                    -- Amount still in payment/escrow flow
                    COALESCE(
                        SUM(
                            CASE
                                WHEN Status IN
                                (
                                    'PAYMENT_VERIFIED',
                                    'HELD'
                                )
                                THEN ExpertAmount
                                ELSE 0
                            END
                        ),
                        0
                    ) AS PendingAmount,

                    -- Refunded amount
                    COALESCE(
                        SUM(
                            CASE
                                WHEN Status = 'REFUNDED'
                                THEN TotalAmount
                                ELSE 0
                            END
                        ),
                        0
                    ) AS RefundedAmount

                FROM escrowpayments;
            ";

            var revenue =
                await connection.QueryFirstOrDefaultAsync<AdminDashboardModel>(
                    revenueSql);

            if (revenue != null)
            {
                dashboard.TotalRevenue = revenue.TotalRevenue;
                dashboard.TotalPlatformFee = revenue.TotalPlatformFee;
                dashboard.TotalTaxAmount = revenue.TotalTaxAmount;
                dashboard.TotalExpertAmount = revenue.TotalExpertAmount;
                dashboard.ReleasedAmount = revenue.ReleasedAmount;
                dashboard.PendingAmount = revenue.PendingAmount;
                dashboard.RefundedAmount = revenue.RefundedAmount;
            }


            // =========================================================
            // 11. RESCHEDULES, BLOCK DATES AND SLOTS
            // =========================================================

            const string otherCountsSql = @"
                SELECT
                    (SELECT COUNT(*) FROM reschedules)
                        AS TotalReschedules,

                    (SELECT COUNT(*) FROM blockdates)
                        AS TotalBlockDates,

                    (SELECT COUNT(*) FROM userslots)
                        AS TotalUserSlots,

                    (SELECT COUNT(*) FROM bookedslots)
                        AS TotalBookedSlots;
            ";

            var otherCounts =
                await connection.QueryFirstOrDefaultAsync<AdminDashboardModel>(
                    otherCountsSql);

            if (otherCounts != null)
            {
                dashboard.TotalReschedules = otherCounts.TotalReschedules;
                dashboard.TotalBlockDates = otherCounts.TotalBlockDates;
                dashboard.TotalUserSlots = otherCounts.TotalUserSlots;
                dashboard.TotalBookedSlots = otherCounts.TotalBookedSlots;
            }


            return dashboard;
        }
    }
}
