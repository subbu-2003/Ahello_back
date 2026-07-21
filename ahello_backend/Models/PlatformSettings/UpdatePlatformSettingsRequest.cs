namespace ahello_backend.Models.PlatformSettings
{
    public class UpdatePlatformSettingsRequest
    {
        public decimal FeePercentage { get; set; }

        public int? ModifiedBy { get; set; }
    }
}
