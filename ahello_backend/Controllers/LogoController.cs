using ahello_backend.Models.Logos;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogoController : ControllerBase
    {
        private readonly ILogoService _service;
        private readonly IWebHostEnvironment _env;

        public LogoController(
            ILogoService service,
            IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        // =========================================================
        // GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _service.GetAllAsync();

                return Ok(new
                {
                    Success = true,
                    Data = data
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
        [HttpGet("isactive")]
        public async Task<IActionResult> GetByIsActive()
        {
            try
            {
                var data = await _service.GetByIsActiveAsync();

                return Ok(new
                {
                    Success = true,
                    Data = data
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
        // =========================================================
        // POST
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromForm] LogoPost model)
        {
            if (string.IsNullOrWhiteSpace(model.LogoName))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Logo name is required."
                });
            }

            if (model.LogoFile == null ||
                model.LogoFile.Length == 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Logo image is required."
                });
            }

            try
            {
                var rootPath = _env.WebRootPath
                    ?? Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot");

                // =====================================================
                // LOGO UPLOAD
                // =====================================================

                if (model.LogoFile != null &&
                    model.LogoFile.Length > 0)
                {
                    // 5 MB limit
                    if (model.LogoFile.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest(new
                        {
                            Success = false,
                            Message = "Logo image size must be less than 5 MB."
                        });
                    }

                    var folderPath = Path.Combine(
                        rootPath,
                        "logos");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    var extension =
                        Path.GetExtension(model.LogoFile.FileName);

                    var fileName =
                        $"{Guid.NewGuid()}{extension}";

                    var filePath =
                        Path.Combine(folderPath, fileName);

                    await using var stream =
                        new FileStream(
                            filePath,
                            FileMode.Create);

                    await model.LogoFile.CopyToAsync(stream);

                    // Save only URL/path in database
                    model.LogoUrl =
                        $"/logos/{fileName}";
                }

                var id = await _service.CreateAsync(model);

                return Ok(new
                {
                    Success = true,
                    Message = "Logo created successfully.",
                    Id = id,
                    LogoUrl = model.LogoUrl
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

        // =========================================================
        // PUT
        // =========================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromForm] LogoPut model)
        {
            if (string.IsNullOrWhiteSpace(model.LogoName))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Logo name is required."
                });
            }

            try
            {
                var rootPath = _env.WebRootPath
                    ?? Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot");

                // =====================================================
                // GET EXISTING LOGO
                // =====================================================

                var allLogos =
                    await _service.GetAllAsync();

                var existing =
                    allLogos.FirstOrDefault(x => x.Id == id);

                if (existing == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Logo not found."
                    });
                }

                // =====================================================
                // NEW LOGO IMAGE
                // =====================================================

                if (model.LogoFile != null &&
                    model.LogoFile.Length > 0)
                {
                    if (model.LogoFile.Length >
                        5 * 1024 * 1024)
                    {
                        return BadRequest(new
                        {
                            Success = false,
                            Message = "Logo image size must be less than 5 MB."
                        });
                    }

                    // Delete old logo
                    if (!string.IsNullOrEmpty(existing.Logo))
                    {
                        var oldPath = Path.Combine(
                            rootPath,
                            existing.Logo.TrimStart('/'));

                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    // Logo folder
                    var folderPath = Path.Combine(
                        rootPath,
                        "logos");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    // Generate unique filename
                    var extension =
                        Path.GetExtension(
                            model.LogoFile.FileName);

                    var fileName =
                        $"{Guid.NewGuid()}{extension}";

                    var filePath =
                        Path.Combine(
                            folderPath,
                            fileName);

                    await using var stream =
                        new FileStream(
                            filePath,
                            FileMode.Create);

                    await model.LogoFile.CopyToAsync(stream);

                    model.LogoUrl =
                        $"/logos/{fileName}";
                }

                // =====================================================
                // REMOVE LOGO
                // =====================================================

                else if (model.RemoveLogo)
                {
                    if (!string.IsNullOrEmpty(existing.Logo))
                    {
                        var oldPath = Path.Combine(
                            rootPath,
                            existing.Logo.TrimStart('/'));

                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    model.LogoUrl = null;
                }

                // =====================================================
                // KEEP EXISTING LOGO
                // =====================================================

                else
                {
                    model.LogoUrl = existing.Logo;
                }

                var updated =
                    await _service.UpdateAsync(
                        id,
                        model);

                if (!updated)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Logo update failed."
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Logo updated successfully.",
                    Id = id,
                    LogoUrl = model.LogoUrl
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
        [HttpPut("{id}/isactive")]
        public async Task<IActionResult> UpdateLogoIsActive(
    int id,
    [FromBody] LogoIsActivePut model)
        {
            try
            {
                var result =
                    await _service.UpdateLogoIsActiveAsync(
                        id,
                        model);

                if (!result)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Logo not found."
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = model.IsActive
                        ? "Logo activated successfully."
                        : "Logo deactivated successfully."
                });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains(
                    "An active logo already exists"))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = ex.Message
                    });
                }

                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}