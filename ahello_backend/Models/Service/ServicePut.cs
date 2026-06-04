namespace ahello_backend.Models.Service
{
    public class ServicePut
    {
        public int UserId { get; set; }
        public int ServiceTypeId { get; set; }
        public int ServiceCategoryId { get; set; }
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

        public string ModifiedBy { get; set; }
    }
}
