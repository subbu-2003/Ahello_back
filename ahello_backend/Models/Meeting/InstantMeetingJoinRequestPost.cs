namespace ahello_backend.Models.Meeting
{
    public class InstantMeetingJoinRequestPost
    {
        public string RoomName { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;
    }
}