namespace ahello_backend.Models.MeetingParticipant
{
    public class MeetingWithParticipantsResponse
    {
        public int MeetingId { get; set; }
        public int ParticipantCount { get; set; }
        public int TotalDurationSeconds { get; set; }
        public string TotalDuration { get; set; }
        public MeetingRecordingInfo? Recording { get; set; }
        public List<ParticipantSessionInfo> Participants { get; set; }
    }

    public class MeetingRecordingInfo
    {
        public int? RecordingId { get; set; }
        public string? RecordingUrl { get; set; }
        public string? FileName { get; set; }
        public string? Status { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int? DurationSeconds { get; set; }
    }

    public class ParticipantSessionInfo
    {
        public int MeetingParticipantId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string ParticipantType { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public int? DurationSeconds { get; set; }
        public string? Duration { get; set; }
    }
}