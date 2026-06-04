namespace ahello_backend.Models.Reviews
{
    public class Review
    {
        public int ReviewId { get; set; }

        public int BookingId { get; set; }

        public string Comments { get; set; }

        public string Rating { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }

        public string ModifiedBy { get; set; }
    }
}
