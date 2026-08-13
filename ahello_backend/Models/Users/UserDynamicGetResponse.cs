using ahello_backend.Models.Users;

namespace ahello_backend.Models.User
{
    public class UserDynamicGetResponse
    {
        public int UserId { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string FullName { get; set; }

        public string Email { get; set; }
        public string? ProfileUrl { get; set; }
        public string? Slug { get; set; }

        public string MobileNumber { get; set; }

        public string WhatsAppNumber { get; set; }

        public string Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        public string Pincode { get; set; }

        public string Qualification { get; set; }

        public string Occupation { get; set; }

        public string CompanyName { get; set; }

        public string Experience { get; set; }

        public string? InstagramURL { get; set; }
        public string? FacebookURL { get; set; }
        public string? YouTubeURL { get; set; }

        public string WebsiteURL { get; set; }

        public string Notes { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<UserDynamicFieldResponse> Fields { get; set; }
        public List<UserDropdownOptionResponse> DropdownOptions { get; set; }
             = new List<UserDropdownOptionResponse>();
    }
}
