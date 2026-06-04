using System.ComponentModel.DataAnnotations;

namespace ahello_backend.Models.Users
{
    public class UserPost
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public IFormFile? ProfileFile { get; set; }

        public string? ProfileUrl { get; set; }

        public string MobileNumber { get; set; }

        public string WhatsAppNumber { get; set; }

        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? Country { get; set; }

        public string? Pincode { get; set; }

        public string? Qualification { get; set; }

        public string? Occupation { get; set; }

        public string? CompanyName { get; set; }

        public string? Experience { get; set; }

        public string? SocialMediaLinks { get; set; }

        public string? WebsiteURL { get; set; }

        public string? Notes { get; set; }

        public string? CreatedBy { get; set; }
    }
}
