namespace ahello_backend.Models.Meeting
{
    public class InstantMeetingJoinRequest
    {
        public int Id { get; set; }

        public int InstantMeetingId { get; set; }

        public int UserId { get; set; }
        public string? UserName { get; set; } 

        public string? Email { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}