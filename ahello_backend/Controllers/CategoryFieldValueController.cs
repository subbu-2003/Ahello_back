using ahello_backend.Models.Category;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryFieldValueController : ControllerBase
    {
        private readonly ICategoryFieldValueService _service;

        public CategoryFieldValueController(
            ICategoryFieldValueService service)
        {
            _service = service;
        }

        // POST
        [HttpPost]
        public async Task<IActionResult> Create(
            CreateCategoryFieldValue model)
        {
            var id = await _service.CreateAsync(model);

            return Ok(new
            {
                Success = true,
                CategoryFieldValueId = id
            });
        }

        // PUT
        [HttpPut]
        public async Task<IActionResult> Update(
            UpdateCategoryFieldValue model)
        {
            bool success = await _service.UpdateAsync(model);

            if (!success)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Update failed"
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Updated successfully"
            });
        }

        // GET BY CATEGORY
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var data = await _service.GetByCategoryAsync(categoryId);

            return Ok(data);
        }

        // GET BY ID
        [HttpGet("{categoryFieldValueId}")]
        public async Task<IActionResult> GetById(
            int categoryFieldValueId)
        {
            var data = await _service.GetByIdAsync(categoryFieldValueId);

            if (data == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Data not found"
                });
            }

            return Ok(data);
        }
    }
}