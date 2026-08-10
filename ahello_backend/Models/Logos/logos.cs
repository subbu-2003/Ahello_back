namespace ahello_backend.Models.Logos
{
    public class LogoGetResponse
    {
        public int Id { get; set; }
        public string LogoName { get; set; }
        public string Logo { get; set; }

        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Pincode { get; set; }
        public string? MobileNumber { get; set; }

        public bool IsActive { get; set; }

        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }

    public class LogoPost
    {
        public string LogoName { get; set; }

        public IFormFile? LogoFile { get; set; }

        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Pincode { get; set; }
        public string? MobileNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public int? CreatedBy { get; set; }

        // Set by controller after file upload
        public string? LogoUrl { get; set; }
    }

    public class LogoPut
    {
        public string LogoName { get; set; }

        public IFormFile? LogoFile { get; set; }

        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Pincode { get; set; }
        public string? MobileNumber { get; set; }

        public bool IsActive { get; set; }

        public int? ModifiedBy { get; set; }

        public bool RemoveLogo { get; set; } = false;

        // Set by controller
        public string? LogoUrl { get; set; }
    }
    public class LogoIsActivePut
    {
        public bool IsActive { get; set; }
        public int? ModifiedBy { get; set; }
    }
}
