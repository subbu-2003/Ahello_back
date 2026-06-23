using ahello_backend.Models.Category;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryDynamicController : ControllerBase
    {
        private readonly ICategoryDynamicService _service;

        public CategoryDynamicController(ICategoryDynamicService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
        int pageNumber = 1,
        int pageSize = 10,
        string? search = null,
        DateTime? createdDate = null, bool? isActive = null)
        {
            var data = await _service.GetAllAsync(
                pageNumber,
                pageSize,
                search,
                createdDate, isActive);

            return Ok(data);
        }
        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetById(int categoryId)
        {
            var data = await _service.GetByIdAsync(categoryId);

            if (data == null)
                return NotFound("Category not found");

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryDynamicPost model)
        {
            var id = await _service.CreateAsync(model);

            return Ok(new
            {
                Success = true,
                CategoryId = id
            });
        }

        [HttpPut("{categoryId}")]
        public async Task<IActionResult> Update(int categoryId, CategoryDynamicPut model)
        {
            var updated = await _service.UpdateAsync(categoryId, model);

            if (!updated)
                return BadRequest("Update failed");

            return Ok(new
            {
                Success = true,
                Message = "Updated successfully"
            });
        }

        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> Delete(int categoryId)
        {
            var deleted = await _service.DeleteAsync(categoryId);

            if (!deleted)
                return BadRequest("Delete failed");

            return Ok(new
            {
                Success = true,
                Message = "Deleted successfully"
            });
        }
        [HttpGet("paged")]
        public async Task<IActionResult> GetAllPaged(
        int pageNumber = 1,
        int pageSize = 10, string? search = null)
        {
            var data = await _service.GetAllPagedAsync(pageNumber, pageSize, search);

            return Ok(data);
        }
        [HttpPut("{categoryId}/status")]
        public async Task<IActionResult> UpdateStatus(int categoryId, CategoryStatusUpdateRequest model)
        {
            var updated = await _service.UpdateStatusAsync(
                categoryId,
                model.IsActive,
                model.ModifiedBy);

            if (!updated)
                return NotFound("Category not found");

            return Ok(new
            {
                Success = true,
                Message = "Category status updated successfully"
            });
        }
    }
}