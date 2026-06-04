using ahello_backend.Models.Form;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FormController : ControllerBase
    {
        private readonly IFormService _formService;

        public FormController(IFormService formService)
        {
            _formService = formService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _formService.GetAllAsync();

            return Ok(result);
        }

        [HttpGet("{formId}")]
        public async Task<IActionResult> GetById(int formId)
        {
            var result = await _formService.GetByIdAsync(formId);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var result = await _formService.GetByUserIdAsync(userId);

            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> Create(FormCreate model)
        {
            var result = await _formService.CreateAsync(model);

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(FormUpdate model)
        {
            var result = await _formService.UpdateAsync(model);

            return Ok(result);
        }

        [HttpDelete("{formId}")]
        public async Task<IActionResult> Delete(int formId)
        {
            var result = await _formService.DeleteAsync(formId);

            return Ok(result);
        }
    }
}