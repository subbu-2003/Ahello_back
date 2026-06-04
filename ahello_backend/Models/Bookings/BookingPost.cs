namespace ahello_backend.Models.Bookings
{
    public class BookingPost
    {
        public int UserId { get; set; }

        public int ClientId { get; set; }

        public int ServiceId { get; set; }
        public int SlotId { get; set; }

        public DateTime ScheduleDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string Status { get; set; }

        public string CreatedBy { get; set; }
    }
}
