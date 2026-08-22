using ahello_backend.DbContexts;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class DigitalBookPaymentLogRepository : IDigitalBookPaymentLogRepository
    {
        private readonly DbContext _db;

        public DigitalBookPaymentLogRepository(DbContext db)
        {
            _db = db;
        }

        public async Task InsertAsync(
            int? digitalBookPaymentId,
            int? digitalBookingId,
            string action,
            string status,
            string? requestJson = null,
            string? responseJson = null,
            string? errorMessage = null,
            string? createdBy = null)
        {
            var query = @"
INSERT INTO DigitalBookPaymentLogs
(
    DigitalBookPaymentId,
    DigitalBookingId,
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
    @DigitalBookPaymentId,
    @DigitalBookingId,
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
                DigitalBookPaymentId = digitalBookPaymentId,
                DigitalBookingId = digitalBookingId,
                Action = action,
                Status = status,
                RequestJson = requestJson,
                ResponseJson = responseJson,
                ErrorMessage = errorMessage,
                CreatedBy = createdBy
            });
        }
    }
}
