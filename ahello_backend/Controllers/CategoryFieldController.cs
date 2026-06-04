using ahello_backend.Models.Category;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryFieldController : ControllerBase
    {
        private readonly ICategoryFieldService _service;

        public CategoryFieldController(ICategoryFieldService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryField model)
        {
            var id = await _service.CreateAsync(model);

            return Ok(new
            {
                Success = true,
                Message = "Category field created successfully",
                CategoryFieldId = id
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateCategoryField model)
        {
            var result = await _service.UpdateAsync(model);

            return Ok(new
            {
                Success = result,
                Message = "Category field updated successfully"
            });
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var data = await _service.GetByCategoryAsync(categoryId);

            return Ok(data);
        }

        [HttpGet("{categoryFieldId}")]
        public async Task<IActionResult> GetById(int categoryFieldId)
        {
            var data = await _service.GetByIdAsync(categoryFieldId);

            return Ok(data);
        }

        [HttpDelete("{categoryFieldId}")]
        public async Task<IActionResult> Delete(
            int categoryFieldId,
            [FromQuery] string modifiedBy)
        {
            var result = await _service.DeleteAsync(
                categoryFieldId,
                modifiedBy
            );

            return Ok(new
            {
                Success = result,
                Message = "Category field deleted successfully"
            });
        }
    }
}