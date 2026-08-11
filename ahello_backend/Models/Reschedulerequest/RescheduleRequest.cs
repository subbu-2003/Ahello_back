namespace ahello_backend.Models.Reschedulerequest
{
    public class RescheduleRequestPost
    {
        public int BookingId { get; set; }

        public int SlotId { get; set; }

        public DateTime RequestedDate { get; set; }

        public TimeSpan RequestedStartTime { get; set; }

        public TimeSpan RequestedEndTime { get; set; }

        public string? Reason { get; set; }

        public string CreatedBy { get; set; } = string.Empty;
    }

    public class RescheduleRequestStatusPut
    {
        public string Status { get; set; } = string.Empty;

        public string ModifiedBy { get; set; } = string.Empty;
    }

    public class RescheduleRequestRead
    {
        public int RequestId { get; set; }

        public int BookingId { get; set; }

        public int UserId { get; set; }

        public string? UserName { get; set; }
        public string? UserEmail { get; set; }

        public int ClientId { get; set; }

        public string? ClientName { get; set; }
        public string? ClientEmail { get; set; }

        public int ServiceId { get; set; }

        public string? ServiceTitle { get; set; }

        public int SlotId { get; set; }

        public DateTime RequestedDate { get; set; }

        public TimeSpan RequestedStartTime { get; set; }

        public TimeSpan RequestedEndTime { get; set; }

        public string? Reason { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }

        public string? ModifiedBy { get; set; }
    }
}
