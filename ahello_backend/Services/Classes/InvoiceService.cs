using ahello_backend.Models.Bookings;
using ahello_backend.Models.Invoices;
using ahello_backend.Models.Payment;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceService(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<int> CreateInvoiceFromVerifiedPaymentAsync(
            EscrowPayment escrowPayment, BookingRead booking, string createdBy)
        {
            // Idempotency guard: verify-payment could theoretically be hit twice
            // (e.g. webhook + client callback both firing). Never create a duplicate invoice.
            var alreadyExists = await _invoiceRepository.ExistsByBookingIdAsync(booking.BookingId);
            if (alreadyExists)
            {
                var existing = await _invoiceRepository.GetInvoiceByBookingIdAsync(booking.BookingId);
                return existing!.InvoiceId;
            }

            var invoice = new Invoice
            {
                InvoiceNumber = GenerateInvoiceNumber(booking.BookingId),
                BookingId = booking.BookingId,
                EscrowPaymentId = escrowPayment.EscrowPaymentId,
                UserId = booking.UserId,
                ClientId = booking.ClientId,
                ServiceId = booking.ServiceId,
                TotalAmount = escrowPayment.TotalAmount,
                PlatformFee = escrowPayment.PlatformFee,
                ExpertAmount = escrowPayment.ExpertAmount,
                Currency = escrowPayment.Currency,
                Status = "Issued",
                IssuedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            return await _invoiceRepository.InsertInvoiceAsync(invoice);
        }

        // Simple, deterministic, no separate sequence table needed.
        private string GenerateInvoiceNumber(int bookingId)
        {
            return $"INV-{bookingId:D6}";
        }

        public Task<IEnumerable<InvoiceResponseDto>> GetAllInvoicesAsync()
            => _invoiceRepository.GetAllInvoicesAsync();

        public Task<InvoiceResponseDto?> GetInvoiceByBookingIdAsync(int bookingId)
            => _invoiceRepository.GetInvoiceByBookingIdAsync(bookingId);

        public Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByUserIdAsync(int userId)
            => _invoiceRepository.GetInvoicesByUserIdAsync(userId);
    }
}