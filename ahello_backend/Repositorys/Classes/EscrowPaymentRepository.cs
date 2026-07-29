using ahello_backend.DbContexts;
using ahello_backend.Models.Payment;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class EscrowPaymentRepository : IEscrowPaymentRepository
    {
        private readonly DbContext _db;

        public EscrowPaymentRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<int> InsertAsync(EscrowPayment payment)
        {
            var query = @"
        INSERT INTO EscrowPayments
        (
            BookingId,
            UserId,
            ClientId,
            RazorpayAccountId,
            RazorpayOrderId,
            RazorpayPaymentId,
            RazorpaySignature,
            RazorpayTransferId,
            TotalAmount,
            PlatformFee,
            ExpertAmount,
            Currency,
            Status,
            OrderResponseJson,
            VerifyResponseJson,
            TransferResponseJson,
            PaidAt,
            HeldAt,
            CreatedAt,
            CreatedBy
        )
        VALUES
        (
            @BookingId,
            @UserId,
            @ClientId,
            @RazorpayAccountId,
            @RazorpayOrderId,
            @RazorpayPaymentId,
            @RazorpaySignature,
            @RazorpayTransferId,
            @TotalAmount,
            @PlatformFee,
            @ExpertAmount,
            @Currency,
            @Status,
            @OrderResponseJson,
            @VerifyResponseJson,
            @TransferResponseJson,
            @PaidAt,
            @HeldAt,
            NOW(),
            @CreatedBy
        );

        SELECT LAST_INSERT_ID();";

            using var connection = _db.GetConnection();

            return await connection.ExecuteScalarAsync<int>(query, payment);
        }

        public async Task<EscrowPayment?> GetByBookingIdAsync(int bookingId)
        {
            var query = @"
                SELECT *
                FROM EscrowPayments
                WHERE BookingId = @BookingId
                LIMIT 1";

            using var connection = _db.GetConnection();

            return await connection.QuerySingleOrDefaultAsync<EscrowPayment>(
                query,
                new { BookingId = bookingId });
        }


        public async Task<dynamic?> GetBookingPaymentInfoAsync(int bookingId)
        {
            var query = @"
        SELECT
            b.BookingId,
            b.UserId,
            b.ClientId,
            b.ServiceId,
            s.Price
        FROM bookings b
        INNER JOIN services s
            ON s.ServiceId = b.ServiceId
        WHERE b.BookingId = @BookingId
        LIMIT 1";

            using var connection = _db.GetConnection();

            return await connection.QuerySingleOrDefaultAsync<dynamic>(
                query,
                new { BookingId = bookingId });
        }

        public async Task<string?> GetServiceNameAsync(int serviceId)
        {
            var query = @"
        SELECT ServiceTitle
        FROM services
        WHERE ServiceId = @ServiceId
        LIMIT 1";

            using var connection = _db.GetConnection();

            return await connection.QuerySingleOrDefaultAsync<string?>(
                query,
                new { ServiceId = serviceId });
        }

        public async Task UpdateAfterPaymentAsync(
     int escrowPaymentId,
     string paymentId,
     string signature,
     string transferId,
     string transferJson)
        {
            var query = @"
        UPDATE EscrowPayments
        SET
            RazorpayPaymentId = @PaymentId,
            RazorpaySignature = @Signature,
            RazorpayTransferId = @TransferId,
            TransferResponseJson = @TransferJson,
            Status = 'HELD',
            PaidAt = NOW(),
            HeldAt = NOW(),
            ModifiedAt = NOW()
        WHERE EscrowPaymentId = @EscrowPaymentId";

            using var connection = _db.GetConnection();

            await connection.ExecuteAsync(query, new
            {
                EscrowPaymentId = escrowPaymentId,
                PaymentId = paymentId,
                Signature = signature,
                TransferId = transferId,
                TransferJson = transferJson
            });

            Console.WriteLine($"[InvoiceDebug] UpdateAfterPaymentAsync reached invoice block for EscrowPaymentId={escrowPaymentId}");

            try
            {
                var alreadyExists = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(1) FROM invoices WHERE EscrowPaymentId = @EscrowPaymentId",
                    new { EscrowPaymentId = escrowPaymentId }) > 0;

                Console.WriteLine($"[InvoiceDebug] alreadyExists = {alreadyExists}");

                if (!alreadyExists)
                {
                    var data = await connection.QuerySingleOrDefaultAsync<dynamic>(@"
                SELECT
                    ep.EscrowPaymentId, ep.BookingId, ep.UserId, ep.ClientId,
                    ep.TotalAmount, ep.PlatformFee, ep.ExpertAmount, ep.Currency,
                    b.ServiceId
                FROM EscrowPayments ep
                INNER JOIN bookings b ON b.BookingId = ep.BookingId
                WHERE ep.EscrowPaymentId = @EscrowPaymentId",
                        new { EscrowPaymentId = escrowPaymentId });

                    Console.WriteLine($"[InvoiceDebug] data fetched: {(data == null ? "NULL" : "OK")}");

                    if (data != null)
                    {
                        var invoiceNumber = $"INV-{(int)data.BookingId:D6}";

                        var rows = await connection.ExecuteAsync(@"
                    INSERT INTO invoices
                        (InvoiceNumber, BookingId, EscrowPaymentId, UserId, ClientId, ServiceId,
                         TotalAmount, PlatformFee, ExpertAmount, Currency, Status,
                         IssuedAt, CreatedAt, CreatedBy)
                    VALUES
                        (@InvoiceNumber, @BookingId, @EscrowPaymentId, @UserId, @ClientId, @ServiceId,
                         @TotalAmount, @PlatformFee, @ExpertAmount, @Currency, 'Issued',
                         NOW(), NOW(), 'System')",
                            new
                            {
                                InvoiceNumber = invoiceNumber,
                                BookingId = (int)data.BookingId,
                                EscrowPaymentId = escrowPaymentId,
                                UserId = (int)data.UserId,
                                ClientId = (int)data.ClientId,
                                ServiceId = (int)data.ServiceId,
                                TotalAmount = (decimal)data.TotalAmount,
                                PlatformFee = (decimal)data.PlatformFee,
                                ExpertAmount = (decimal)data.ExpertAmount,
                                Currency = (string)data.Currency
                            });

                        Console.WriteLine($"[InvoiceDebug] Insert rows affected: {rows}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InvoiceDebug] ERROR: {ex.GetType().Name} - {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        public async Task UpdateReleaseAsync(
            int escrowPaymentId,
            string releaseJson)
        {
            var query = @"
                UPDATE EscrowPayments
                SET
                    Status = 'RELEASED',
                    ReleaseResponseJson = @ReleaseJson,
                    ReleasedAt = NOW(),
                    ModifiedAt = NOW()
                WHERE EscrowPaymentId = @EscrowPaymentId";

            using var connection = _db.GetConnection();

            await connection.ExecuteAsync(query, new
            {
                EscrowPaymentId = escrowPaymentId,
                ReleaseJson = releaseJson
            });
        }

        public async Task UpdateRefundAsync(
            int escrowPaymentId,
            string refundJson)
        {
            var query = @"
                UPDATE EscrowPayments
                SET
                    Status = 'REFUNDED',
                    RefundResponseJson = @RefundJson,
                    RefundedAt = NOW(),
                    ModifiedAt = NOW()
                WHERE EscrowPaymentId = @EscrowPaymentId";

            using var connection = _db.GetConnection();

            await connection.ExecuteAsync(query, new
            {
                EscrowPaymentId = escrowPaymentId,
                RefundJson = refundJson
            });
        }

        public async Task UpdateStatusAsync(
            int escrowPaymentId,
            string status,
            string? failureReason = null)
        {
            var query = @"
                UPDATE EscrowPayments
                SET
                    Status = @Status,
                    FailureReason = @FailureReason,
                    ModifiedAt = NOW()
                WHERE EscrowPaymentId = @EscrowPaymentId";

            using var connection = _db.GetConnection();

            await connection.ExecuteAsync(query, new
            {
                EscrowPaymentId = escrowPaymentId,
                Status = status,
                FailureReason = failureReason
            });
        }
        public async Task UpdatePaymentVerifiedAsync(
    int escrowPaymentId,
    string paymentId,
    string signature,
    string verifyResponseJson)
        {
            var query = @"
        UPDATE EscrowPayments
        SET
            RazorpayPaymentId = @PaymentId,
            RazorpaySignature = @Signature,
            VerifyResponseJson = @VerifyResponseJson,
            Status = 'PAYMENT_VERIFIED',
            PaidAt = NOW(),
            ModifiedAt = NOW()
        WHERE EscrowPaymentId = @EscrowPaymentId";

            using var connection = _db.GetConnection();

            await connection.ExecuteAsync(query, new
            {
                EscrowPaymentId = escrowPaymentId,
                PaymentId = paymentId,
                Signature = signature,
                VerifyResponseJson = verifyResponseJson
            });
        }


        public async Task<decimal?> GetServicePriceAsync(int serviceId)
        {
            var query = @"
        SELECT Price
        FROM services
        WHERE ServiceId = @ServiceId
        LIMIT 1";

            using var connection = _db.GetConnection();

            return await connection.QuerySingleOrDefaultAsync<decimal?>(
                query,
                new { ServiceId = serviceId });
        }
        public async Task<(IEnumerable<EscrowPaymentDetails> Data, int TotalRecords)>
     GetEscrowDetailsByUserIdAsync(
         int userId,
         string? search,
         string? key,
         int pageNumber,
         int pageSize)
        {
            var whereConditions = new List<string>
    {
        "ep.UserId = @UserId"
    };

            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    whereConditions.Add(@"
            (
                s.ServiceTitle LIKE @Search
                OR ep.RazorpayOrderId LIKE @Search
                OR ep.RazorpayPaymentId LIKE @Search
                OR ep.RazorpayTransferId LIKE @Search
                OR ep.Status LIKE @Search
                OR CAST(ep.BookingId AS CHAR) LIKE @Search
                OR CAST(ep.EscrowPaymentId AS CHAR) LIKE @Search
            )");

                    parameters.Add("Search", $"%{search}%");
                }
                else
                {
                    switch (key.ToLower())
                    {
                        case "servicetitle":
                        case "servicename":
                            whereConditions.Add("s.ServiceTitle LIKE @Search");
                            break;

                        case "status":
                            whereConditions.Add("ep.Status LIKE @Search");
                            break;

                        case "razorpayorderid":
                            whereConditions.Add("ep.RazorpayOrderId LIKE @Search");
                            break;

                        case "razorpaypaymentid":
                            whereConditions.Add("ep.RazorpayPaymentId LIKE @Search");
                            break;

                        case "razorpaytransferid":
                            whereConditions.Add("ep.RazorpayTransferId LIKE @Search");
                            break;

                        case "bookingid":
                            whereConditions.Add("CAST(ep.BookingId AS CHAR) LIKE @Search");
                            break;

                        case "escrowpaymentid":
                            whereConditions.Add("CAST(ep.EscrowPaymentId AS CHAR) LIKE @Search");
                            break;

                        default:
                            throw new ArgumentException(
                                $"Invalid search key: {key}");
                    }

                    parameters.Add("Search", $"%{search}%");
                }
            }

            var whereClause = string.Join(
                " AND ",
                whereConditions);

            // Count
            var countQuery = $@"
        SELECT COUNT(*)
        FROM EscrowPayments ep
        INNER JOIN bookings b
            ON b.BookingId = ep.BookingId
        INNER JOIN services s
            ON s.ServiceId = b.ServiceId
        WHERE {whereClause}";

            // Data
            var dataQuery = $@"
        SELECT
            ep.EscrowPaymentId,
            ep.BookingId,
            ep.UserId,
            ep.ClientId,
            b.ServiceId,
            s.ServiceTitle,
            ep.RazorpayOrderId,
            ep.RazorpayPaymentId,
            ep.RazorpayTransferId,
            ep.TotalAmount,
            ep.PlatformFee,
            ep.ExpertAmount,
            ep.Currency,
            ep.Status,
            ep.VerifyResponseJson,
            ep.TransferResponseJson,
            ep.ReleaseResponseJson,
            ep.RefundResponseJson,
            ep.PaidAt,
            ep.HeldAt,
            ep.ReleasedAt,
            ep.RefundedAt,
            ep.CreatedAt
        FROM EscrowPayments ep
        INNER JOIN bookings b
            ON b.BookingId = ep.BookingId
        INNER JOIN services s
            ON s.ServiceId = b.ServiceId
        WHERE {whereClause}
        ORDER BY ep.CreatedAt DESC
        LIMIT @PageSize OFFSET @Offset";

            parameters.Add("PageSize", pageSize);
            parameters.Add(
                "Offset",
                (pageNumber - 1) * pageSize);

            using var connection = _db.GetConnection();

            var totalRecords = await connection.ExecuteScalarAsync<int>(
                countQuery,
                parameters);

            var data = await connection.QueryAsync<EscrowPaymentDetails>(
                dataQuery,
                parameters);

            return (data, totalRecords);
        }
        public async Task UpdateTransferResponseAsync(
        int escrowPaymentId,
        string transferJson)
        {
            var sql = @"
        UPDATE EscrowPayments
        SET
            TransferResponseJson = @TransferJson,
            ModifiedAt = NOW()
        WHERE EscrowPaymentId = @EscrowPaymentId";

            using var connection = _db.GetConnection();

            await connection.ExecuteAsync(sql, new
            {
                EscrowPaymentId = escrowPaymentId,
                TransferJson = transferJson
            });
        }
    }
}