namespace ahello_backend.Models.Reviews
{
    public class ReviewRequest
    {
        public int BookingId { get; set; }

        public string Comments { get; set; }

        public string Rating { get; set; }

        public string CreatedBy { get; set; }
    }
}
