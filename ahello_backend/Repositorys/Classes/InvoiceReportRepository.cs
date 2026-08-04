using ahello_backend.DbContexts;
using ahello_backend.Models.Invoice_reports;
using ahello_backend.Models.Service_reports;
using ahello_backend.Repositorys.Interfaces;
using ClosedXML.Excel;
using Dapper;
using System.Data;

namespace ahello_backend.Repositorys.Classes
{
    public class InvoiceReportRepository : IInvoiceReportRepository
    {
        private readonly DbContext _db;

        public InvoiceReportRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<InvoiceReportResponseModel> GetDatewiseAsync(InvoiceReportRequestModel request)
            => await BuildReportAsync(request, includeStatusSummary: false, includeMonthlyBreakdown: false);

        public async Task<InvoiceReportResponseModel> GetMonthwiseAsync(InvoiceReportRequestModel request)
            => await BuildReportAsync(request, includeStatusSummary: true, includeMonthlyBreakdown: false);

        public async Task<InvoiceReportResponseModel> GetYearwiseAsync(InvoiceReportRequestModel request)
            => await BuildReportAsync(request, includeStatusSummary: false, includeMonthlyBreakdown: true);

        // ---------------------------------------------------------------
        // Core builder shared by Datewise / Monthwise / Yearwise.
        // Invoice rows are already the report grain (no grouping needed
        // like ServiceReport), so pagination happens directly in SQL.
        // ---------------------------------------------------------------
        private async Task<InvoiceReportResponseModel> BuildReportAsync(
            InvoiceReportRequestModel request, bool includeStatusSummary, bool includeMonthlyBreakdown)
        {
            using IDbConnection conn = _db.GetConnection();

            var totalRecords = await GetTotalCountAsync(conn, request);
            var data = (await GetPagedRowsAsync(conn, request)).ToList();
            var summary = await GetSummaryAsync(conn, request);

            List<InvoiceStatusSummaryModel>? statusSummary = null;
            List<InvoiceMonthlyBreakdownModel>? monthlyBreakdown = null;

            if (includeStatusSummary)
                statusSummary = (await GetStatusSummaryAsync(conn, request)).ToList();

            if (includeMonthlyBreakdown)
                monthlyBreakdown = (await GetMonthlyBreakdownAsync(conn, request)).ToList();

            return new InvoiceReportResponseModel
            {
                Summary = summary,
                StatusSummary = statusSummary,
                MonthlyBreakdown = monthlyBreakdown,
                Data = data,
                Pagination = new PaginationModel
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalRecords = totalRecords,
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize)
                }
            };
        }

        // ---------------------------------------------------------------
        // Shared WHERE clause + params for every query below.
        // Filters: IssuedAt date range, optional UserId (expert),
        // optional BookingId, optional Search (matches InvoiceNumber).
        // ---------------------------------------------------------------
        private const string WhereClause = @"
            WHERE i.IssuedAt BETWEEN @FromDate AND @ToDate
                AND (@UserId IS NULL OR i.UserId = @UserId)
                AND (@BookingId IS NULL OR i.BookingId = @BookingId)
                AND (@Search IS NULL OR i.InvoiceNumber LIKE CONCAT('%', @Search, '%'))";

        private static object BuildParams(InvoiceReportRequestModel request) => new
        {
            request.FromDate,
            request.ToDate,
            request.UserId,
            request.BookingId,
            Search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search
        };

        private async Task<int> GetTotalCountAsync(IDbConnection conn, InvoiceReportRequestModel request)
        {
            var sql = $"SELECT COUNT(*) FROM invoices i {WhereClause};";
            return await conn.ExecuteScalarAsync<int>(sql, BuildParams(request));
        }

        private async Task<IEnumerable<InvoiceReportItemModel>> GetPagedRowsAsync(IDbConnection conn, InvoiceReportRequestModel request)
        {
            var sql = $@"
                SELECT
                    i.InvoiceId, i.InvoiceNumber, i.BookingId, i.UserId, i.ClientId,
                    i.TotalAmount, i.PlatformFee, i.TaxAmount, i.ExpertAmount,
                    i.Currency, i.Status, i.IssuedAt
                FROM invoices i
                {WhereClause}
                ORDER BY i.IssuedAt DESC, i.InvoiceId DESC
                LIMIT @PageSize OFFSET @Offset;";

            var parameters = new DynamicParameters(BuildParams(request));
            parameters.Add("PageSize", request.PageSize);
            parameters.Add("Offset", (request.PageNumber - 1) * request.PageSize);

            return await conn.QueryAsync<InvoiceReportItemModel>(sql, parameters);
        }

        // ---------------------------------------------------------------
        // Overall summary block (not paginated, spans full filtered range)
        // ---------------------------------------------------------------
        private async Task<InvoiceReportSummaryModel> GetSummaryAsync(IDbConnection conn, InvoiceReportRequestModel request)
        {
            var sql = $@"
                SELECT
                    COUNT(*) AS TotalInvoices,
                    IFNULL(SUM(i.TotalAmount), 0) AS TotalAmount,
                    IFNULL(SUM(i.PlatformFee), 0) AS TotalPlatformFee,
                    IFNULL(SUM(i.TaxAmount), 0) AS TotalTaxAmount,
                    IFNULL(SUM(i.ExpertAmount), 0) AS TotalExpertAmount
                FROM invoices i
                {WhereClause};";

            var summary = await conn.QueryFirstOrDefaultAsync<InvoiceReportSummaryModel>(sql, BuildParams(request));
            return summary ?? new InvoiceReportSummaryModel();
        }

        // ---------------------------------------------------------------
        // Count of invoices per Status within range (Monthwise)
        // ---------------------------------------------------------------
        private async Task<IEnumerable<InvoiceStatusSummaryModel>> GetStatusSummaryAsync(IDbConnection conn, InvoiceReportRequestModel request)
        {
            var sql = $@"
                SELECT i.Status AS Status, COUNT(*) AS Count
                FROM invoices i
                {WhereClause}
                GROUP BY i.Status
                ORDER BY i.Status;";

            return await conn.QueryAsync<InvoiceStatusSummaryModel>(sql, BuildParams(request));
        }

        // ---------------------------------------------------------------
        // Per-month invoice count + revenue within range (Yearwise)
        // ---------------------------------------------------------------
        private async Task<IEnumerable<InvoiceMonthlyBreakdownModel>> GetMonthlyBreakdownAsync(IDbConnection conn, InvoiceReportRequestModel request)
        {
            var sql = $@"
                SELECT
                    MONTHNAME(i.IssuedAt) AS Month,
                    COUNT(*) AS InvoiceCount,
                    IFNULL(SUM(i.TotalAmount), 0) AS Revenue
                FROM invoices i
                {WhereClause}
                GROUP BY MONTH(i.IssuedAt), MONTHNAME(i.IssuedAt)
                ORDER BY MONTH(i.IssuedAt);";

            return await conn.QueryAsync<InvoiceMonthlyBreakdownModel>(sql, BuildParams(request));
        }

        // ---------------------------------------------------------------
        // Excel export — pulls the full (unpaginated) filtered row set
        // ---------------------------------------------------------------
        public async Task<byte[]> ExportDatewiseExcelAsync(InvoiceReportRequestModel request)
        {
            var rows = await GetAllRowsAsync(request);
            return GenerateExcel(rows, "Datewise Invoices");
        }

        public async Task<byte[]> ExportMonthwiseExcelAsync(InvoiceReportRequestModel request)
        {
            var rows = await GetAllRowsAsync(request);
            return GenerateExcel(rows, "Monthwise Invoices");
        }

        public async Task<byte[]> ExportYearwiseExcelAsync(InvoiceReportRequestModel request)
        {
            var rows = await GetAllRowsAsync(request);
            return GenerateExcel(rows, "Yearwise Invoices");
        }

        private async Task<List<InvoiceReportItemModel>> GetAllRowsAsync(InvoiceReportRequestModel request)
        {
            using IDbConnection conn = _db.GetConnection();

            var sql = $@"
                SELECT
                    i.InvoiceId, i.InvoiceNumber, i.BookingId, i.UserId, i.ClientId,
                    i.TotalAmount, i.PlatformFee, i.TaxAmount, i.ExpertAmount,
                    i.Currency, i.Status, i.IssuedAt
                FROM invoices i
                {WhereClause}
                ORDER BY i.IssuedAt DESC, i.InvoiceId DESC;";

            var result = await conn.QueryAsync<InvoiceReportItemModel>(sql, BuildParams(request));
            return result.ToList();
        }

        private byte[] GenerateExcel(List<InvoiceReportItemModel> data, string sheetName)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add(sheetName);

            int row = 1;
            ws.Cell(row, 1).Value = "Invoice Number";
            ws.Cell(row, 2).Value = "Booking Id";
            ws.Cell(row, 3).Value = "Expert Id";
            ws.Cell(row, 4).Value = "Client Id";
            ws.Cell(row, 5).Value = "Total Amount";
            ws.Cell(row, 6).Value = "Platform Fee";
            ws.Cell(row, 7).Value = "Tax Amount";
            ws.Cell(row, 8).Value = "Expert Amount";
            ws.Cell(row, 9).Value = "Currency";
            ws.Cell(row, 10).Value = "Status";
            ws.Cell(row, 11).Value = "Issued At";
            row++;

            foreach (var item in data)
            {
                ws.Cell(row, 1).Value = item.InvoiceNumber;
                ws.Cell(row, 2).Value = item.BookingId;
                ws.Cell(row, 3).Value = item.UserId;
                ws.Cell(row, 4).Value = item.ClientId;
                ws.Cell(row, 5).Value = item.TotalAmount;
                ws.Cell(row, 6).Value = item.PlatformFee;
                ws.Cell(row, 7).Value = item.TaxAmount;
                ws.Cell(row, 8).Value = item.ExpertAmount;
                ws.Cell(row, 9).Value = item.Currency;
                ws.Cell(row, 10).Value = item.Status;
                ws.Cell(row, 11).Value = item.IssuedAt;
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}