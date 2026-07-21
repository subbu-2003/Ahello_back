namespace ahello_backend.Models.PlatformSettings
{
    public class UpdatePlatformSettingsStatusRequest
    {
        public bool IsActive { get; set; }

        public int? ModifiedBy { get; set; }
    }
}
