using ahello_backend.Models.Servicecategory;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceCategoryDynamicValueController
        : ControllerBase
    {
        private readonly
            IServiceCategoryDynamicValueService _service;

        public ServiceCategoryDynamicValueController(
            IServiceCategoryDynamicValueService service)
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
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] ServiceCategoryDynamicPost model)
        {
            var id = await _service.CreateAsync(model);

            return Ok(new
            {
                Message = "Created Successfully",
                ServiceCategoryId = id
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] ServiceCategoryDynamicPut model)
        {
            var result = await _service.UpdateAsync(
                id,
                model);

            if (!result)
                return NotFound();

            return Ok(new
            {
                Message = "Updated Successfully"
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound();

            return Ok(new
            {
                Message = "Deleted Successfully"
            });
        }
    }
}
