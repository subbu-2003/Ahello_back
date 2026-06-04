namespace ahello_backend.Models.Blockdate
{
    public class BlockDatePut
    {
        public int BlockDateId { get; set; }

        public int UserId { get; set; }

        public int ServiceId { get; set; }

        public DateTime BlockDate { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public string? Reason { get; set; }

        public string? ModifiedBy { get; set; }
    }
}
