using ahello_backend.Models.Pagination;

namespace ahello_backend.Models.Users
{
    public class UserProfileResponse
    {
        public int UserId { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string FullName { get; set; }
        public string ProfileUrl { get; set; }
        public string? Slug { get; set; }
        public string? InstagramURL { get; set; }
        public string? FacebookURL { get; set; }
        public string? YouTubeURL { get; set; }
        public string Email { get; set; }

        public string MobileNumber { get; set; }

        public string WhatsAppNumber { get; set; }

        public string Notes { get; set; }

        // Rating Details
        public decimal AverageRating { get; set; }

        public int TotalRatingCount { get; set; }

        // Services Pagination
        public PagedResult<UserProfileServiceItem> Services { get; set; }
    }

    public class UserProfileServiceItem
    {
        public int ServiceId { get; set; }

        public int ServiceTypeId { get; set; }
        public int ServiceCategoryId { get; set; }
        public string ServiceCategoryName { get; set; }

        public string ServiceTitle { get; set; }

        public decimal Price { get; set; }

        public string Duration { get; set; }

        public string ShortDescription { get; set; }

        public string FullDescription { get; set; }

        public string Tags { get; set; }

        public string Language { get; set; }

        public string ThumbnailImage { get; set; }

        public string BannerImage { get; set; }

        public string IntroVideo { get; set; }

        public string Status { get; set; }
    }
}
