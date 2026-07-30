namespace ahello_backend.Models.Meeting
{
    public class InstantMeeting
    {
        public int Id { get; set; }

        public string RoomId { get; set; } = string.Empty;

        public string RoomName { get; set; } = string.Empty;
        public string MeetingLink { get; set; } = string.Empty;

        public int UserId { get; set; }

        public string HostKey { get; set; } = string.Empty;

        public DateTime? CreatedAt { get; set; }
    }
}