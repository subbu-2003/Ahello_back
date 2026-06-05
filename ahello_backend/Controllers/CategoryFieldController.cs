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

        public CategoryFieldController(
            ICategoryFieldService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryField model)
        {
            try
            {
                var id = await _service.CreateAsync(model);

                return Ok(new
                {
                    Success = true,
                    Message = "Category field created successfully",
                    CategoryFieldId = id
                });
            }
            catch (Exception ex)
            {
                if (ex.Message == "Field Name already exists")
                {
                    return BadRequest(new
                    {
                        Message = ex.Message
                    });
                }

                return BadRequest(new
                {
                    Message = "Invalid data"
                });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateCategoryField model)
        {
            try
            {
                var result = await _service.UpdateAsync(model);

                return Ok(new
                {
                    Success = result,
                    Message = "Category field updated successfully"
                });
            }
            catch (Exception ex)
            {
                if (ex.Message == "Field Name already exists")
                {
                    return BadRequest(new
                    {
                        Message = ex.Message
                    });
                }

                return BadRequest(new
                {
                    Message = "Invalid data"
                });
            }
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            try
            {
                var data = await _service.GetByCategoryAsync(categoryId);

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

        [HttpGet("{categoryFieldId}")]
        public async Task<IActionResult> GetById(int categoryFieldId)
        {
            try
            {
                var data = await _service.GetByIdAsync(categoryFieldId);

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

        [HttpDelete("{categoryFieldId}")]
        public async Task<IActionResult> Delete(
            int categoryFieldId,
            [FromQuery] string modifiedBy)
        {
            try
            {
                var result = await _service.DeleteAsync(
                    categoryFieldId,
                    modifiedBy);

                return Ok(new
                {
                    Success = result,
                    Message = "Category field deleted successfully"
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