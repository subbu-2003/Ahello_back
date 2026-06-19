namespace ahello_backend.Models.Bookings
{
    public class ServiceWiseClientRow
    {
        public int ServiceId { get; set; }
        public string ServiceTitle { get; set; }

        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string Email { get; set; }
        public DateTime? LastBookingDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string BookingStatus { get; set; }
    }
}
