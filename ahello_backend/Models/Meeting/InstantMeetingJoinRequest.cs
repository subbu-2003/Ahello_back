namespace ahello_backend.Models.Meeting
{
    public class InstantMeetingJoinRequest
    {
        public int Id { get; set; }

        public int InstantMeetingId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}