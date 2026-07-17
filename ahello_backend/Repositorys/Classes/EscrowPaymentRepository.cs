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
    }
}