namespace ahello_backend.Models.Service
{
    public class ServiceDynamicPut
    {
        public int UserId { get; set; }
        public int ServiceTypeId { get; set; }
        public int ServiceCategoryId { get; set; }

        public string ServiceTitle { get; set; }
        public decimal Price { get; set; }

        public string Duration { get; set; }

        public string? ShortDescription { get; set; }

        public string? FullDescription { get; set; }

        public string? Tags { get; set; }

        public string? Language { get; set; }

        public IFormFile? ThumbnailImage { get; set; }      // form file
        public string? ThumbnailImageUrl { get; set; }      // set by controller after save
        public IFormFile? BannerImage { get; set; }         // form file
        public string? BannerImageUrl { get; set; }

        public IFormFile? IntroVideoFile { get; set; }   // ← new upload field
        public string? IntroVideo { get; set; }           // ← keeps existing URL string

        public string Status { get; set; }
        public bool IsActive { get; set; } = true;
        public string ModifiedBy { get; set; }

        public List<CreateServiceDynamicFieldValue> Fields { get; set; }
            = new();
    }
}
