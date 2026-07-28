namespace ahello_backend.Models.Invoices
{
    public class InvoiceResponseDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime IssuedAt { get; set; }
        public string Status { get; set; }

        // Booking
        public int BookingId { get; set; }
        public DateTime ScheduleDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string BookingStatus { get; set; }

        // Expert (UserId)
        public int UserId { get; set; }
        public string ExpertName { get; set; }
        public string ExpertEmail { get; set; }
        public string ExpertMobile { get; set; }

        // Client
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public string ClientMobile { get; set; }

        // Service
        public int ServiceId { get; set; }
        public string ServiceTitle { get; set; }
        public string Duration { get; set; }

        // Payment
        public int EscrowPaymentId { get; set; }
        public string RazorpayOrderId { get; set; }
        public string RazorpayPaymentId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal ExpertAmount { get; set; }
        public string Currency { get; set; }
    }
}
