using ahello_backend.Models.DigitalBook;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DigitalBookController : ControllerBase
    {
        private readonly IDigitalBookService _service;
        private readonly IWebHostEnvironment _environment;

        public DigitalBookController(
            IDigitalBookService service,
            IWebHostEnvironment environment)
        {
            _service = service;
            _environment = environment;
        }

        // POST: api/DigitalBook/create
        [HttpPost("create")]
        public async Task<IActionResult> Create(
            [FromForm] DigitalBook digitalBook,
            IFormFile? previewImage,
            IFormFile? pdfFile)
        {
            try
            {
                // IMAGE VALIDATION
                if (previewImage != null)
                {
                    var imageExtension =
                        Path.GetExtension(previewImage.FileName)
                            .ToLowerInvariant();

                    var allowedImageExtensions = new[]
                    {
                        ".jpg",
                        ".jpeg",
                        ".png",
                        ".webp"
                    };

                    if (!allowedImageExtensions.Contains(
                            imageExtension))
                    {
                        return BadRequest(new
                        {
                            message =
                                "Only JPG, JPEG, PNG and WEBP images are allowed."
                        });
                    }

                    digitalBook.PreviewImage =
                        await SaveFileAsync(
                            previewImage,
                            "uploads/digitalbooks/images");
                }

                // PDF VALIDATION
                if (pdfFile != null)
                {
                    var pdfExtension =
                        Path.GetExtension(pdfFile.FileName)
                            .ToLowerInvariant();

                    if (pdfExtension != ".pdf")
                    {
                        return BadRequest(new
                        {
                            message =
                                "Only PDF files are allowed."
                        });
                    }

                    digitalBook.PdfFile =
                        await SaveFileAsync(
                            pdfFile,
                            "uploads/digitalbooks/pdf");
                }

                var id =
                    await _service.CreateAsync(digitalBook);

                return Ok(new
                {
                    message =
                        "Digital book created successfully.",
                    digitalBookId = id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // PUT: api/DigitalBook/put
        [HttpPut("put")]
        public async Task<IActionResult> Update(
            [FromForm] DigitalBook digitalBook,
            IFormFile? previewImage,
            IFormFile? pdfFile)
        {
            try
            {
                // IMAGE
                if (previewImage != null)
                {
                    var imageExtension =
                        Path.GetExtension(previewImage.FileName)
                            .ToLowerInvariant();

                    var allowedImageExtensions = new[]
                    {
                        ".jpg",
                        ".jpeg",
                        ".png",
                        ".webp"
                    };

                    if (!allowedImageExtensions.Contains(
                            imageExtension))
                    {
                        return BadRequest(new
                        {
                            message =
                                "Only JPG, JPEG, PNG and WEBP images are allowed."
                        });
                    }

                    digitalBook.PreviewImage =
                        await SaveFileAsync(
                            previewImage,
                            "uploads/digitalbooks/images");
                }

                // PDF
                if (pdfFile != null)
                {
                    var pdfExtension =
                        Path.GetExtension(pdfFile.FileName)
                            .ToLowerInvariant();

                    if (pdfExtension != ".pdf")
                    {
                        return BadRequest(new
                        {
                            message =
                                "Only PDF files are allowed."
                        });
                    }

                    digitalBook.PdfFile =
                        await SaveFileAsync(
                            pdfFile,
                            "uploads/digitalbooks/pdf");
                }

                var result =
                    await _service.UpdateAsync(digitalBook);

                if (!result)
                {
                    return NotFound(new
                    {
                        message =
                            "Digital book not found."
                    });
                }

                return Ok(new
                {
                    message =
                        "Digital book updated successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // GET: api/DigitalBook
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result =
                    await _service.GetAllAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // GET: api/DigitalBook/User/User/5
        [HttpGet("User/User/{id}")]
        public async Task<IActionResult> GetByUserId(
            int id)
        {
            try
            {
                var result =
                    await _service.GetByUserIdAsync(id);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // DELETE: api/DigitalBook/isActive
        [HttpDelete("isActive")]
        public async Task<IActionResult> Deactivate(
            int digitalBookId,
            int modifiedBy)
        {
            try
            {
                var result =
                    await _service.DeactivateAsync(
                        digitalBookId,
                        modifiedBy);

                if (!result)
                {
                    return NotFound(new
                    {
                        message =
                            "Digital book not found or already inactive."
                    });
                }

                return Ok(new
                {
                    message =
                        "Digital book deactivated successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // GET:
        // api/DigitalBook/profile-by-slug?slug=user-slug
        [HttpGet("profile-by-slug")]
        public async Task<IActionResult> GetBySlug(
     string slug)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug))
                {
                    return BadRequest(new
                    {
                        message = "Slug is required."
                    });
                }

                var result =
                    await _service.GetBySlugAsync(slug);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // SAVE FILE
        private async Task<string> SaveFileAsync(
            IFormFile file,
            string folder)
        {
            var webRootPath =
                _environment.WebRootPath;

            if (string.IsNullOrEmpty(webRootPath))
            {
                webRootPath =
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot");
            }

            var uploadPath =
                Path.Combine(
                    webRootPath,
                    folder);

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var extension =
                Path.GetExtension(file.FileName);

            var fileName =
                $"{Guid.NewGuid()}{extension}";

            var fullPath =
                Path.Combine(
                    uploadPath,
                    fileName);

            using var stream =
                new FileStream(
                    fullPath,
                    FileMode.Create);

            await file.CopyToAsync(stream);

            return $"/{folder}/{fileName}"
                .Replace("\\", "/");
        }
        // GET: api/DigitalBook/admin/all
        // GET: api/DigitalBook/admin/all
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAdminDigitalBooks(
            [FromQuery] string? search,
            [FromQuery] string? approvalStatus,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1)
                {
                    return BadRequest(new
                    {
                        message = "PageNumber must be greater than 0."
                    });
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    return BadRequest(new
                    {
                        message = "PageSize must be between 1 and 100."
                    });
                }

                // APPROVAL STATUS VALIDATION
                if (!string.IsNullOrWhiteSpace(approvalStatus))
                {
                    approvalStatus = approvalStatus.Trim();

                    var allowedStatuses = new[]
                    {
                "Pending",
                "Approved",
                "Rejected"
            };

                    if (!allowedStatuses.Any(
                            x => x.Equals(
                                approvalStatus,
                                StringComparison.OrdinalIgnoreCase)))
                    {
                        return BadRequest(new
                        {
                            message =
                                "ApprovalStatus must be Pending, Approved, or Rejected."
                        });
                    }

                    // Normalize value
                    approvalStatus =
                        allowedStatuses.First(
                            x => x.Equals(
                                approvalStatus,
                                StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    approvalStatus = null;
                }

                var result =
                    await _service.GetAdminDigitalBooksAsync(
                        search,
                        approvalStatus,
                        pageNumber,
                        pageSize);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
        [HttpGet("admin/pending")]
        public async Task<IActionResult> GetPending()
        {
            try
            {
                var result = await _service.GetPendingAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPut("admin/approval/{digitalBookId}")]
        public async Task<IActionResult> UpdateApprovalStatus(
    int digitalBookId,
    [FromBody] DigitalBookApprovalRequest request)
        {
            try
            {
                if (request.AdminId <= 0)
                {
                    return BadRequest(new
                    {
                        message = "AdminId is required."
                    });
                }

                if (request.ApprovalStatus == DigitalBookApprovalStatus.Rejected &&
                    string.IsNullOrWhiteSpace(request.RejectionReason))
                {
                    return BadRequest(new
                    {
                        message = "Rejection reason is required when rejecting a digital book."
                    });
                }

                var result = await _service.UpdateApprovalStatusAsync(
                    digitalBookId,
                    request.AdminId,
                    request.ApprovalStatus,
                    request.RejectionReason);

                if (!result)
                {
                    return BadRequest(new
                    {
                        message =
                            "Digital book approval status cannot be changed. " +
                            "It may already be approved, rejected, published, or inactive."
                    });
                }

                if (request.ApprovalStatus == DigitalBookApprovalStatus.Approved)
                {
                    return Ok(new
                    {
                        message = "Digital book approved successfully.",
                        digitalBookId = digitalBookId,
                        status = "Draft",
                        approvalStatus = "Approved",
                        approvedBy = request.AdminId
                    });
                }

                return Ok(new
                {
                    message = "Digital book rejected successfully.",
                    digitalBookId = digitalBookId,
                    status = "Draft",
                    approvalStatus = "Rejected",
                    rejectionReason = request.RejectionReason
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
        [HttpPut("publish/{digitalBookId}")]
        public async Task<IActionResult> Publish(
        int digitalBookId,
        int userId)
        {
            try
            {
                var result = await _service.PublishAsync(
                    digitalBookId,
                    userId);

                if (!result)
                {
                    return BadRequest(new
                    {
                        message = "Digital book is currently under admin review. It cannot be published until the admin approves it.",
                        digitalBookId = digitalBookId,
                        status = "Draft",
                        approvalStatus = "Pending"
                    });
                }

                return Ok(new
                {
                    message = "Digital book published successfully.",
                    digitalBookId = digitalBookId,
                    status = "Published"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}