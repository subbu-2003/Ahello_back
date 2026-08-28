namespace ahello_backend.Models.WebinarPayment
{
    public class CreateWebinarOrderDto
    {
        public int UserId { get; set; }
        public int WebinarId { get; set; }
        public int WebinarRegistrationId { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class WebinarRegistrationPayload
    {
        public int WebinarRegistrationId { get; set; }
        public int UserId { get; set; }
        public int ClientId { get; set; }
        public int WebinarId { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class VerifyWebinarPaymentDto
    {
        public WebinarRegistrationPayload WebinarRegistrationPayload { get; set; } = new();
        public string RazorpayOrderId { get; set; } = string.Empty;
        public string RazorpayPaymentId { get; set; } = string.Empty;
        public string RazorpaySignature { get; set; } = string.Empty;
    }

    public class ReleaseWebinarDto
    {
        public int WebinarRegistrationId { get; set; }
    }

    public class RefundWebinarDto
    {
        public int WebinarRegistrationId { get; set; }
    }
}
