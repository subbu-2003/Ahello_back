namespace ahello_backend.Models.Meeting
{
    public class MeetingStatusPut
    {
        public int MeetingId { get; set; }
        public string Status { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
