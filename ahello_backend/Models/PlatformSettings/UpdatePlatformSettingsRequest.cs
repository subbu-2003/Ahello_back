namespace ahello_backend.Models.PlatformSettings
{
    public class UpdatePlatformSettingsRequest
    {
        public string FeeType { get; set; } = "percentage";

        public decimal? FeePercentage { get; set; }

        public decimal? FeeAmount { get; set; }

        public int? ModifiedBy { get; set; }
    }
}
