namespace ahello_backend.Models.MeetingParticipant
{
    public class MeetingRecording
    {
        public int RecordingId { get; set; }

        public int MeetingId { get; set; }

        public string? RoomId { get; set; }

        public string? RoomName { get; set; }

        public string? HMSRecordingId { get; set; }

        public string? RecordingAssetId { get; set; }

        public string? FileName { get; set; }

        public string? RecordingUrl { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? EndedAt { get; set; }

        public int? DurationSeconds { get; set; }

        public string Status { get; set; } = "Starting";

        public DateTime? DeleteAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
