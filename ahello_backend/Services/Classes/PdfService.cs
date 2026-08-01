using ahello_backend.Models.Invoices;
using ahello_backend.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ahello_backend.Services.Classes
{
    public class PdfService : IPdfService
    {
        public byte[] GenerateInvoicePdf(InvoicePdfDto invoice)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // Header
                    page.Header()
                        .Text("INVOICE")
                        .FontSize(24)
                        .Bold()
                        .AlignCenter();

                    // Content
                    page.Content().Column(column =>
                    {
                        column.Spacing(20);

                        // Invoice Information
                        column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10)
                        .Column(info =>
                        {
                            info.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"Invoice No : {invoice.InvoiceNumber}").Bold();

                                row.RelativeItem().AlignRight()
                                    .Text($"Issued : {invoice.IssuedAt:dd-MM-yyyy}");
                            });
                });

                        // FROM & TO
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(10)
                                .Column(c =>
                                {
                                    c.Item().Text("FROM").Bold().FontSize(14);

                                    c.Item().Text(invoice.ExpertName).Bold();
                                    c.Item().Text(invoice.ExpertEmail);
                                    c.Item().Text(invoice.ExpertMobile);
                                });

                            row.ConstantItem(30);

                            row.RelativeItem().Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(10)
                                .Column(c =>
                                {
                                    c.Item().Text("TO").Bold().FontSize(14);

                                    c.Item().Text(invoice.ClientName).Bold();
                                    c.Item().Text(invoice.ClientEmail);
                                    c.Item().Text(invoice.ClientMobile);
                                });
                        });

                        // Service Table
                       

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4);   // Description
                                columns.RelativeColumn(1);   // Schedule
                                columns.RelativeColumn(1);   // Time
                                columns.RelativeColumn(1);   // Amount
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("Description");
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignCenter().Text("Date");
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignCenter().Text("Time");
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignRight().Text("Amount");
                            });

                            table.Cell().BorderBottom(1).Padding(8).Text(invoice.ServiceTitle);

                            table.Cell().BorderBottom(1).Padding(8)
                                .AlignCenter()
                                .Text(invoice.ScheduleDate.ToString("dd-MM-yyyy"));

                            table.Cell().BorderBottom(1).Padding(8)
                                .AlignCenter()
                                .Text(invoice.StartTime);

                            table.Cell().BorderBottom(1).Padding(8)
                                .AlignRight()
                                .Text($"₹ {invoice.TotalAmount:F2}");
                        });

                        // Payment Summary
                        column.Item().AlignRight().Width(300)
                        .Border(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Padding(15)
                        .Column(summary =>
                        {
                            summary.Item().Text("PAYMENT SUMMARY")
                                .Bold()
                                .FontSize(14);

                            summary.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Total");
                                r.ConstantItem(80).AlignRight()
                                    .Text($"₹ {invoice.TotalAmount:F2}");
                            });

                            summary.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Platform Fee");
                                r.ConstantItem(90)
                                    .AlignRight()
                                    .Text($"₹ {invoice.PlatformFee:F2}");
                            });

                            summary.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Expert Amount");
                                r.ConstantItem(90)
                                    .AlignRight()
                                    .Text($"₹ {invoice.ExpertAmount:F2}");
                            });

                            summary.Item().PaddingVertical(5).LineHorizontal(1);

                            summary.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Total").Bold();

                                r.ConstantItem(90)
                                    .AlignRight()
                                    .Text($"₹ {invoice.TotalAmount:F2}")
                                    .Bold();
                            });
                        });
                        column.Item()
                    .PaddingTop(20)
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.DefaultTextStyle(x => x
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2));

                        text.Span("Thank you for choosing ");
                        text.Span("Ahllo").Bold();
                        text.Span("!");
                    });
                    });
                   
                });
            }).GeneratePdf();
        }
    }
}