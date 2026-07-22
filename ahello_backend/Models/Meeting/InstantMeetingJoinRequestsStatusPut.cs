namespace ahello_backend.Models.Meeting
{
    public class InstantMeetingJoinRequestsStatusPut
    {
        public int InstantMeetingId { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}