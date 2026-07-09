namespace ahello_backend.Models.Meeting
{
    public class PublicMeetingTokenPost
    {
        public string RoomId { get; set; } = "";

        public string RoomCode { get; set; } = "";

        public string Name { get; set; } = "";

        public bool IsHost { get; set; }
    }
}