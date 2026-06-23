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
        private readonly VideoValidationService _videoValidator;   // ← add
        private readonly FileUploadService _fileUpload;            // ← add

        public ServiceDynamicController(
             IServiceDynamicService service,
             IWebHostEnvironment env,
             VideoValidationService videoValidator,                  // ← add
             FileUploadService fileUpload)                          // ← add
        {
            _service = service;
            _env = env;
            _videoValidator = videoValidator;                      // ← add
            _fileUpload = fileUpload;                              // ← add
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
          string? status = null,
          bool? isActive = null)
        {
            var result = await _service.GetByUserIdPagedAsync(
                userId,
                pageNumber,
                pageSize,
                search,
                serviceCategoryName,
                status,
                isActive);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ServiceDynamicPost model)
        {
            try
            {
            var rootPath = _env.WebRootPath
                ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            if (model.ThumbnailImage != null && model.ThumbnailImage.Length > 0)
            {
                if (model.ThumbnailImage.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Thumbnail image size must be less than 5 MB."
                    });
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
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Banner image size must be less than 5 MB."
                    });
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
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Intro video size must be less than 10 MB."
                    });
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
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message
                    
                });
            }
        }

        [HttpPut("{serviceId}")]
        public async Task<IActionResult> Update(
            int serviceId,
            [FromForm] ServiceDynamicPut model)
        {
            try { 
            var rootPath = _env.WebRootPath
                ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            if (model.ThumbnailImage != null && model.ThumbnailImage.Length > 0)
            {
                if (model.ThumbnailImage.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Thumbnail image size must be less than 5 MB."
                    });
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
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Banner image size must be less than 5 MB."
                    });
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
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Intro video size must be less than 10 MB."
                    });
                }
                var (isValid, error) = _videoValidator.Validate(model.IntroVideoFile);
                if (!isValid)
                    return BadRequest(new { Success = false, Message = error });

                // Delete old video file from disk before saving new one
                var existing = await _service.GetByIdAsync(serviceId);
                if (!string.IsNullOrEmpty(existing?.IntroVideo))
                    _fileUpload.DeleteVideo(existing.IntroVideo);

                model.IntroVideo = await _fileUpload.SaveVideoAsync(model.IntroVideoFile);
            }
            var updated = await _service.UpdateAsync(serviceId, model);
            if (!updated) return BadRequest("Update failed");

            return Ok(new
            {
                Success = true,
                Message = "Updated successfully",
                ThumbnailImageUrl = model.ThumbnailImageUrl,
                BannerImageUrl = model.BannerImageUrl,
                IntroVideoUrl     = model.IntroVideo
            });
        }
             
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message
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
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Delete failed"
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message
                    
                });
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
        public async Task<IActionResult> UpdateServiceIsActive(int serviceId,[FromBody] ServiceIsActivePut model)
        {
            var result =
                await _service.UpdateServiceIsActiveAsync(
                    serviceId,
                    model);

            if (!result)
            {
                return NotFound(new
                {
                    Message = "Service not found"
                });
            }

            return Ok(new
            {
                Message = "Service status updated successfully"
            });
        }
    }
}