using ahello_backend.Models.WebinarPayment;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IWebinarPaymentRepository
    {
        Task<int> InsertAsync(WebinarPayment payment);
        Task<WebinarPayment?> GetByWebinarRegistrationIdAsync(int webinarRegistrationId);
        Task<dynamic?> GetWebinarRegistrationPaymentInfoAsync(int webinarRegistrationId);
        Task<decimal?> GetWebinarPriceAsync(int webinarId);
        Task<string?> GetWebinarNameAsync(int webinarId);
        Task<(IEnumerable<WebinarPaymentDetails> Data, int TotalRecords)>
            GetWebinarPaymentDetailsByUserIdAsync(
                int userId,
                string? search,
                string? key,
                int pageNumber,
                int pageSize);
        Task UpdateAfterPaymentAsync(
            int webinarPaymentId,
            string paymentId,
            string signature,
            string transferId,
            string transferJson);
        Task UpdatePaymentVerifiedAsync(
            int webinarPaymentId,
            string paymentId,
            string signature,
            string verifyResponseJson);
        Task UpdateReleaseAsync(int webinarPaymentId, string releaseJson);
        Task UpdateRefundAsync(int webinarPaymentId, string refundJson);
        Task UpdateStatusAsync(
            int webinarPaymentId,
            string status,
            string? failureReason = null);
        Task UpdateTransferResponseAsync(int webinarPaymentId, string transferJson);
    }

}
