using ahello_backend.Models.Form;
using ahello_backend.Models.Forms;
using ahello_backend.Services.Classes.ahello_backend.Services.Classes;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FormDynamicController
        : ControllerBase
    {
        private readonly IFormDynamicService _service;
        private readonly IWebHostEnvironment _env;
        private readonly FileUploadService _fileUpload;

        public FormDynamicController(IFormDynamicService service, IWebHostEnvironment env, FileUploadService fileUpload)
        {
            _service = service;
            _env = env;
            _fileUpload = fileUpload;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();

            return Ok(data);
        }

        [HttpGet("{formId}")]
        public async Task<IActionResult> GetById( int formId)
        {
            var data =
                await _service.GetByIdAsync(
                    formId
                    );

            return Ok(data);
        }
        [HttpGet("user/search")]
        public async Task<IActionResult> GetByUserId(
            [FromQuery] int userId,
            [FromQuery] string? searchText,
            [FromQuery] bool? isActive,
            [FromQuery] DateTime? date,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var model = new FormSearchRequest
            {
                UserId = userId,
                SearchText = searchText,
                IsActive = isActive,
                Date = date,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var data = await _service.GetByUserIdAsync(model);

            return Ok(data);
        }
        [HttpGet("user/search/active")]
        public async Task<IActionResult> GetActiveFormsByUserId(
        [FromQuery] int userId,
        [FromQuery] string? searchText,
        [FromQuery] DateTime? date,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            var model = new FormSearchRequest
            {
                UserId = userId,
                SearchText = searchText,
                Date = date,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var data = await _service.GetActiveFormsByUserIdAsync(model);

            return Ok(data);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] FormDynamicPost model)
        {
            try
            {
                var rootPath = _env.WebRootPath
                    ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                // Step 1 - File save first
                string? fileUrl = null;
                if (model.FieldFile != null && model.FieldFile.Length > 0)
                {
                    if (model.FieldFile.Length > 5 * 1024 * 1024)
                        return BadRequest(new { Success = false, Message = "File size must be less than 5 MB." });

                    var folderPath = Path.Combine(rootPath, "form-files");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.FieldFile.FileName)}";
                    var filePath = Path.Combine(folderPath, fileName);

                    await using var stream = new FileStream(filePath, FileMode.Create);
                    await model.FieldFile.CopyToAsync(stream);

                    fileUrl = $"/form-files/{fileName}";
                }

                // Step 2 - Fields build pannu from flat swagger values
                var dropdowns = new List<FormDropdownOptionPost>();

                if (!string.IsNullOrWhiteSpace(model.DropDownOptionsJson)
                    && model.DropDownOptionsJson != "string")
                {
                    try
                    {
                        dropdowns = System.Text.Json.JsonSerializer.Deserialize<List<FormDropdownOptionPost>>(
                            model.DropDownOptionsJson,
                            new System.Text.Json.JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            }) ?? new();
                    }
                    catch { dropdowns = new(); }
                }

                // Always build Fields from flat values — ignore swagger Fields array
                model.Fields = new List<FormDynamicFieldPost>
        {
            new FormDynamicFieldPost
            {
                FormFieldId = model.FormFieldId,
                FieldValue = fileUrl ?? model.FieldValue ?? "",
                DropDownOptions = dropdowns
            }
        };

                var id = await _service.CreateAsync(model);
                return Ok(new { Success = true, Message = "Form created successfully", FormId = id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
        [HttpPut("{formId}")]
        public async Task<IActionResult> Update(int formId, [FromForm] FormDynamicPut model)
        {
            try
            {
                var result = await _service.UpdateAsync(formId, model);

                return Ok(new
                {
                    Success = result,
                    Message = "Form updated successfully"
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

        [HttpDelete("{formId}")]
        public async Task<IActionResult> Delete( int formId)
        {
            var result =
                await _service.DeleteAsync(
                    formId);

            return Ok(new
            {
                Success = result,
                Message = "Form deleted successfully"
            });
        }

        [HttpPost("save-form-template")]
        public async Task<IActionResult> SaveFormTemplate(
    [FromBody] FormTemplatePost model)
        {
            var result =
                await _service.CreateFormTemplateAsync(model);

            return Ok(new
            {
                Success = true,
                FormId = result,
                Message = "Form Template Saved Successfully"
            });
        }

        [HttpGet("submitted/{formId}")]
        public async Task<IActionResult>GetSubmittedForm( int formId)
        {
            var result =
                await _service.GetSubmittedFormAsync(
                    formId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
        [HttpPut("update-form-template/{formId}")]
        public async Task<IActionResult> UpdateFormTemplate(
                int formId,
                [FromBody] FormTemplatePut model)
        {
            var result =
                await _service.UpdateFormTemplateAsync(formId, model);

            if (!result)
            {
                return NotFound(new
                {
                    Message = "Form not found"
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Form Template Updated Successfully"
            });
        }
        [HttpPost("submit-form")]
        public async Task<IActionResult> SubmitForm(
    [FromBody] FormSubmitPost model)
        {
            var result =
                await _service
                    .SubmitFormAsync(model);

            if (!result)
            {
                return BadRequest(new
                {
                    Message = "Failed to submit form"
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Form submitted successfully"
            });
        }
        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus(
    [FromBody] FormStatusUpdateRequest model)
        {
            var result =
                await _service.UpdateFormStatusAsync(model);

            if (!result)
            {
                return NotFound(new
                {
                    Message = "Form not found"
                });
            }

            return Ok(new
            {
                Message = "Form status updated successfully"
            });
        }
    }
}
