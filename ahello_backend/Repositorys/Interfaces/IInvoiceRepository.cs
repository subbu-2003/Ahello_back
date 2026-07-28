using ahello_backend.Models.Invoices;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IInvoiceRepository
    {
        // Called from PaymentVerify flow right after escrowpayments.Status = PAYMENT_VERIFIED
        Task<int> InsertInvoiceAsync(Invoice invoice);

        // Returns true if an invoice already exists for this booking (idempotency guard)
        Task<bool> ExistsByBookingIdAsync(int bookingId);

        Task<IEnumerable<InvoiceResponseDto>> GetAllInvoicesAsync();

        Task<InvoiceResponseDto> GetInvoiceByBookingIdAsync(int bookingId);

        Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByUserIdAsync(int userId);
    }
}
