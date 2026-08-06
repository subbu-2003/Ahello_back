namespace ahello_backend.Models
{
    public class DashboardGet
    {
        public int UserId { get; set; }

        public DashboardStatistics Statistics { get; set; } = new();

        public List<DashboardServiceGet> Services { get; set; } = new();

        public List<DashboardRecentBookingGet> RecentBookings { get; set; } = new();
    }

    public class DashboardStatistics
    {
        public int TotalServices { get; set; }

        public int TotalBookings { get; set; }

        public int ConfirmedBookings { get; set; }

        public int PendingBookings { get; set; }

        public int CompletedBookings { get; set; }

        public int CancelledBookings { get; set; }

        public int RejectedBookings { get; set; }

        public int RescheduledBookings { get; set; }

        public int NoShowBookings { get; set; }

        public int TotalInvoices { get; set; }

        public int TotalMeetings { get; set; }

        public int UpcomingMeetings { get; set; }

        public int CompletedMeetings { get; set; }

        public int CancelledMeetings { get; set; }

        public decimal AverageRating { get; set; }

        public int TotalReviews { get; set; }

        public decimal TotalRevenue { get; set; }

        public decimal ReleasedAmount { get; set; }

        public decimal PendingAmount { get; set; }
    }

    public class DashboardServiceGet
    {
        public string ServiceName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string Status { get; set; } = string.Empty;

        public int TotalBookings { get; set; }

        public int CompletedBookings { get; set; }

        public int PendingBookings { get; set; }

        public int CancelledBookings { get; set; }

        public decimal AverageRating { get; set; }

        public int TotalReviews { get; set; }

        public decimal Revenue { get; set; }
    }

    public class DashboardRecentBookingGet
    {
        public int BookingId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public string ClientName { get; set; } = string.Empty;

        public DateTime ScheduleDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
