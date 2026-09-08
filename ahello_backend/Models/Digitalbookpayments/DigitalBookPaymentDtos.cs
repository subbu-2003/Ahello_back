namespace ahello_backend.Models.Digitalbookpayments
{
    public class DigitalBookCreateOrderDto
    {
        public int UserId { get; set; }
        public int ClientId { get; set; }
        public int DigitalBookId { get; set; }
        public int CreatedBy { get; set; }
    }

    public class DigitalBookingPayloadDto
    {
        public int UserId { get; set; }
        public int ClientId { get; set; }
        public int ServiceId { get; set; }
        public int SlotId { get; set; }
        public DateTime ScheduleDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Status { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class DigitalBookVerifyAndHoldDto
    {
        public int UserId { get; set; }
        public int ClientId { get; set; }
        public int DigitalBookId { get; set; }
        public int CreatedBy { get; set; }

        public string RazorpayOrderId { get; set; } = null!;
        public string RazorpayPaymentId { get; set; } = null!;
        public string RazorpaySignature { get; set; } = null!;
    }

    public class DigitalBookReleaseDto
    {
        public int DigitalBookPaymentId { get; set; }
    }

    public class DigitalBookRefundDto
    {
        public int DigitalBookId { get; set; }
    }
}
