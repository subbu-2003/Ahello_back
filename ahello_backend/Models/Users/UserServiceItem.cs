namespace ahello_backend.Models.Users
{
    public class UserServiceItem
    {
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
    }
}
