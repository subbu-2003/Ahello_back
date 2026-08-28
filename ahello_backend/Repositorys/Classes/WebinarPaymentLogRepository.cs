using ahello_backend.DbContexts;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class WebinarPaymentLogRepository : IWebinarPaymentLogRepository
    {
        private readonly DbContext _db;
        public WebinarPaymentLogRepository(DbContext db)
        {
            _db = db;
        }

        public async Task InsertAsync(
            int? webinarPaymentId,
            int? webinarRegistrationId,
            string action,
            string status,
            string? requestJson = null,
            string? responseJson = null,
            string? errorMessage = null,
            string? createdBy = null)
        {
            var query = @"
                INSERT INTO WebinarPaymentLogs
                (
                    WebinarPaymentId,
                    WebinarRegistrationId,
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
                    @WebinarPaymentId,
                    @WebinarRegistrationId,
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
                WebinarPaymentId = webinarPaymentId,
                WebinarRegistrationId = webinarRegistrationId,
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
