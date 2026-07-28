using ahello_backend.Models.Bookings;
using ahello_backend.Models.Invoices;
using ahello_backend.Models.Payment;

namespace ahello_backend.Services.Interfaces
{
    public interface IInvoiceService
    {
        // Called from EscrowPaymentService right after Status -> PAYMENT_VERIFIED
        Task<int> CreateInvoiceFromVerifiedPaymentAsync(EscrowPayment escrowPayment, BookingRead booking, string createdBy);

        Task<IEnumerable<InvoiceResponseDto>> GetAllInvoicesAsync();

        Task<InvoiceResponseDto?> GetInvoiceByBookingIdAsync(int bookingId);

        Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByUserIdAsync(int userId);
    }
}