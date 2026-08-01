using ahello_backend.DbContexts;
using ahello_backend.Models.Invoices;
using ahello_backend.Repositorys.Interfaces;
using Dapper;
using System.Data;

namespace ahello_backend.Repositorys.Classes
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly DbContext _db;

        public InvoiceRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<bool> ExistsByBookingIdAsync(int bookingId)
        {
            const string sql = "SELECT COUNT(1) FROM invoices WHERE BookingId = @BookingId";
            using var connection = _db.GetConnection();
            var count = await connection.ExecuteScalarAsync<int>(sql, new { BookingId = bookingId });
            return count > 0;
        }

        public async Task<int> InsertInvoiceAsync(Invoice invoice)
        {
            const string sql = @"
                INSERT INTO invoices
                    (InvoiceNumber, BookingId, EscrowPaymentId, UserId, ClientId, ServiceId,
                     TotalAmount, PlatformFee, ExpertAmount, Currency, Status,
                     IssuedAt, CreatedAt, CreatedBy)
                VALUES
                    (@InvoiceNumber, @BookingId, @EscrowPaymentId, @UserId, @ClientId, @ServiceId,
                     @TotalAmount, @PlatformFee, @ExpertAmount, @Currency, @Status,
                     @IssuedAt, @CreatedAt, @CreatedBy);
                SELECT LAST_INSERT_ID();";

            var parameters = new DynamicParameters();
            parameters.Add("InvoiceNumber", invoice.InvoiceNumber, DbType.String);
            parameters.Add("BookingId", invoice.BookingId, DbType.Int32);
            parameters.Add("EscrowPaymentId", invoice.EscrowPaymentId, DbType.Int32);
            parameters.Add("UserId", invoice.UserId, DbType.Int32);
            parameters.Add("ClientId", invoice.ClientId, DbType.Int32);
            parameters.Add("ServiceId", invoice.ServiceId, DbType.Int32);
            parameters.Add("TotalAmount", invoice.TotalAmount, DbType.Decimal);
            parameters.Add("PlatformFee", invoice.PlatformFee, DbType.Decimal);
            parameters.Add("ExpertAmount", invoice.ExpertAmount, DbType.Decimal);
            parameters.Add("Currency", invoice.Currency, DbType.String);
            parameters.Add("Status", invoice.Status ?? "Issued", DbType.String);
            parameters.Add("IssuedAt", invoice.IssuedAt, DbType.DateTime);
            parameters.Add("CreatedAt", invoice.CreatedAt, DbType.DateTime);
            parameters.Add("CreatedBy", (object)invoice.CreatedBy ?? DBNull.Value, DbType.String);

            using var connection = _db.GetConnection();
            var newId = await connection.ExecuteScalarAsync<int>(sql, parameters);
            return newId;
        }

        // Shared join used by all 3 GET endpoints
        private const string BaseSelectSql = @"
            SELECT
                i.InvoiceId, i.InvoiceNumber, i.IssuedAt, i.Status,
                b.BookingId, b.ScheduleDate, b.StartTime, b.EndTime, b.Status AS BookingStatus,
                eu.UserId, eu.FullName AS ExpertName, eu.Email AS ExpertEmail, eu.MobileNumber AS ExpertMobile,
                cu.UserId AS ClientId, cu.FullName AS ClientName, cu.Email AS ClientEmail, cu.MobileNumber AS ClientMobile,
                s.ServiceId, s.ServiceTitle, s.Duration,
                ep.EscrowPaymentId, ep.RazorpayOrderId, ep.RazorpayPaymentId,
                i.TotalAmount, i.PlatformFee, i.ExpertAmount, i.Currency
            FROM invoices i
            INNER JOIN bookings b        ON b.BookingId = i.BookingId
            INNER JOIN users eu          ON eu.UserId = i.UserId
            INNER JOIN users cu          ON cu.UserId = i.ClientId
            INNER JOIN services s        ON s.ServiceId = i.ServiceId
            INNER JOIN EscrowPayments ep ON ep.EscrowPaymentId = i.EscrowPaymentId";

        public async Task<IEnumerable<InvoiceResponseDto>> GetAllInvoicesAsync()
        {
            var sql = BaseSelectSql + " ORDER BY i.IssuedAt DESC";
            using var connection = _db.GetConnection();
            return await connection.QueryAsync<InvoiceResponseDto>(sql);
        }

        public async Task<InvoiceResponseDto?> GetInvoiceByBookingIdAsync(int bookingId)
        {
            var sql = BaseSelectSql + " WHERE i.BookingId = @BookingId";
            using var connection = _db.GetConnection();
            return await connection.QuerySingleOrDefaultAsync<InvoiceResponseDto>(sql, new { BookingId = bookingId });
        }

        // NOTE: filters by UserId = expert. If you also need "invoices where this user
        // was the client", add a separate method / a @Role param.
        public async Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByUserIdAsync(int userId)
        {
            var sql = BaseSelectSql + " WHERE i.UserId = @UserId ORDER BY i.IssuedAt DESC";
            using var connection = _db.GetConnection();
            return await connection.QueryAsync<InvoiceResponseDto>(sql, new { UserId = userId });
        }
        public async Task<InvoicePdfDto?> GetInvoicePdfAsync(int bookingId)
        {
            const string sql = @"
                SELECT
                    i.InvoiceNumber,
                    i.IssuedAt,

                    b.ScheduleDate,
                    b.StartTime,

                    eu.FullName AS ExpertName,
                    eu.Email AS ExpertEmail,
                    eu.MobileNumber AS ExpertMobile,

                    cu.FullName AS ClientName,
                    cu.Email AS ClientEmail,
                    cu.MobileNumber AS ClientMobile,

                    s.ServiceTitle,

                    i.TotalAmount,
                    i.PlatformFee,
                    i.ExpertAmount

                FROM invoices i
                INNER JOIN bookings b
                    ON b.BookingId = i.BookingId

                INNER JOIN users eu
                    ON eu.UserId = i.UserId

                INNER JOIN users cu
                    ON cu.UserId = i.ClientId

                INNER JOIN services s
                    ON s.ServiceId = i.ServiceId

                WHERE i.BookingId = @BookingId;
                ";

            using var connection = _db.GetConnection();

            return await connection.QuerySingleOrDefaultAsync<InvoicePdfDto>(
                sql,
                new { BookingId = bookingId });
        }
    }
}