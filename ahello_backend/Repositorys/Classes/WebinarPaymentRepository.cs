using ahello_backend.DbContexts;
using ahello_backend.Models.WebinarPayment;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class WebinarPaymentRepository : IWebinarPaymentRepository
    {
        private readonly DbContext _db;

        public WebinarPaymentRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<int> InsertAsync(WebinarPayment payment)
        {
            var query = @"
INSERT INTO webinarpayments
(
    WebinarRegistrationId,
    UserId,
    ClientId,
    RazorpayAccountId,
    RazorpayOrderId,
    RazorpayPaymentId,
    RazorpaySignature,
    RazorpayTransferId,
    TotalAmount,
    PlatformFee,
    TaxAmount,
    TaxRate,
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
    @WebinarRegistrationId,
    @UserId,
    @ClientId,
    @RazorpayAccountId,
    @RazorpayOrderId,
    @RazorpayPaymentId,
    @RazorpaySignature,
    @RazorpayTransferId,
    @TotalAmount,
    @PlatformFee,
    @TaxAmount,
    @TaxRate,
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

        public async Task<WebinarPayment?> GetByWebinarRegistrationIdAsync(int webinarRegistrationId)
        {
            var query = @"
                SELECT *
                FROM webinarpayments
                WHERE WebinarRegistrationId = @WebinarRegistrationId
                LIMIT 1";

            using var connection = _db.GetConnection();

            return await connection.QuerySingleOrDefaultAsync<WebinarPayment>(
                query,
                new { WebinarRegistrationId = webinarRegistrationId });
        }

        public async Task<dynamic?> GetWebinarRegistrationPaymentInfoAsync(int webinarRegistrationId)
        {
            var query = @"
        SELECT
            wr.WebinarRegistrationId,
            wr.UserId,
            wr.WebinarId,
            w.RegistrationFee AS Price
        FROM webinar_registrations wr
        INNER JOIN webinars w
            ON w.WebinarId = wr.WebinarId
        WHERE wr.WebinarRegistrationId = @WebinarRegistrationId
        LIMIT 1";

            using var connection = _db.GetConnection();

            return await connection.QuerySingleOrDefaultAsync<dynamic>(
                query,
                new { WebinarRegistrationId = webinarRegistrationId });
        }

        public async Task<string?> GetWebinarNameAsync(int webinarId)
        {
            var query = @"
        SELECT Title
        FROM webinars
        WHERE WebinarId = @WebinarId
        LIMIT 1";

            using var connection = _db.GetConnection();

            return await connection.QuerySingleOrDefaultAsync<string?>(
                query,
                new { WebinarId = webinarId });
        }

        public async Task<decimal?> GetWebinarPriceAsync(int webinarId)
        {
            var query = @"
        SELECT RegistrationFee
        FROM webinars
        WHERE WebinarId = @WebinarId
        LIMIT 1";

            using var connection = _db.GetConnection();

            return await connection.QuerySingleOrDefaultAsync<decimal?>(
                query,
                new { WebinarId = webinarId });
        }

        public async Task UpdateAfterPaymentAsync(
            int webinarPaymentId,
            string paymentId,
            string signature,
            string transferId,
            string transferJson)
        {
            var query = @"
    UPDATE webinarpayments
    SET
        RazorpayPaymentId = @PaymentId,
        RazorpaySignature = @Signature,
        RazorpayTransferId = @TransferId,
        TransferResponseJson = @TransferJson,
        Status = 'HELD',
        PaidAt = NOW(),
        HeldAt = NOW(),
        ModifiedAt = NOW()
    WHERE WebinarPaymentId = @WebinarPaymentId";

            using var connection = _db.GetConnection();

            await connection.ExecuteAsync(query, new
            {
                WebinarPaymentId = webinarPaymentId,
                PaymentId = paymentId,
                Signature = signature,
                TransferId = transferId,
                TransferJson = transferJson
            });
        }

        public async Task UpdatePaymentVerifiedAsync(
            int webinarPaymentId,
            string paymentId,
            string signature,
            string verifyResponseJson)
        {
            var query = @"
        UPDATE webinarpayments
        SET
            RazorpayPaymentId = @PaymentId,
            RazorpaySignature = @Signature,
            VerifyResponseJson = @VerifyResponseJson,
            Status = 'PAYMENT_VERIFIED',
            PaidAt = NOW(),
            ModifiedAt = NOW()
        WHERE WebinarPaymentId = @WebinarPaymentId";

            using var connection = _db.GetConnection();

            await connection.ExecuteAsync(query, new
            {
                WebinarPaymentId = webinarPaymentId,
                PaymentId = paymentId,
                Signature = signature,
                VerifyResponseJson = verifyResponseJson
            });
        }

        public async Task UpdateReleaseAsync(int webinarPaymentId, string releaseJson)
        {
            var query = @"
                UPDATE webinarpayments
                SET
                    Status = 'RELEASED',
                    ReleaseResponseJson = @ReleaseJson,
                    ReleasedAt = NOW(),
                    ModifiedAt = NOW()
                WHERE WebinarPaymentId = @WebinarPaymentId";

            using var connection = _db.GetConnection();

            await connection.ExecuteAsync(query, new
            {
                WebinarPaymentId = webinarPaymentId,
                ReleaseJson = releaseJson
            });
        }

        public async Task UpdateRefundAsync(int webinarPaymentId, string refundJson)
        {
            var query = @"
                UPDATE webinarpayments
                SET
                    Status = 'REFUNDED',
                    RefundResponseJson = @RefundJson,
                    RefundedAt = NOW(),
                    ModifiedAt = NOW()
                WHERE WebinarPaymentId = @WebinarPaymentId";

            using var connection = _db.GetConnection();

            await connection.ExecuteAsync(query, new
            {
                WebinarPaymentId = webinarPaymentId,
                RefundJson = refundJson
            });
        }

        public async Task UpdateStatusAsync(
            int webinarPaymentId,
            string status,
            string? failureReason = null)
        {
            var query = @"
                UPDATE webinarpayments
                SET
                    Status = @Status,
                    FailureReason = @FailureReason,
                    ModifiedAt = NOW()
                WHERE WebinarPaymentId = @WebinarPaymentId";

            using var connection = _db.GetConnection();

            await connection.ExecuteAsync(query, new
            {
                WebinarPaymentId = webinarPaymentId,
                Status = status,
                FailureReason = failureReason
            });
        }

        public async Task<(IEnumerable<WebinarPaymentDetails> Data, int TotalRecords)>
            GetWebinarPaymentDetailsByUserIdAsync(
                int userId,
                string? search,
                string? key,
                int pageNumber,
                int pageSize)
        {
            var whereConditions = new List<string>
            {
                "wp.UserId = @UserId"
            };

            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    whereConditions.Add(@"
            (
                w.Title LIKE @Search
                OR wp.RazorpayOrderId LIKE @Search
                OR wp.RazorpayPaymentId LIKE @Search
                OR wp.RazorpayTransferId LIKE @Search
                OR wp.Status LIKE @Search
                OR CAST(wp.WebinarRegistrationId AS CHAR) LIKE @Search
                OR CAST(wp.WebinarPaymentId AS CHAR) LIKE @Search
            )");

                    parameters.Add("Search", $"%{search}%");
                }
                else
                {
                    switch (key.ToLower())
                    {
                        case "webinartitle":
                        case "webinarname":
                            whereConditions.Add("w.Title LIKE @Search");
                            break;

                        case "status":
                            whereConditions.Add("wp.Status LIKE @Search");
                            break;

                        case "razorpayorderid":
                            whereConditions.Add("wp.RazorpayOrderId LIKE @Search");
                            break;

                        case "razorpaypaymentid":
                            whereConditions.Add("wp.RazorpayPaymentId LIKE @Search");
                            break;

                        case "razorpaytransferid":
                            whereConditions.Add("wp.RazorpayTransferId LIKE @Search");
                            break;

                        case "webinarregistrationid":
                            whereConditions.Add("CAST(wp.WebinarRegistrationId AS CHAR) LIKE @Search");
                            break;

                        case "webinarpaymentid":
                            whereConditions.Add("CAST(wp.WebinarPaymentId AS CHAR) LIKE @Search");
                            break;

                        default:
                            throw new ArgumentException($"Invalid search key: {key}");
                    }

                    parameters.Add("Search", $"%{search}%");
                }
            }

            var whereClause = string.Join(" AND ", whereConditions);

            var countQuery = $@"
        SELECT COUNT(*)
        FROM webinarpayments wp
        INNER JOIN webinar_registrations wr
            ON wr.WebinarRegistrationId = wp.WebinarRegistrationId
        INNER JOIN webinars w
            ON w.WebinarId = wr.WebinarId
        WHERE {whereClause}";

            var dataQuery = $@"
    SELECT
        wp.WebinarPaymentId,
        wp.WebinarRegistrationId,
        wp.UserId,
        wp.ClientId,
        wr.WebinarId,
        w.Title AS WebinarTitle,
        wp.RazorpayOrderId,
        wp.RazorpayPaymentId,
        wp.RazorpayTransferId,
        wp.TotalAmount,
        wp.PlatformFee,
        wp.TaxAmount,
        wp.TaxRate,
        wp.ExpertAmount,
        wp.Currency,
        wp.Status,
        wp.VerifyResponseJson,
        wp.TransferResponseJson,
        wp.ReleaseResponseJson,
        wp.RefundResponseJson,
        wp.PaidAt,
        wp.HeldAt,
        wp.ReleasedAt,
        wp.RefundedAt,
        wp.CreatedAt
    FROM webinarpayments wp
    INNER JOIN webinar_registrations wr
        ON wr.WebinarRegistrationId = wp.WebinarRegistrationId
    INNER JOIN webinars w
        ON w.WebinarId = wr.WebinarId
    WHERE {whereClause}
    ORDER BY wp.CreatedAt DESC
    LIMIT @PageSize OFFSET @Offset";

            parameters.Add("PageSize", pageSize);
            parameters.Add("Offset", (pageNumber - 1) * pageSize);

            using var connection = _db.GetConnection();

            var totalRecords = await connection.ExecuteScalarAsync<int>(countQuery, parameters);
            var data = await connection.QueryAsync<WebinarPaymentDetails>(dataQuery, parameters);

            return (data, totalRecords);
        }

        public async Task UpdateTransferResponseAsync(int webinarPaymentId, string transferJson)
        {
            var sql = @"
        UPDATE webinarpayments
        SET
            TransferResponseJson = @TransferJson,
            ModifiedAt = NOW()
        WHERE WebinarPaymentId = @WebinarPaymentId";

            using var connection = _db.GetConnection();

            await connection.ExecuteAsync(sql, new
            {
                WebinarPaymentId = webinarPaymentId,
                TransferJson = transferJson
            });
        }
    }
}
