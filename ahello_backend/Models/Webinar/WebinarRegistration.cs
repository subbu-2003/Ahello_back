namespace ahello_backend.Models.Webinar
{
    public class WebinarRegistration
    {
        public int WebinarRegistrationId { get; set; }

        public int WebinarId { get; set; }

        public int UserId { get; set; }

        public DateTime RegistrationDate { get; set; }

        public string Status { get; set; } = "Pending";

        public string PaymentStatus { get; set; } = "NotRequired";

        public decimal PaymentAmount { get; set; }

        public int? PaymentId { get; set; }

        public DateTime? JoinedAt { get; set; }

        public DateTime? LeftAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
