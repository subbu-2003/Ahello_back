namespace ahello_backend.DTO.Payment
{
    public class CreateOrderDto
    {
        public int UserId { get; set; }
        public int ClientId { get; set; }
        public int ServiceId { get; set; }
        public int SlotId { get; set; }
        public DateTime ScheduleDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; } = "Confirmed";
        public string CreatedBy { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}