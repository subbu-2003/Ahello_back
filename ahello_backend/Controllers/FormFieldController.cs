using ahello_backend.Models.Form;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FormFieldController : ControllerBase
    {
        private readonly IFormFieldService _service;

        public FormFieldController(IFormFieldService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateFormField model)
        {
            var id = await _service.CreateAsync(model);

            return Ok(new
            {
                Success = true,
                Message = "Form field created successfully",
                FormFieldId = id
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateFormField model)
        {
            var result = await _service.UpdateAsync(model);

            return Ok(new
            {
                Success = result,
                Message = "Form field updated successfully"
            });
        }

        [HttpGet("form/{formId}")]
        public async Task<IActionResult> GetByForm(int formId)
        {
            var data = await _service.GetByFormAsync(formId);

            return Ok(data);
        }

        [HttpGet("{formFieldId}")]
        public async Task<IActionResult> GetById(int formFieldId)
        {
            var data = await _service.GetByIdAsync(formFieldId);

            return Ok(data);
        }

        [HttpDelete("{formFieldId}")]
        public async Task<IActionResult> Delete(
            int formFieldId,
            [FromQuery] string modifiedBy)
        {
            var result = await _service.DeleteAsync(
                formFieldId,
                modifiedBy
            );

            return Ok(new
            {
                Success = result,
                Message = "Form field deleted successfully"
            });
        }
    }
}
