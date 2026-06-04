namespace ahello_backend.Models.Reviews
{
    public class ReviewUpdateRequest
    {
        public string Comments { get; set; }

        public string Rating { get; set; }

        public string ModifiedBy { get; set; }
    }
}
