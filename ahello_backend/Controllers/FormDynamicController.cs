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

        public FormDynamicController(IFormDynamicService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();

            return Ok(data);
        }

        [HttpGet("{formId}/{clientId}")]
        public async Task<IActionResult> GetById( int formId, int clientId)
        {
            var data =
                await _service.GetByIdAsync(
                    formId,
                    clientId);

            return Ok(data);
        }
        [HttpGet("user/{userId}/{clientId}")]
        public async Task<IActionResult> GetByUserId(int userId, int clientId)
        {
            var data =
                await _service.GetByUserIdAsync(
                    userId,
                    clientId);

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(FormDynamicPost model)
        {
            var id = await _service.CreateAsync(model);

            return Ok(new
            {
                Success = true,
                Message = "Form created successfully",
                FormId = id
            });
        }

        [HttpPut("{formId}")]
        public async Task<IActionResult> Update(int formId, FormDynamicPut model)
        {
            var result = await _service.UpdateAsync(formId, model);

            return Ok(new
            {
                Success = result,
                Message = "Form updated successfully"
            });
        }

        [HttpDelete("{formId}/{clientId}")]
        public async Task<IActionResult> Delete( int formId,int clientId)
        {
            var result =
                await _service.DeleteAsync(
                    formId,
                    clientId);

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

        [HttpGet("submitted/{formId}/{clientId}")]
        public async Task<IActionResult>GetSubmittedForm( int formId, int clientId)
        {
            var result =
                await _service.GetSubmittedFormAsync(
                    formId,
                    clientId);

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
    }
}
