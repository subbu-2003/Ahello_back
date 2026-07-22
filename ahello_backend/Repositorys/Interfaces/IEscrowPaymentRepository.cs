using ahello_backend.Models.Payment;
namespace ahello_backend.Repositorys.Interfaces
{
    public interface IEscrowPaymentRepository
    {
        Task<int> InsertAsync(EscrowPayment payment);
        Task<EscrowPayment?> GetByBookingIdAsync(int bookingId);
        Task<dynamic?> GetBookingPaymentInfoAsync(int bookingId);
        Task<decimal?> GetServicePriceAsync(int serviceId);
        Task<string?> GetServiceNameAsync(int serviceId);

        Task<(IEnumerable<EscrowPaymentDetails> Data, int TotalRecords)>
GetEscrowDetailsByUserIdAsync(
    int userId,
    string? search,
    string? key,
    int pageNumber,
    int pageSize);
        Task UpdateAfterPaymentAsync(
            int escrowPaymentId,
            string paymentId,
            string signature,
            string transferId,
            string transferJson);
        Task UpdatePaymentVerifiedAsync(
        int escrowPaymentId,
        string paymentId,
        string signature,
        string verifyResponseJson); 
        Task UpdateReleaseAsync(int escrowPaymentId, string releaseJson);
        Task UpdateRefundAsync(int escrowPaymentId, string refundJson);
        Task UpdateStatusAsync(
            int escrowPaymentId,
            string status,
            string? failureReason = null);
        Task UpdateTransferResponseAsync(int escrowPaymentId, string transferJson);
    }
}