using ahello_backend.Models.Service;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceDynamicController : ControllerBase
    {
        private readonly IServiceDynamicService _service;
        private readonly IWebHostEnvironment _env;
        private readonly VideoValidationService _videoValidator;
        private readonly FileUploadService _fileUpload;

        public ServiceDynamicController(
             IServiceDynamicService service,
             IWebHostEnvironment env,
             VideoValidationService videoValidator,
             FileUploadService fileUpload)
        {
            _service = service;
            _env = env;
            _videoValidator = videoValidator;
            _fileUpload = fileUpload;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }

        [HttpGet("{serviceId}")]
        public async Task<IActionResult> GetById(int serviceId)
        {
            var data = await _service.GetByIdAsync(serviceId);
            if (data == null) return NotFound("Service not found");
            return Ok(data);
        }

        [HttpGet("user/{userId}/pagination")]
        public async Task<IActionResult> GetByUserIdPagination(
          int userId,
          int pageNumber = 1,
          int pageSize = 10,
          string? search = null,
          string? serviceCategoryName = null,
           string? serviceTypeName = null,
          string? status = null,
          bool? isActive = null)
        {
            var result = await _service.GetByUserIdPagedAsync(
                userId,
                pageNumber,
                pageSize,
                search,
                serviceCategoryName,
                serviceTypeName,
                status,
                isActive);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ServiceDynamicPost model)
        {
            if (!model.UserId.HasValue || model.UserId.Value <= 0)
            {
                return BadRequest(new { Success = false, Message = "User is required." });
            }

            if (!model.ServiceTypeId.HasValue || model.ServiceTypeId.Value <= 0)
            {
                return BadRequest(new { Success = false, Message = "Please select a service type." });
            }

            if (!model.ServiceCategoryId.HasValue || model.ServiceCategoryId.Value <= 0)
            {
                return BadRequest(new { Success = false, Message = "Please select a service category." });
            }

            if (!model.Price.HasValue || model.Price.Value <= 0)
            {
                return BadRequest(new { Success = false, Message = "Please enter a valid price." });
            }

            try
            {
                var rootPath = _env.WebRootPath
                    ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                if (model.ThumbnailImage != null && model.ThumbnailImage.Length > 0)
                {
                    if (model.ThumbnailImage.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest(new { Success = false, Message = "Thumbnail image size must be less than 5 MB." });
                    }

                    var folderPath = Path.Combine(rootPath, "service-thumbnails");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ThumbnailImage.FileName)}";
                    var filePath = Path.Combine(folderPath, fileName);

                    await using var stream = new FileStream(filePath, FileMode.Create);
                    await model.ThumbnailImage.CopyToAsync(stream);

                    model.ThumbnailImageUrl = $"/service-thumbnails/{fileName}";
                }

                if (model.BannerImage != null && model.BannerImage.Length > 0)
                {
                    if (model.BannerImage.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest(new { Success = false, Message = "Banner image size must be less than 5 MB." });
                    }

                    var folderPath = Path.Combine(rootPath, "service-banners");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.BannerImage.FileName)}";
                    var filePath = Path.Combine(folderPath, fileName);

                    await using var stream = new FileStream(filePath, FileMode.Create);
                    await model.BannerImage.CopyToAsync(stream);

                    model.BannerImageUrl = $"/service-banners/{fileName}";
                }

                // ── IntroVideo ─────────────────────────────────────────────
                if (model.IntroVideoFile != null && model.IntroVideoFile.Length > 0)
                {
                    if (model.IntroVideoFile.Length > 10 * 1024 * 1024)
                    {
                        return BadRequest(new { Success = false, Message = "Intro video size must be less than 10 MB." });
                    }

                    var (isValid, error) = _videoValidator.Validate(model.IntroVideoFile);
                    if (!isValid)
                        return BadRequest(new { Success = false, Message = error });

                    model.IntroVideo = await _fileUpload.SaveVideoAsync(model.IntroVideoFile);
                }

                var id = await _service.CreateAsync(model);

                return Ok(new
                {
                    Success = true,
                    ServiceId = id,
                    ThumbnailImageUrl = model.ThumbnailImageUrl,
                    BannerImageUrl = model.BannerImageUrl,
                    IntroVideoUrl = model.IntroVideo
                });
            }
            catch (Exception ex)
            {
                string message = ex.Message;

                if (message.Contains("Service Title already exists"))
                    message = "Service title already exists.";
                else if (message.Contains("FK_service_servicecategorydynamic"))
                    message = "Selected service category does not exist.";
                else if (message.Contains("FK_service_servicetype"))
                    message = "Selected service type does not exist.";
                else if (message.Contains("FK_service_user"))
                    message = "Selected user does not exist.";
                else if (message.Contains("foreign key constraint"))
                    message = "The selected information is invalid. Please verify and try again.";
                else
                    message = "Unable to save the service. Please try again.";

                // TEMP: surface real error while debugging — remove/guard behind env check later
                return StatusCode(500, new
                {
                    Success = false,
                    Message = message,
                    DebugError = ex.ToString()
                });
            }
        }

        [HttpPut("{serviceId}")]
        public async Task<IActionResult> Update(
            int serviceId,
            [FromForm] ServiceDynamicPut model)
        {
            if (!model.UserId.HasValue || model.UserId.Value <= 0)
            {
                return BadRequest(new { Success = false, Message = "User is required." });
            }

            if (!model.ServiceTypeId.HasValue || model.ServiceTypeId.Value <= 0)
            {
                return BadRequest(new { Success = false, Message = "Please select a service type." });
            }

            if (!model.ServiceCategoryId.HasValue || model.ServiceCategoryId.Value <= 0)
            {
                return BadRequest(new { Success = false, Message = "Please select a service category." });
            }

            if (!model.Price.HasValue || model.Price.Value <= 0)
            {
                return BadRequest(new { Success = false, Message = "Please enter a valid price." });
            }

            try
            {
                var rootPath = _env.WebRootPath
                    ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                // Fetch existing row ONCE — used for both replace and remove decisions
                var existing = await _service.GetByIdAsync(serviceId);
                if (existing == null)
                {
                    return NotFound(new { Success = false, Message = "Service not found." });
                }

                // ── Thumbnail ─────────────────────────────────────────────
                if (model.ThumbnailImage != null && model.ThumbnailImage.Length > 0)
                {
                    if (model.ThumbnailImage.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest(new { Success = false, Message = "Thumbnail image size must be less than 5 MB." });
                    }

                    if (!string.IsNullOrEmpty(existing.ThumbnailImage))
                    {
                        var oldPath = Path.Combine(rootPath, existing.ThumbnailImage.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    var folderPath = Path.Combine(rootPath, "service-thumbnails");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ThumbnailImage.FileName)}";
                    var filePath = Path.Combine(folderPath, fileName);

                    await using var stream = new FileStream(filePath, FileMode.Create);
                    await model.ThumbnailImage.CopyToAsync(stream);

                    model.ThumbnailImageUrl = $"/service-thumbnails/{fileName}";
                }
                else if (model.RemoveThumbnail)
                {
                    if (!string.IsNullOrEmpty(existing.ThumbnailImage))
                    {
                        var oldPath = Path.Combine(rootPath, existing.ThumbnailImage.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }
                    model.ThumbnailImageUrl = null;
                }
                else
                {
                    model.ThumbnailImageUrl = existing.ThumbnailImage; // keep unchanged
                }

                // ── Banner ────────────────────────────────────────────────
                if (model.BannerImage != null && model.BannerImage.Length > 0)
                {
                    if (model.BannerImage.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest(new { Success = false, Message = "Banner image size must be less than 5 MB." });
                    }

                    if (!string.IsNullOrEmpty(existing.BannerImage))
                    {
                        var oldPath = Path.Combine(rootPath, existing.BannerImage.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    var folderPath = Path.Combine(rootPath, "service-banners");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.BannerImage.FileName)}";
                    var filePath = Path.Combine(folderPath, fileName);

                    await using var stream = new FileStream(filePath, FileMode.Create);
                    await model.BannerImage.CopyToAsync(stream);

                    model.BannerImageUrl = $"/service-banners/{fileName}";
                }
                else if (model.RemoveBanner)
                {
                    if (!string.IsNullOrEmpty(existing.BannerImage))
                    {
                        var oldPath = Path.Combine(rootPath, existing.BannerImage.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }
                    model.BannerImageUrl = null;
                }
                else
                {
                    model.BannerImageUrl = existing.BannerImage; // keep unchanged
                }

                // ── IntroVideo ────────────────────────────────────────────
                if (model.IntroVideoFile != null && model.IntroVideoFile.Length > 0)
                {
                    if (model.IntroVideoFile.Length > 10 * 1024 * 1024)
                    {
                        return BadRequest(new { Success = false, Message = "Intro video size must be less than 10 MB." });
                    }

                    var (isValid, error) = _videoValidator.Validate(model.IntroVideoFile);
                    if (!isValid)
                        return BadRequest(new { Success = false, Message = error });

                    if (!string.IsNullOrEmpty(existing.IntroVideo))
                        _fileUpload.DeleteVideo(existing.IntroVideo);

                    model.IntroVideo = await _fileUpload.SaveVideoAsync(model.IntroVideoFile);
                }
                else if (model.RemoveIntroVideo)
                {
                    if (!string.IsNullOrEmpty(existing.IntroVideo))
                        _fileUpload.DeleteVideo(existing.IntroVideo);

                    model.IntroVideo = null;
                }
                else
                {
                    model.IntroVideo = existing.IntroVideo; // keep unchanged
                }

                var updated = await _service.UpdateAsync(serviceId, model);
                if (!updated) return BadRequest("Update failed");

                return Ok(new
                {
                    Success = true,
                    Message = "Updated successfully",
                    ThumbnailImageUrl = model.ThumbnailImageUrl,
                    BannerImageUrl = model.BannerImageUrl,
                    IntroVideoUrl = model.IntroVideo
                });
            }
            catch (Exception ex)
            {
                var error = ex.Message;

                if (error.Contains("Service Title already exists"))
                {
                    return Conflict(new { Success = false, Message = "Service title already exists.", DebugError = ex.ToString() });
                }

                if (error.Contains("FK_service_servicecategorydynamic"))
                {
                    return BadRequest(new { Success = false, Message = "Selected service category is invalid.", DebugError = ex.ToString() });
                }

                if (error.Contains("FK_service_servicetype"))
                {
                    return BadRequest(new { Success = false, Message = "Selected service type is invalid.", DebugError = ex.ToString() });
                }

                if (error.Contains("FK_service_user"))
                {
                    return BadRequest(new { Success = false, Message = "Selected user is invalid.", DebugError = ex.ToString() });
                }

                if (error.Contains("foreign key constraint"))
                {
                    return BadRequest(new { Success = false, Message = "Invalid service details. Please verify your input and try again.", DebugError = ex.ToString() });
                }

                // TEMP: surface real error while debugging — remove/guard behind env check later
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message,
                    DebugError = ex.ToString()
                });
            }
        }

        [HttpDelete("{serviceId}")]
        public async Task<IActionResult> Delete(int serviceId)
        {
            try
            {
                var deleted = await _service.DeleteAsync(serviceId);

                if (!deleted)
                {
                    return BadRequest(new { Success = false, Message = "Delete failed" });
                }

                return Ok(new { Success = true, Message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message, DebugError = ex.ToString() });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetAllPaged(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null)
        {
            var data = await _service.GetAllPagedAsync(pageNumber, pageSize, search);
            return Ok(data);
        }

        [HttpPut("{serviceId}/isactive")]
        public async Task<IActionResult> UpdateServiceIsActive(int serviceId, [FromBody] ServiceIsActivePut model)
        {
            var result = await _service.UpdateServiceIsActiveAsync(serviceId, model);

            if (!result)
            {
                return NotFound(new { Message = "Service not found" });
            }

            return Ok(new { Message = "Service status updated successfully" });
        }
    }
}