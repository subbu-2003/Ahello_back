namespace ahello_backend.Models.Meeting
{
    public class InstantChatMessageUpdate
    {
        public int InstantMessageId { get; set; }

        public string RoomId { get; set; }

        public string? MessageText { get; set; }

        public string? AttachmentUrl { get; set; }

        public string? MessageType { get; set; }

        public bool IsRead { get; set; }

        public bool IsDeleted { get; set; }

        public int? FormId { get; set; }
    }
}
