namespace ahello_backend.Models.Admin
{
    public class AdminDashboardModel
    {
        public int TotalUsers { get; set; }
        public int TotalServices { get; set; }
        public int TotalCategories { get; set; }
        public int TotalServiceTypes { get; set; }

        // Bookings
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int NoShowBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int RejectedBookings { get; set; }
        public int RescheduledBookings { get; set; }

        public List<AdminDashboardBookingModel> LatestBookings { get; set; }
            = new List<AdminDashboardBookingModel>();

        // Meetings
        public int TotalMeetings { get; set; }
        public int PendingMeetings { get; set; }
        public int MissedMeetings { get; set; }
        public int RescheduledMeetings { get; set; }
        public int CancelledMeetings { get; set; }
        public int CompletedMeetings { get; set; }

        // Reviews
        public int TotalReviews { get; set; }
        public decimal AverageRating { get; set; }

        // Invoices
        public int TotalInvoices { get; set; }
        public int IssuedInvoices { get; set; }
        public int CancelledInvoices { get; set; }

        public List<AdminDashboardInvoiceModel> LatestInvoices { get; set; }
            = new List<AdminDashboardInvoiceModel>();

        // Payments
        public int TotalPayments { get; set; }
        public int CreatedPayments { get; set; }
        public int PaymentVerified { get; set; }
        public int PaymentFailed { get; set; }
        public int HeldPayments { get; set; }
        public int ReleasedPayments { get; set; }
        public int RefundedPayments { get; set; }
        public int CancelledPayments { get; set; }
        public int ErrorPayments { get; set; }

        public List<AdminDashboardPaymentModel> LatestPayments { get; set; }
            = new List<AdminDashboardPaymentModel>();

        // Revenue
        public decimal TotalRevenue { get; set; }
        public decimal TotalPlatformFee { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public decimal TotalExpertAmount { get; set; }
        public decimal ReleasedAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal RefundedAmount { get; set; }

        // Other
        public int TotalReschedules { get; set; }
        public int TotalBlockDates { get; set; }
        public int TotalUserSlots { get; set; }
        public int TotalBookedSlots { get; set; }
    }

    public class AdminDashboardBookingModel
    {
        public int BookingId { get; set; }
        public string Customer { get; set; }
        public string Service { get; set; }
        public string ScheduleDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Status { get; set; }
    }

    public class AdminDashboardInvoiceModel
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string Customer { get; set; }
        public decimal Total { get; set; }
        public string Date { get; set; }
    }

    public class AdminDashboardPaymentModel
    {
        public int EscrowPaymentId { get; set; }
        public int BookingId { get; set; }
        public string Customer { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal ExpertAmount { get; set; }
        public string Status { get; set; }
        public string Date { get; set; }
    }
}
