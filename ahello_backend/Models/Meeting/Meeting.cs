namespace ahello_backend.Models.Meeting
{
    public class Meeting
    {
        public int MeetingId { get; set; }

        public int UserId { get; set; }
        public string? UserName { get; set; }

        public int ClientId { get; set; }
        public string? ClientName { get; set; }
        public string ClientEmail { get; set; }
        public int BookingId { get; set; }
        public int ServiceId { get; set; }

        public string? ServiceTitle { get; set; }

        public int ServiceCategoryId { get; set; }

        public string? ServiceCategoryName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public string? MeetingLink { get; set; }

        public string Status { get; set; }
        public string Email { get; set; }

        public bool ReminderSent { get; set; }

        public DateTime? LastReminderSent { get; set; }

        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }
}