using ahello_backend.Models.Form;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FormDropdownOptionController
        : ControllerBase
    {
        private readonly IFormDropdownOptionService
            _service;

        public FormDropdownOptionController(
            IFormDropdownOptionService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            CreateFormDropdownOption model)
        {
            var result = await _service.CreateAsync(model);

            return Ok(new
            {
                Success = true,
                Message =
                    "Form dropdown options created successfully",
                RowsAffected = result
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update(
            UpdateFormDropdownOption model)
        {
            var result = await _service.UpdateAsync(model);

            return Ok(new
            {
                Success = result,
                Message =
                    "Form dropdown option updated successfully"
            });
        }

        [HttpGet("{optionId}")]
        public async Task<IActionResult> GetById(
            int optionId)
        {
            var data = await _service.GetByIdAsync(optionId);

            return Ok(data);
        }

        [HttpGet("field/{formFieldId}")]
        public async Task<IActionResult> GetByFieldId(
            int formFieldId,
            [FromQuery] int? formId)
        {
            var data = await _service.GetByFieldIdAsync(
                formFieldId,
                formId);

            return Ok(data);
        }
    }
}