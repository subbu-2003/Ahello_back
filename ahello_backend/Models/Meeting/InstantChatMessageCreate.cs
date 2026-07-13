namespace ahello_backend.Models.Meeting
{
    public class InstantChatMessageCreate
    {
        public string RoomId { get; set; }
        public string PeerId { get; set; }

        public string UserName { get; set; }

        public string? MessageText { get; set; }

        public string? AttachmentUrl { get; set; }
        public IFormFile? File { get; set; }
        public string? MessageType { get; set; }

        public bool IsRead { get; set; }

        public int? FormId { get; set; }
    }
}
