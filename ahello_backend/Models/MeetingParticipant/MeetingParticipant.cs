namespace ahello_backend.Models.MeetingParticipant
{
    public class MeetingParticipant
    {
        public int MeetingParticipantId { get; set; }

        public int MeetingId { get; set; }

        public int UserId { get; set; }

        public DateTime JoinedAt { get; set; }

        public DateTime? LeftAt { get; set; }

        public int? DurationSeconds { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
