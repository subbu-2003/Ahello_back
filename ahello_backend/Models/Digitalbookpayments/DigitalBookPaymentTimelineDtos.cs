using ahello_backend.Models.Payment;

namespace ahello_backend.Models.Digitalbookpayments
{
    public class DigitalBookPaymentTimelineResponseDto
    {
        public int DigitalBookPaymentId { get; set; }
        public int DigitalBookingId { get; set; }
        public int UserId { get; set; }
        public int ClientId { get; set; }
        public string? ServiceName { get; set; }
        public string? RazorpayOrderId { get; set; }
        public string? RazorpayPaymentId { get; set; }
        public string? RazorpayTransferId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal ExpertAmount { get; set; }
        public string? Currency { get; set; }
        public string? Status { get; set; }
        public List<TimelineStepDto> Timeline { get; set; } = new();
    }

    public class DigitalBookPaymentTimelinePagedResponseDto
    {
        public List<DigitalBookPaymentTimelineResponseDto> Data { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }

}
