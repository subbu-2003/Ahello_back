namespace ahello_backend.Models.Blockdate
{
    public class BlockDateGet
    {
        public int BlockDateId { get; set; }

        public int UserId { get; set; }

        public string? UserName { get; set; }

        public int ServiceId { get; set; }

        public string? ServiceTitle { get; set; }

        public DateTime BlockDate { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public string? Reason { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }

        public string? ModifiedBy { get; set; }
    }
}
