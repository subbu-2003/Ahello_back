using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ahllo.Controllers
{
    [ApiController]
    [Route("api/invoices")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        // GET api/invoices
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var invoices = await _invoiceService.GetAllInvoicesAsync();
            return Ok(invoices);
        }

        // GET api/invoices/booking/5
        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetByBookingId(int bookingId)
        {
            var invoice = await _invoiceService.GetInvoiceByBookingIdAsync(bookingId);
            if (invoice == null)
                return NotFound(new { message = $"No invoice found for BookingId {bookingId}" });

            return Ok(invoice);
        }

        // GET api/invoices/user/15
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var invoices = await _invoiceService.GetInvoicesByUserIdAsync(userId);
            return Ok(invoices);
        }
        [HttpGet("{bookingId}/download")]
        public async Task<IActionResult> DownloadInvoice(int bookingId)
        {
            var pdf = await _invoiceService.DownloadInvoicePdfAsync(bookingId);

            if (pdf == null)
                return NotFound();

            return File(
                pdf,
                "application/pdf",
                $"Invoice_{bookingId}.pdf");
        }
    }
}
