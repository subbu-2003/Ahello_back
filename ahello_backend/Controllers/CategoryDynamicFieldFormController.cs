using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryDynamicFieldFormController : ControllerBase
    {
        private readonly ICategoryDynamicFieldFormService _service;

        public CategoryDynamicFieldFormController(
            ICategoryDynamicFieldFormService service)
        {
            _service = service;
        }

        // GET FORM
        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetForm(int categoryId)
        {
            try
            {
                if (categoryId <= 0)
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Invalid CategoryId"
                    });

                var result =
                    await _service.GetCategoryDynamicFieldFormAsync(categoryId);

                return Ok(result);
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
    }
}