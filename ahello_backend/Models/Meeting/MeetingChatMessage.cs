namespace ahello_backend.Models.Meeting
{
    public class MeetingChatMessage
    {
        public int ChatMessageId { get; set; }

        public int MeetingId { get; set; }

        public int UserId { get; set; }

        public string MessageText { get; set; }

        public string AttachmentUrl { get; set; }

        public string MessageType { get; set; }

        public bool IsRead { get; set; }

        public DateTime? SentAt { get; set; }

        public DateTime? ReadAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? CreatedAt { get; set; }
        public int? FormId { get; set; }
    }
}
