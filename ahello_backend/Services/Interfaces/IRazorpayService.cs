using ahello_backend.DTO.Payment;

namespace ahello_backend.Services.Interfaces
{
    public interface IRazorpayService
    {
        Task<(string accountId, string responseJson)> CreateLinkedAccountAsync(
            CreateLinkedAccountDto dto,
            dynamic user);

        Task<(string stakeholderId, string responseJson)> CreateStakeholderAsync(
            string accountId,
            CreateStakeholderDto dto,
            dynamic user);

        Task<(string productId, string responseJson)> RequestProductAsync(
            string accountId);

        Task<string> UpdateProductAsync(
            string accountId,
            string productId,
            UpdateProductDto dto);

        Task<(string orderId, string responseJson)> CreateOrderAsync(
            decimal amount,
            string currency,
            string receipt,
             string serviceName);

        bool VerifySignature(
            string orderId,
            string paymentId,
            string signature);

        Task<(string transferId, string responseJson)> CreateHeldTransferAsync(
            string paymentId,
            string accountId,
            decimal expertAmount);

        Task<string> ReleaseTransferAsync(string transferId);

        Task<string> ReverseTransferAsync(
            string transferId,
            decimal amount);

        Task<string> RefundPaymentAsync(
            string paymentId,
            decimal amount);
        Task<string> GetTransferAsync(string transferId);
    }
}