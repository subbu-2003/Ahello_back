using ahello_backend.Models.Digitalbookpayments;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IDigitalBookPaymentRepository
    {
        Task<int> InsertAsync(DigitalBookPayment payment);
        Task<DigitalBookPayment?> GetByDigitalBookIdAsync(int digitalBookId);
        Task<dynamic?> GetDigitalBookPaymentInfoAsync(int digitalBookId);

        Task<(IEnumerable<DigitalBookPaymentDetails> Data, int TotalRecords)>
            GetDigitalBookPaymentDetailsByUserIdAsync(
                int userId,
                string? search,
                string? key,
                int pageNumber,
                int pageSize);
        Task UpdateAfterPaymentAsync(
            int digitalBookPaymentId,
            string paymentId,
            string signature,
            string transferId,
            string transferJson);
        Task UpdatePaymentVerifiedAsync(
            int digitalBookPaymentId,
            string paymentId,
            string signature,
            string verifyResponseJson);
        Task UpdateReleaseAsync(int digitalBookPaymentId, string releaseJson);
        Task UpdateRefundAsync(int digitalBookPaymentId, string refundJson);
        Task UpdateStatusAsync(
            int digitalBookPaymentId,
            string status,
            string? failureReason = null);
        Task UpdateTransferResponseAsync(int digitalBookPaymentId, string transferJson);
        Task<DigitalBookPayment?> GetByIdAsync(int digitalBookPaymentId);
    }
}
