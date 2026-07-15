namespace ahello_backend.Models.Meeting
{
    public class InstantMeetingTokenPost
    {
        public string RoomId { get; set; } = "";

        public string RoomName { get; set; } = "";

        public string Name { get; set; } = "";

        public string? HostKey { get; set; }
    }
}