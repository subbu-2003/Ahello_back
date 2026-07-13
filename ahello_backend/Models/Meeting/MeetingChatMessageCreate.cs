namespace ahello_backend.Models.Meeting
{
    public class MeetingChatMessageCreate
    {
        public int MeetingId { get; set; }

        public int UserId { get; set; }

        public string MessageText { get; set; }

        public string? AttachmentUrl { get; set; }
        public IFormFile? File { get; set; }

        public string MessageType { get; set; }

        public bool IsRead { get; set; }
        public int? FormId { get; set; }
    }
}
