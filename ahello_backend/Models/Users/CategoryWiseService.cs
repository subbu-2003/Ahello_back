namespace ahello_backend.Models.Users
{
    public class CategoryWiseService
    {
        public int ServiceId { get; set; }

        // User Profile
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? ProfileUrl { get; set; }
        public string? Slug { get; set; }

        // User Category
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }

        // Service Type
        public int? ServiceTypeId { get; set; }
        public string? ServiceTypeName { get; set; }

        // Service Category - used for grouping
        public int? ServiceCategoryId { get; set; }
        public string? ServiceCategoryName { get; set; }

        // Service
        public string? ServiceTitle { get; set; }
        public string? ShortDescription { get; set; }
        public decimal? Price { get; set; }
        public string? Duration { get; set; }
        public string? IntroVideo { get; set; }
        public string? ThumbnailImage { get; set; }

        // Rating
        public decimal AverageRating { get; set; }
        public int TotalRatingCount { get; set; }
    }
}
