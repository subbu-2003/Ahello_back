namespace ahello_backend.Models.Blockdate
{
    public class BlockDatePost
    {
        public int UserId { get; set; }

        public int ServiceId { get; set; }

        public DateTime BlockDate { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public string? Reason { get; set; }

        public string? CreatedBy { get; set; }
    }
}
