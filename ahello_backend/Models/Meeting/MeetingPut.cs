namespace ahello_backend.Models.Meeting
{
    public class MeetingPut
    {
        public int MeetingId { get; set; }

        public int UserId { get; set; }

        public int BookingId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? MeetingLink { get; set; }

        public string Status { get; set; }

        public string? ModifiedBy { get; set; }
    }
}
