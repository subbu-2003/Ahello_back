namespace ahello_backend.Models.Webinar
{
    public class CreateWebinarRequest
    {
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

        public string? VideoUrl { get; set; }

        public string? CreatedBy { get; set; }
    }


    public class UpdateWebinarRequest
    {
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime ScheduleDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public int? DurationMinutes { get; set; }

        public int? MaxParticipants { get; set; }

        public decimal RegistrationFee { get; set; }

        public string? ModifiedBy { get; set; }
    }


    public class RegisterWebinarRequest
    {
        public int WebinarId { get; set; }

        public int UserId { get; set; }

        public string? CreatedBy { get; set; }
    }
}
