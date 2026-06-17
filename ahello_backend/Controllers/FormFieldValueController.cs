using ahello_backend.Models.Form;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FormFieldValueController : ControllerBase
    {
        private readonly IFormFieldValueService _service;
        private readonly FileUploadService _fileUpload;

        public FormFieldValueController(
            IFormFieldValueService service, FileUploadService fileUpload)
        {
            _service = service;
            _fileUpload = fileUpload;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateFormFieldValue model)
        {
            if (model.File != null && model.File.Length > 0)
            {
                model.FieldValue =
                    await _fileUpload.SaveFileAsync(model.File, "form-files");
            }

            var id = await _service.CreateAsync(model);

            return Ok(new
            {
                Success = true,
                Message = "Form field value created successfully",
                FormFieldValueId = id
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update(
            UpdateFormFieldValue model)
        {
            var result = await _service.UpdateAsync(model);

            return Ok(new
            {
                Success = result,
                Message = "Form field value updated successfully"
            });
        }

        [HttpGet("form/{formId}")]
        public async Task<IActionResult> GetByForm(int formId)
        {
            var data = await _service.GetByFormAsync(formId);

            return Ok(data);
        }

        [HttpGet("{formFieldValueId}")]
        public async Task<IActionResult> GetById(
            int formFieldValueId)
        {
            var data = await _service.GetByIdAsync(
                formFieldValueId);

            return Ok(data);
        }
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(
            int userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            try
            {
                var result = await _service.GetByUserIdAsync(
                    userId,
                    pageNumber,
                    pageSize,
                    search);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}