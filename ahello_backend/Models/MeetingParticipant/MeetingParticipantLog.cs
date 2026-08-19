namespace ahello_backend.Models.MeetingParticipant
{
    public class MeetingParticipantLog
    {
        public int MeetingParticipantId { get; set; }

        public int MeetingId { get; set; }

        public int UserId { get; set; }

        public string? UserName { get; set; }
        public string? ParticipantType { get; set; }

        public DateTime JoinedAt { get; set; }

        public DateTime? LeftAt { get; set; }

        public int? DurationSeconds { get; set; }
        public string? Duration { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
