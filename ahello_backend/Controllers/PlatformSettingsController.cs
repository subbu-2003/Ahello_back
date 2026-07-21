using ahello_backend.Models.PlatformSettings;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlatformSettingsController : ControllerBase
    {
        private readonly IPlatformSettingsService _service;

        public PlatformSettingsController(
            IPlatformSettingsService service)
        {
            _service = service;
        }

        // ============================================================
        // GET ALL
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();

            return Ok(result);
        }

        // ============================================================
        // GET BY PLATFORM SETTING ID
        // ============================================================

        [HttpGet("{platformSettingId}")]
        public async Task<IActionResult> GetById(
            int platformSettingId)
        {
            var result = await _service.GetByIdAsync(
                platformSettingId);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Platform setting not found."
                });
            }

            return Ok(result);
        }

        // ============================================================
        // POST
        // ============================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreatePlatformSettingsRequest request)
        {
            var id = await _service.CreateAsync(request);

            return Ok(new
            {
                message = "Platform setting created successfully.",
                platformSettingId = id
            });
        }

        // ============================================================
        // PUT - UPDATE FEE PERCENTAGE
        // ============================================================

        [HttpPut("{platformSettingId}")]
        public async Task<IActionResult> Update(
            int platformSettingId,
            [FromBody] UpdatePlatformSettingsRequest request)
        {
            var result = await _service.UpdateAsync(
                platformSettingId,
                request);

            if (!result)
            {
                return NotFound(new
                {
                    message = "Platform setting not found."
                });
            }

            return Ok(new
            {
                message = "Platform setting updated successfully."
            });
        }

        // ============================================================
        // PUT - UPDATE IS ACTIVE
        // ============================================================

        [HttpPut("{platformSettingId}/is-active")]
        public async Task<IActionResult> UpdateIsActive(
            int platformSettingId,
            [FromBody] UpdatePlatformSettingsStatusRequest request)
        {
            var result = await _service.UpdateIsActiveAsync(
                platformSettingId,
                request);

            if (!result)
            {
                return NotFound(new
                {
                    message = "Platform setting not found."
                });
            }

            return Ok(new
            {
                message = "Platform setting status updated successfully."
            });
        }
    }
}
