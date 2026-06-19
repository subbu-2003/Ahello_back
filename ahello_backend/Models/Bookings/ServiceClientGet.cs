namespace ahello_backend.Models.Bookings
{
    public class ServiceClientGet
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string Email { get; set; }
        public DateTime? LastBookingDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string BookingStatus { get; set; }
    }
}
