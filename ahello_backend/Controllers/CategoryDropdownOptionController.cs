using ahello_backend.Models.Category;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryDropdownOptionController
        : ControllerBase
    {
        private readonly
            ICategoryDropdownOptionService _service;

        public CategoryDropdownOptionController(
            ICategoryDropdownOptionService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            CreateCategoryDropdownOption model)
        {
            var rows = await _service.CreateAsync(model);

            return Ok(new
            {
                Success = true,
                RowsAffected = rows
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update(
            UpdateCategoryDropdownOption model)
        {
            var updated =
                await _service.UpdateAsync(model);

            if (!updated)
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

        [HttpGet("{optionId}")]
        public async Task<IActionResult> GetById(
            int optionId)
        {
            var data =
                await _service.GetByIdAsync(optionId);

            if (data == null)
            {
                return NotFound(new
                {
                    Message = "Data not found"
                });
            }

            return Ok(data);
        }

        [HttpGet("field/{categoryFieldId}")]
        public async Task<IActionResult> GetByFieldId(
            int categoryFieldId,
            int? categoryId)
        {
            var data =
                await _service.GetByFieldIdAsync(
                    categoryFieldId,
                    categoryId);

            return Ok(data);
        }
    }
}