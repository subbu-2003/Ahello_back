using ahello_backend.Models.PlatformSettings;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class PlatformSettingsService : IPlatformSettingsService
    {
        private readonly IPlatformSettingsRepository _repository;

        public PlatformSettingsService(
            IPlatformSettingsRepository repository)
        {
            _repository = repository;
        }

        // ============================================================
        // GET ALL
        // ============================================================

        public async Task<IEnumerable<PlatformSettings>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        // ============================================================
        // GET BY ID
        // ============================================================

        public async Task<PlatformSettings?> GetByIdAsync(
            int platformSettingId)
        {
            return await _repository.GetByIdAsync(
                platformSettingId);
        }

        // ============================================================
        // POST
        // ============================================================

        public async Task<int> CreateAsync(
            CreatePlatformSettingsRequest request)
        {
            return await _repository.CreateAsync(request);
        }

        // ============================================================
        // PUT
        // ============================================================

        public async Task<bool> UpdateAsync(
            int platformSettingId,
            UpdatePlatformSettingsRequest request)
        {
            return await _repository.UpdateAsync(
                platformSettingId,
                request);
        }

        // ============================================================
        // PUT - IS ACTIVE
        // ============================================================

        public async Task<bool> UpdateIsActiveAsync(
            int platformSettingId,
            UpdatePlatformSettingsStatusRequest request)
        {
            return await _repository.UpdateIsActiveAsync(
                platformSettingId,
                request);
        }
    }
}