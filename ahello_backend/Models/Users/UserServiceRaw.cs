namespace ahello_backend.Models.Users
{
    public class UserServiceRaw
    {
        public int UserId { get; set; }

        public string FullName { get; set; }
        public string ProfileUrl { get; set; }
        public int? CategoryId { get; set; }

        public string CategoryName { get; set; }

        public int ServiceId { get; set; }
        public int? ServiceTypeId { get; set; }

        public string ServiceTypeName { get; set; }
        public int ServiceCategoryId { get; set; }
        public string ServiceCategoryName { get; set; }
        public string ServiceTitle { get; set; }

        public string ShortDescription { get; set; }

        public decimal Price { get; set; }

        public string Duration { get; set; }

        public string IntroVideo { get; set; }

        public string ThumbnailImage { get; set; }

        public decimal AverageRating { get; set; }
        public int TotalRatingCount { get; set; }
    }
}
