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
            try
            {
                var id = await _service.CreateAsync(model);

                return Ok(new
                {
                    Success = true,
                    Message = "Form field created successfully",
                    FormFieldId = id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message == "Field Name already exists"
                        ? ex.Message
                        : "Invalid data"
                });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateFormField model)
        {
            try
            {
                var result = await _service.UpdateAsync(model);

                return Ok(new
                {
                    Success = result,
                    Message = "Form field updated successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message == "Field Name already exists"
                        ? ex.Message
                        : "Invalid data"
                });
            }
        }

        [HttpGet("form/{formId}")]
        public async Task<IActionResult> GetByForm(int formId)
        {
            try
            {
                var data = await _service.GetByFormAsync(formId);

                return Ok(data);
            }
            catch
            {
                return BadRequest(new
                {
                    Message = "Something went wrong"
                });
            }
        }

        [HttpGet("{formFieldId}")]
        public async Task<IActionResult> GetById(int formFieldId)
        {
            try
            {
                var data = await _service.GetByIdAsync(formFieldId);

                return Ok(data);
            }
            catch
            {
                return BadRequest(new
                {
                    Message = "Something went wrong"
                });
            }
        }

        [HttpDelete("{formFieldId}")]
        public async Task<IActionResult> Delete(
            int formFieldId,
            [FromQuery] string modifiedBy)
        {
            try
            {
                var result = await _service.DeleteAsync(
                    formFieldId,
                    modifiedBy);

                return Ok(new
                {
                    Success = result,
                    Message = "Form field deleted successfully"
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Message = "Something went wrong"
                });
            }
        }
    }
}