namespace ahello_backend.Models.MeetingParticipant
{
    public class MeetingParticipantWithRecordingLog
    {
        // Participant fields
        public int MeetingParticipantId { get; set; }
        public int MeetingId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string ParticipantType { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public int? DurationSeconds { get; set; }
        public string? Duration { get; set; }
        public DateTime CreatedAt { get; set; }

        // Recording fields
        public int? RecordingId { get; set; }
        public string? RecordingUrl { get; set; }
        public string? FileName { get; set; }
        public string? RecordingStatus { get; set; }
        public DateTime? RecordingStartedAt { get; set; }
        public DateTime? RecordingEndedAt { get; set; }
        public int? RecordingDurationSeconds { get; set; }
    }
}