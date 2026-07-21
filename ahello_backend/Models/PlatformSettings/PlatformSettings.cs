namespace ahello_backend.Models.PlatformSettings
{
    public class PlatformSettings
    {
        public int PlatformSettingId { get; set; }

        public decimal FeePercentage { get; set; }

        public bool IsActive { get; set; }

        public DateTime? ModifiedAt { get; set; }

        public int? ModifiedBy { get; set; }
    }
}
