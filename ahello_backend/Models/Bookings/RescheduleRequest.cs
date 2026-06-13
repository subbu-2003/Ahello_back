namespace ahello_backend.Models.Bookings
{
    public class RescheduleRequest
    {
        public DateTime NewDate { get; set; }
        public TimeSpan NewStartTime { get; set; }
        public TimeSpan NewEndTime { get; set; }

        public int SlotId { get; set; }

        // Current logged-in user id / expert id / client id
        public int RescheduledBy { get; set; }
    }
}
