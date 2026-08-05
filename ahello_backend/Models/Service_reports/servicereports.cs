namespace ahello_backend.Models.Service_reports
{
    // ---------------- REQUEST ----------------
    public class ServiceReportRequestModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int UserId { get; set; }
        public string? ServiceTitle { get; set; }
        public string? ServiceCategoryName { get; set; }
        public string? Search { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    // ---------------- RESPONSE ----------------
    public class ServiceReportResponseModel
    {
        // Datewise -> null. Monthwise -> populated. Yearwise -> null (see YearlySummary instead).
        public ServiceReportSummaryModel? Summary { get; set; }

        // Only populated for the Yearwise report: one aggregated row per year.
        public List<YearlySummaryModel>? YearlySummary { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<ServiceReportItemModel> Data { get; set; }
        public PaginationModel Pagination { get; set; }
    }

    public class ServiceReportSummaryModel
    {
        public int TotalServices { get; set; }
        public int PublishedServices { get; set; }
        public int DraftServices { get; set; }
        public int TotalClients { get; set; }
        public int TotalBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int PendingBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int RejectedBookings { get; set; }
        public int RescheduledBookings { get; set; }
        public int NoShowBookings { get; set; }
        public decimal TotalRevenue { get; set; }
    }
    public class YearlySummaryModel
    {
        public string Year { get; set; }
        public int TotalBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int PendingBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int RejectedBookings { get; set; }
        public int RescheduledBookings { get; set; }
        public int NoShowBookings { get; set; }
        public decimal Revenue { get; set; }
    }
    public class ServiceReportItemModel
    {
        // Used as the "period" key row for Datewise/Monthwise/Yearwise reports
        public string PeriodLabel { get; set; }      // e.g. "2026-01-05" / "2026-01" / "2026"

        public int ServiceId { get; set; }
        public string ServiceTitle { get; set; }
        public string ServiceCategoryName { get; set; }
        public string ServiceTypeName { get; set; }
        public decimal Price { get; set; }
        public string Duration { get; set; }
        public string Language { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }

        public int BookingCount { get; set; }
        public int ConfirmedBookings { get; set; }
        public int PendingBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int RejectedBookings { get; set; }
        public int RescheduledBookings { get; set; }
        public int NoShowBookings { get; set; }
        public decimal Revenue { get; set; }

        public List<ClientBookingModel> Clients { get; set; } = new List<ClientBookingModel>();
        public List<DynamicFieldModel> DynamicFields { get; set; } = new List<DynamicFieldModel>();
    }

    public class ClientBookingModel
    {
        public int BookingId { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public DateTime BookingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string BookingStatus { get; set; } // Final settled amount from escrowpayments.ExpertAmount for this booking (0 if no escrow record yet)

        public decimal ExpertAmount { get; set; }

    }

    public class DynamicFieldModel
    {
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
    }

    public class PaginationModel
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }

    // Flat row used internally when mapping Dapper query results before grouping
    public class ServiceReportFlatRow
    {
        public string PeriodLabel { get; set; }
        public int ServiceId { get; set; }
        public string ServiceTitle { get; set; }
        public string ServiceCategoryName { get; set; }
        public string ServiceTypeName { get; set; }
        public decimal Price { get; set; }
        public string Duration { get; set; }
        public string Language { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }

        public int? BookingId { get; set; }
        public int? ClientId { get; set; }
        public string ClientName { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public DateTime? BookingDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string BookingStatus { get; set; }
        // From escrowpayments.ExpertAmount, joined on BookingId (null if no escrow record for this booking)
        public decimal? ExpertAmount { get; set; }
    }
}
