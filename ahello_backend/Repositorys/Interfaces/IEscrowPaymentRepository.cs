using ahello_backend.Models.Payment;
namespace ahello_backend.Repositorys.Interfaces
{
    public interface IEscrowPaymentRepository
    {
        Task<int> InsertAsync(EscrowPayment payment);
        Task<EscrowPayment?> GetByBookingIdAsync(int bookingId);
        Task<dynamic?> GetBookingPaymentInfoAsync(int bookingId);
        Task<decimal?> GetServicePriceAsync(int serviceId);
        Task<IEnumerable<EscrowPaymentDetails>> GetEscrowDetailsByUserIdAsync(int userId);
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
    }
}