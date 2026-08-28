using ahello_backend.Models.Payment;

namespace ahello_backend.Models.WebinarPayment
{
    public class WebinarPayment
    {
        public int WebinarPaymentId { get; set; }
        public int WebinarRegistrationId { get; set; }
        public int UserId { get; set; }
        public int ClientId { get; set; }
        public string? RazorpayAccountId { get; set; }
        public string? RazorpayOrderId { get; set; }
        public string? RazorpayPaymentId { get; set; }
        public string? RazorpaySignature { get; set; }
        public string? RazorpayTransferId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal ExpertAmount { get; set; }
        public string Currency { get; set; } = "INR";
        public string Status { get; set; } = "CREATED";
        public string? OrderResponseJson { get; set; }
        public string? VerifyResponseJson { get; set; }
        public string? TransferResponseJson { get; set; }
        public string? ReleaseResponseJson { get; set; }
        public string? RefundResponseJson { get; set; }
        public string? FailureReason { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? HeldAt { get; set; }
        public DateTime? ReleasedAt { get; set; }
        public DateTime? RefundedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class WebinarPaymentDetails
    {
        public int WebinarPaymentId { get; set; }
        public int WebinarRegistrationId { get; set; }
        public int UserId { get; set; }
        public int ClientId { get; set; }
        public int WebinarId { get; set; }
        public string? WebinarTitle { get; set; }
        public string? RazorpayOrderId { get; set; }
        public string? RazorpayPaymentId { get; set; }
        public string? RazorpayTransferId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal ExpertAmount { get; set; }
        public string Currency { get; set; } = "INR";
        public string Status { get; set; } = "CREATED";
        public string? VerifyResponseJson { get; set; }
        public string? TransferResponseJson { get; set; }
        public string? ReleaseResponseJson { get; set; }
        public string? RefundResponseJson { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? HeldAt { get; set; }
        public DateTime? ReleasedAt { get; set; }
        public DateTime? RefundedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class WebinarTimelinePagedResponseDto
    {
        public List<WebinarTimelineResponseDto> Data { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }

    public class WebinarTimelineResponseDto
    {
        public int WebinarPaymentId { get; set; }
        public int WebinarRegistrationId { get; set; }
        public int UserId { get; set; }
        public int ClientId { get; set; }
        public string? WebinarTitle { get; set; }
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
}
