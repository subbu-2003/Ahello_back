namespace ahello_backend.Models.MeetingParticipant
{
    public class ParticipantMeetingLogResponse
    {
        public int MeetingId { get; set; }

        public int UserId { get; set; }

        public string? UserName { get; set; }

        public string? ParticipantType { get; set; }

        public int TotalDurationSeconds { get; set; }

        public string? TotalDuration { get; set; }

        public IEnumerable<MeetingParticipantLog> Sessions { get; set; }
            = new List<MeetingParticipantLog>();
    }
}
