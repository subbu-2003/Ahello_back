namespace ahello_backend.Models.PlatformSettings
{
    public class CreatePlatformSettingsRequest
    {
        public decimal FeePercentage { get; set; }

        public int? ModifiedBy { get; set; }
    }
}
