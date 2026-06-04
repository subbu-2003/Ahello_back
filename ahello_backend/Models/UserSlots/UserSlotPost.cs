namespace ahello_backend.Models.UserSlots
{
    public class UserSlotPost
    {
        public int UserId { get; set; }
        public int ServiceId { get; set; }

        public DateTime? SlotDate { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? RecurrenceType { get; set; }
        public int? DayOfWeek { get; set; }
        public int? DayOfMonth { get; set; }

        public string CreatedBy { get; set; }
    }
}
