using ahello_backend.Models.Category;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_AHLLO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound("Category not found");

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryCreate model)
        {
            var result = await _service.CreateAsync(model);

            return Ok(new
            {
                Message = "Category created successfully",
                Result = result
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update(CategoryUpdate model)
        {
            var result = await _service.UpdateAsync(model);

            return Ok(new
            {
                Message = "Category updated successfully",
                Result = result
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            return Ok(new
            {
                Message = "Category deleted successfully",
                Result = result
            });
        }
    }
}
