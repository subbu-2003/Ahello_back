namespace ahello_backend.Models.Webinar
{
    public class Webinar
    {
        public int WebinarId { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public string WebinarType { get; set; } = "Live";

        public DateTime ScheduleDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public int? DurationMinutes { get; set; }

        public int? MaxParticipants { get; set; }

        public decimal RegistrationFee { get; set; }

        public string Status { get; set; } = "Draft";

        public string ApprovalStatus { get; set; } = "Pending";

        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }

        public string? VideoUrl { get; set; }

        public string VideoApprovalStatus { get; set; } = "NotRequired";

        public int? VideoApprovedBy { get; set; }
        public DateTime? VideoApprovedAt { get; set; }
        public string? VideoRejectionReason { get; set; }

        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
