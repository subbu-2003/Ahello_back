using ahello_backend.DbContexts;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class EscrowPaymentLogRepository : IEscrowPaymentLogRepository
    {
        private readonly DbContext _db;

        public EscrowPaymentLogRepository(DbContext db)
        {
            _db = db;
        }

        public async Task InsertAsync(
            int? escrowPaymentId,
            int? bookingId,
            string action,
            string status,
            string? requestJson = null,
            string? responseJson = null,
            string? errorMessage = null,
            string? createdBy = null)
        {
            var query = @"
                INSERT INTO EscrowPaymentLogs
                (
                    EscrowPaymentId,
                    BookingId,
                    Action,
                    Status,
                    RequestJson,
                    ResponseJson,
                    ErrorMessage,
                    CreatedAt,
                    CreatedBy
                )
                VALUES
                (
                    @EscrowPaymentId,
                    @BookingId,
                    @Action,
                    @Status,
                    @RequestJson,
                    @ResponseJson,
                    @ErrorMessage,
                    NOW(),
                    @CreatedBy
                )";

            using var connection = _db.GetConnection();

            await connection.ExecuteAsync(query, new
            {
                EscrowPaymentId = escrowPaymentId,
                BookingId = bookingId,
                Action = action,
                Status = status,
                RequestJson = requestJson,
                ResponseJson = responseJson,
                ErrorMessage = errorMessage,
                CreatedBy = createdBy
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
    }
}