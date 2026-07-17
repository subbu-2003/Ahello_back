namespace ahello_backend.DTO.Payment
{
    public class VerifyAndHoldDto
    {
        public CreateOrderDto BookingPayload { get; set; } = new();
        public string RazorpayOrderId { get; set; } = string.Empty;
        public string RazorpayPaymentId { get; set; } = string.Empty;
        public string RazorpaySignature { get; set; } = string.Empty;
    }
}