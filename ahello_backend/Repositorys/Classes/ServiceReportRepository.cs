using ahello_backend.DbContexts;
using ahello_backend.Models.Service_reports;
using ahello_backend.Repositorys.Interfaces;
using Dapper;
using System.Data;
using ClosedXML.Excel;

namespace ahello_backend.Repositorys.Classes
{
    public class ServiceReportRepository : IServiceReportRepository
    {
        private readonly DbContext _db;

        public ServiceReportRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<ServiceReportResponseModel> GetServiceReportDatewiseAsync(ServiceReportRequestModel request)
            => await BuildReportAsync(request, "%Y-%m-%d", ReportGrain.Daily);

        public async Task<ServiceReportResponseModel> GetServiceReportMonthwiseAsync(ServiceReportRequestModel request)
            => await BuildReportAsync(request, "%Y-%m", ReportGrain.Monthly);

        public async Task<ServiceReportResponseModel> GetServiceReportYearwiseAsync(ServiceReportRequestModel request)
            => await BuildReportAsync(request, "%Y", ReportGrain.Yearly);

        private enum ReportGrain
        {
            Daily,
            Monthly,
            Yearly
        }

        // ---------------------------------------------------------------
        // Core builder shared by Datewise / Monthwise / Yearwise
        // dateFormat -> MySQL DATE_FORMAT() pattern controlling the grouping grain
        // grain -> controls which summary block gets populated:
        //   Daily   -> Summary = null (no summary for Datewise)
        //   Monthly -> Summary = overall totals for the filtered range
        //   Yearly  -> YearlySummary = one aggregated row per year
        // ---------------------------------------------------------------
        private async Task<ServiceReportResponseModel> BuildReportAsync(
            ServiceReportRequestModel request, string dateFormat, ReportGrain grain)
        {
            using IDbConnection conn = _db.GetConnection();

            var flatRows = (await GetFlatRowsAsync(conn, request, dateFormat)).ToList();

            // Group flat (per-booking) rows into Period + Service items
            var grouped = flatRows
                .GroupBy(r => new { r.PeriodLabel, r.ServiceId })
                .Select(g =>
                {
                    var first = g.First();
                    var item = new ServiceReportItemModel
                    {
                        PeriodLabel = g.Key.PeriodLabel,
                        ServiceId = g.Key.ServiceId,
                        ServiceTitle = first.ServiceTitle,
                        ServiceCategoryName = first.ServiceCategoryName,
                        ServiceTypeName = first.ServiceTypeName,
                        Price = first.Price,
                        Duration = first.Duration,
                        Language = first.Language,
                        Status = first.Status,
                        IsActive = first.IsActive,
                        Clients = g.Where(x => x.BookingId.HasValue)
                                    .Select(x => new ClientBookingModel
                                    {
                                        BookingId = x.BookingId.Value,
                                        ClientId = x.ClientId ?? 0,
                                        ClientName = x.ClientName,
                                        Email = x.Email,
                                        MobileNumber = x.MobileNumber,
                                        BookingDate = x.BookingDate ?? default,
                                        StartTime = x.StartTime ?? default,
                                        EndTime = x.EndTime ?? default,
                                        BookingStatus = x.BookingStatus,
                                        ExpertAmount = x.ExpertAmount ?? 0
                                    }).ToList()
                    };

                    item.BookingCount = item.Clients.Count;
                    item.ConfirmedBookings = item.Clients.Count(c => c.BookingStatus == "Confirmed");
                    item.PendingBookings = item.Clients.Count(c => c.BookingStatus == "Pending");
                    item.CancelledBookings = item.Clients.Count(c => c.BookingStatus == "Cancelled");
                    item.RejectedBookings = item.Clients.Count(c => c.BookingStatus == "Rejected");
                    item.RescheduledBookings = item.Clients.Count(c => c.BookingStatus == "Rescheduled");
                    item.NoShowBookings = item.Clients.Count(c => c.BookingStatus == "NoShow");

                    // Revenue = sum of the expert's final settled amount (escrowpayments.ExpertAmount)
                    // for this service's confirmed bookings in this period.
                    item.Revenue = item.Clients
                        .Where(c => c.BookingStatus == "Confirmed")
                        .Sum(c => c.ExpertAmount);

                    return item;
                })
                .Where(x => x.PeriodLabel != null) // drop services with no bookings in range
                .OrderBy(x => x.PeriodLabel)
                .ThenBy(x => x.ServiceId)
                .ToList();

            // Attach dynamic fields per service (see NOTE in GetDynamicFieldsAsync about schema assumption)
            foreach (var item in grouped)
            {
                item.DynamicFields = await GetDynamicFieldsAsync(conn, request.UserId);
            }

            var totalRecords = grouped.Count;
            var pagedData = grouped
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            ServiceReportSummaryModel? summary = null;
            List<YearlySummaryModel>? yearlySummary = null;

            switch (grain)
            {
                case ReportGrain.Monthly:
                    summary = await GetSummaryAsync(conn, request);
                    break;
                case ReportGrain.Yearly:
                    yearlySummary = BuildYearlySummary(grouped);
                    break;
                case ReportGrain.Daily:
                default:
                    // No summary block for the Datewise report.
                    break;
            }

            return new ServiceReportResponseModel
            {
                Summary = summary,
                YearlySummary = yearlySummary,
                Data = pagedData,
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
        // Aggregates the already-grouped (period + service) rows into one
        // summary row per year for the Yearwise report. PeriodLabel already
        // equals the year string at this grain (dateFormat = "%Y").
        // ---------------------------------------------------------------
        private List<YearlySummaryModel> BuildYearlySummary(List<ServiceReportItemModel> groupedItems)
        {
            return groupedItems
                .GroupBy(x => x.PeriodLabel)
                .Select(g => new YearlySummaryModel
                {
                    Year = g.Key,
                    TotalBookings = g.Sum(x => x.BookingCount),
                    ConfirmedBookings = g.Sum(x => x.ConfirmedBookings),
                    PendingBookings = g.Sum(x => x.PendingBookings),
                    CancelledBookings = g.Sum(x => x.CancelledBookings),
                    RejectedBookings = g.Sum(x => x.RejectedBookings),
                    RescheduledBookings = g.Sum(x => x.RescheduledBookings),
                    NoShowBookings = g.Sum(x => x.NoShowBookings),
                    Revenue = g.Sum(x => x.Revenue)
                })
                .OrderBy(x => x.Year)
                .ToList();
        }

        // ---------------------------------------------------------------
        // Flat (per-booking) rows: services LEFT JOIN bookings within date range.
        // Client details come from the users table (FullName, Email) via
        // bookings.ClientId -> users.UserId, matching BookingRepository's pattern.
        // Revenue amount comes from escrowpayments.ExpertAmount, joined on BookingId
        // (this is the expert's final settled amount, not the service's list Price).
        // NOTE: verify the users table has a MobileNumber column with that exact
        // name — adjust if it's named differently (e.g. PhoneNumber, Mobile).
        // ---------------------------------------------------------------
        private async Task<IEnumerable<ServiceReportFlatRow>> GetFlatRowsAsync(
            IDbConnection conn, ServiceReportRequestModel request, string dateFormat)
        {
            const string sql = @"
                SELECT
                    CASE WHEN b.ScheduleDate IS NULL THEN NULL
                         ELSE DATE_FORMAT(b.ScheduleDate, @DateFormat) END AS PeriodLabel,
                    s.ServiceId,
                    s.ServiceTitle,
                    sc.ServiceCategoryName,
                    st.ServiceTypeName,
                    s.Price,
                    s.Duration,
                    s.Language,
                    s.Status,
                    s.IsActive,
                    b.BookingId,
                    b.ClientId,
                    cl.FullName AS ClientName,
                    cl.Email,
                    cl.MobileNumber,
                    b.ScheduleDate AS BookingDate,
                    b.StartTime,
                    b.EndTime,
                    b.Status AS BookingStatus,
                    ep.ExpertAmount
                FROM services s
                LEFT JOIN servicetypes st ON st.ServiceTypeId = s.ServiceTypeId
                LEFT JOIN servicecategorydynamic sc ON sc.ServiceCategoryId = s.ServiceCategoryId
                LEFT JOIN bookings b ON b.ServiceId = s.ServiceId
                    AND b.ScheduleDate BETWEEN @FromDate AND @ToDate
                LEFT JOIN users cl ON cl.UserId = b.ClientId
                LEFT JOIN (
                    SELECT ep1.BookingId, ep1.ExpertAmount
                    FROM escrowpayments ep1
                    INNER JOIN (
                        SELECT BookingId, MAX(EscrowPaymentId) AS LatestEscrowPaymentId
                        FROM escrowpayments
                        GROUP BY BookingId
                    ) latest
                        ON latest.BookingId = ep1.BookingId
                        AND latest.LatestEscrowPaymentId = ep1.EscrowPaymentId
                ) ep ON ep.BookingId = b.BookingId
                WHERE s.UserId = @UserId
                    AND (@ServiceTitle IS NULL OR s.ServiceTitle LIKE CONCAT('%', @ServiceTitle, '%'))
                    AND (@ServiceCategoryName IS NULL OR sc.ServiceCategoryName LIKE CONCAT('%', @ServiceCategoryName, '%'))
                    AND (@Search IS NULL OR s.ServiceTitle LIKE CONCAT('%', @Search, '%')
                         OR sc.ServiceCategoryName LIKE CONCAT('%', @Search, '%'))
                ORDER BY b.ScheduleDate, s.ServiceId;";

            var parameters = new
            {
                DateFormat = dateFormat,
                request.FromDate,
                request.ToDate,
                request.UserId,
                ServiceTitle = string.IsNullOrWhiteSpace(request.ServiceTitle) ? null : request.ServiceTitle,
                ServiceCategoryName = string.IsNullOrWhiteSpace(request.ServiceCategoryName) ? null : request.ServiceCategoryName,
                Search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search
            };

            return await conn.QueryAsync<ServiceReportFlatRow>(sql, parameters);
        }

        // ---------------------------------------------------------------
        // Overall summary block (not paginated, spans full filtered date range)
        // ---------------------------------------------------------------
        private async Task<ServiceReportSummaryModel> GetSummaryAsync(IDbConnection conn, ServiceReportRequestModel request)
        {
            const string sql = @"
                SELECT
                    (SELECT COUNT(*) FROM services s
                        WHERE s.UserId = @UserId
                        AND (@ServiceTitle IS NULL OR s.ServiceTitle LIKE CONCAT('%', @ServiceTitle, '%'))) AS TotalServices,
                    (SELECT COUNT(*) FROM services s
                        WHERE s.UserId = @UserId AND s.Status = 'Published'
                        AND (@ServiceTitle IS NULL OR s.ServiceTitle LIKE CONCAT('%', @ServiceTitle, '%'))) AS PublishedServices,
                    (SELECT COUNT(*) FROM services s
                        WHERE s.UserId = @UserId AND s.Status = 'Draft'
                        AND (@ServiceTitle IS NULL OR s.ServiceTitle LIKE CONCAT('%', @ServiceTitle, '%'))) AS DraftServices,
                    (SELECT COUNT(DISTINCT b.ClientId) FROM bookings b
                        INNER JOIN services s ON s.ServiceId = b.ServiceId
                        WHERE s.UserId = @UserId AND b.ScheduleDate BETWEEN @FromDate AND @ToDate) AS TotalClients,
                    (SELECT COUNT(*) FROM bookings b
                        INNER JOIN services s ON s.ServiceId = b.ServiceId
                        WHERE s.UserId = @UserId AND b.ScheduleDate BETWEEN @FromDate AND @ToDate) AS TotalBookings,
                    (SELECT COUNT(*) FROM bookings b
                        INNER JOIN services s ON s.ServiceId = b.ServiceId
                        WHERE s.UserId = @UserId AND b.Status = 'Confirmed'
                        AND b.ScheduleDate BETWEEN @FromDate AND @ToDate) AS ConfirmedBookings,
                    (SELECT COUNT(*) FROM bookings b
                        INNER JOIN services s ON s.ServiceId = b.ServiceId
                        WHERE s.UserId = @UserId AND b.Status = 'Pending'
                        AND b.ScheduleDate BETWEEN @FromDate AND @ToDate) AS PendingBookings,
                    (SELECT COUNT(*) FROM bookings b
                        INNER JOIN services s ON s.ServiceId = b.ServiceId
                        WHERE s.UserId = @UserId AND b.Status = 'Cancelled'
                        AND b.ScheduleDate BETWEEN @FromDate AND @ToDate) AS CancelledBookings,
                    (SELECT COUNT(*) FROM bookings b
                        INNER JOIN services s ON s.ServiceId = b.ServiceId
                        WHERE s.UserId = @UserId AND b.Status = 'Rejected'
                        AND b.ScheduleDate BETWEEN @FromDate AND @ToDate) AS RejectedBookings,
                    (SELECT COUNT(*) FROM bookings b
                        INNER JOIN services s ON s.ServiceId = b.ServiceId
                        WHERE s.UserId = @UserId AND b.Status = 'Rescheduled'
                        AND b.ScheduleDate BETWEEN @FromDate AND @ToDate) AS RescheduledBookings,
                    (SELECT COUNT(*) FROM bookings b
                        INNER JOIN services s ON s.ServiceId = b.ServiceId
                        WHERE s.UserId = @UserId AND b.Status = 'NoShow'
                        AND b.ScheduleDate BETWEEN @FromDate AND @ToDate) AS NoShowBookings,
                    (SELECT IFNULL(SUM(ep1.ExpertAmount), 0)
                        FROM bookings b
                        INNER JOIN services s ON s.ServiceId = b.ServiceId
                        INNER JOIN (
                            SELECT BookingId, MAX(EscrowPaymentId) AS LatestEscrowPaymentId
                            FROM escrowpayments
                            GROUP BY BookingId
                        ) latest ON latest.BookingId = b.BookingId
                        INNER JOIN escrowpayments ep1
                            ON ep1.BookingId = latest.BookingId
                            AND ep1.EscrowPaymentId = latest.LatestEscrowPaymentId
                        WHERE s.UserId = @UserId AND b.Status = 'Confirmed'
                        AND b.ScheduleDate BETWEEN @FromDate AND @ToDate) AS TotalRevenue;";

            var parameters = new
            {
                request.UserId,
                request.FromDate,
                request.ToDate,
                ServiceTitle = string.IsNullOrWhiteSpace(request.ServiceTitle) ? null : request.ServiceTitle
            };

            var summary = await conn.QueryFirstOrDefaultAsync<ServiceReportSummaryModel>(sql, parameters);
            return summary ?? new ServiceReportSummaryModel();
        }

        // ---------------------------------------------------------------
        // Dynamic fields for a service's owner (servicefieldvalues has no
        // ServiceId column in the given schema — it links via UserId + FieldCode.
        // Add a ServiceId column to servicefieldvalues if per-service values
        // are required; adjust this query accordingly).
        // ---------------------------------------------------------------
        private async Task<List<DynamicFieldModel>> GetDynamicFieldsAsync(IDbConnection conn, int userId)
        {
            const string sql = @"
                SELECT sf.FieldName AS FieldName, sfv.FieldValue AS FieldValue
                FROM servicefieldvalues sfv
                INNER JOIN servicefields sf ON sf.FieldCode = sfv.FieldCode
                WHERE sfv.UserId = @UserId AND sf.IsActive = 1;";

            var result = await conn.QueryAsync<DynamicFieldModel>(sql, new { UserId = userId });
            return result.ToList();
        }
        public async Task<byte[]> ExportDatewiseExcelAsync(ServiceReportRequestModel request)
        {
            var report = await GetServiceReportDatewiseAsync(request);
            return GenerateExcel(report, "Datewise Report");
        }

        public async Task<byte[]> ExportMonthwiseExcelAsync(ServiceReportRequestModel request)
        {
            var report = await GetServiceReportMonthwiseAsync(request);
            return GenerateExcel(report, "Monthwise Report");
        }

        public async Task<byte[]> ExportYearwiseExcelAsync(ServiceReportRequestModel request)
        {
            var report = await GetServiceReportYearwiseAsync(request);
            return GenerateExcel(report, "Yearwise Report");
        }
        private byte[] GenerateExcel(ServiceReportResponseModel report, string sheetName)
        {
            using var workbook = new XLWorkbook();

            var ws = workbook.Worksheets.Add(sheetName);

            int row = 1;

            ws.Cell(row, 1).Value = "Period";
            ws.Cell(row, 2).Value = "Service";
            ws.Cell(row, 3).Value = "Category";
            ws.Cell(row, 4).Value = "Type";
            ws.Cell(row, 5).Value = "Price";
            ws.Cell(row, 6).Value = "Bookings";
            ws.Cell(row, 7).Value = "Confirmed";
            ws.Cell(row, 8).Value = "Pending";
            ws.Cell(row, 9).Value = "Cancelled";
            ws.Cell(row, 10).Value = "Rejected";
            ws.Cell(row, 11).Value = "Rescheduled";
            ws.Cell(row, 12).Value = "No Show";
            ws.Cell(row, 13).Value = "Revenue";

            row++;

            foreach (var item in report.Data)
            {
                ws.Cell(row, 1).Value = item.PeriodLabel;
                ws.Cell(row, 2).Value = item.ServiceTitle;
                ws.Cell(row, 3).Value = item.ServiceCategoryName;
                ws.Cell(row, 4).Value = item.ServiceTypeName;
                ws.Cell(row, 5).Value = item.Price;
                ws.Cell(row, 6).Value = item.BookingCount;
                ws.Cell(row, 7).Value = item.ConfirmedBookings;
                ws.Cell(row, 8).Value = item.PendingBookings;
                ws.Cell(row, 9).Value = item.CancelledBookings;
                ws.Cell(row, 10).Value = item.RejectedBookings;
                ws.Cell(row, 11).Value = item.RescheduledBookings;
                ws.Cell(row, 12).Value = item.NoShowBookings;
                ws.Cell(row, 13).Value = item.Revenue;

                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }
    }
}
 