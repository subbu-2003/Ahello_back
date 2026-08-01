using ahello_backend.Models.Invoices;

namespace ahello_backend.Services.Interfaces
{
    public interface IPdfService
    {
        byte[] GenerateInvoicePdf(InvoicePdfDto invoice);
    }
}
