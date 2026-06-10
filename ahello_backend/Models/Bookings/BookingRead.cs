namespace ahello_backend.Models.Bookings
{
    public class BookingRead
    {
        public int BookingId { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }


        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string? ClientEmail { get; set; }

        public int ServiceId { get; set; }

        public string ServiceTitle { get; set; }

        public DateTime ScheduleDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }

        public bool AutoReschedule { get; set; }
    }
}
