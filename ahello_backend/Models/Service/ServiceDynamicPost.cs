using System.ComponentModel.DataAnnotations;

namespace ahello_backend.Models.Service
{
    public class ServiceDynamicPost
    {
        [Required(ErrorMessage = "User is required.")]
        public int? UserId { get; set; }

        [Required(ErrorMessage = "Service Type is required.")]
        public int? ServiceTypeId { get; set; }

        [Required(ErrorMessage = "Service Category is required.")]
        public int? ServiceCategoryId { get; set; }

        public string ServiceTitle { get; set; }
        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Please enter a valid price.")] 
        public decimal? Price { get; set; }

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
        public string CreatedBy { get; set; }

        public List<CreateServiceDynamicFieldValue> Fields { get; set; }
            = new();
    }
}
