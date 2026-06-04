namespace ahello_backend.Models.Users
{
    public class UserServiceRead
    {
        public int UserId { get; set; }

        public string FullName { get; set; }

        public int ServiceId { get; set; }

        public string ServiceTitle { get; set; }

        public string ShortDescription { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public decimal AverageRating { get; set; }

        public string Duration { get; set; }

        public string IntroVideo { get; set; }

        public string ThumbnailImage { get; set; }

        public decimal Price { get; set; }
    }
}
