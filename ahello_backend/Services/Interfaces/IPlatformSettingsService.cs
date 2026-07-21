using ahello_backend.Models.PlatformSettings;

namespace ahello_backend.Services.Interfaces
{
    public interface IPlatformSettingsService
    {
        Task<IEnumerable<PlatformSettings>> GetAllAsync();

        Task<PlatformSettings?> GetByIdAsync(
            int platformSettingId);

        Task<int> CreateAsync(
            CreatePlatformSettingsRequest request);

        Task<bool> UpdateAsync(
            int platformSettingId,
            UpdatePlatformSettingsRequest request);

        Task<bool> UpdateIsActiveAsync(
            int platformSettingId,
            UpdatePlatformSettingsStatusRequest request);
    }
}
