namespace ahello_backend.Models.Meeting
{
    public class InstantChatMessage
    {
        public int InstantMessageId { get; set; }

        public string RoomId { get; set; }
        public string PeerId { get; set; }

        public string UserName { get; set; }

        public string? MessageText { get; set; }

        public string? AttachmentUrl { get; set; }

        public string? MessageType { get; set; }

        public bool IsRead { get; set; }

        public DateTime? SentAt { get; set; }

        public DateTime? ReadAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public int? FormId { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
