using ahello_backend.Models.Service;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IServiceService _service;

        public ServiceController(
            IServiceService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();

            return Ok(data);
        }

        [HttpGet("{serviceId}")]
        public async Task<IActionResult> GetById(
            int serviceId)
        {
            var data = await _service.GetByIdAsync(
                serviceId);

            if (data == null)
                return NotFound("Service not found");

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            ServicePost model)
        {
            var id = await _service.CreateAsync(model);

            return Ok(new
            {
                Success = true,
                ServiceId = id
            });
        }

        [HttpPut("{serviceId}")]
        public async Task<IActionResult> Update(
            int serviceId,
            ServicePut model)
        {
            var updated = await _service.UpdateAsync(
                serviceId,
                model);

            if (!updated)
                return BadRequest("Update failed");

            return Ok(new
            {
                Success = true,
                Message = "Updated successfully"
            });
        }

        [HttpDelete("{serviceId}")]
        public async Task<IActionResult> Delete(
            int serviceId)
        {
            var deleted = await _service.DeleteAsync(
                serviceId);

            if (!deleted)
                return BadRequest("Delete failed");

            return Ok(new
            {
                Success = true,
                Message = "Deleted successfully"
            });
        }
        [HttpGet("category/{serviceCategoryId}")]
        public async Task<IActionResult> GetByServiceCategoryId(
         int serviceCategoryId)
        {
            var result = await _service.GetByServiceCategoryIdAsync(
                serviceCategoryId);

            return Ok(result);
        }
    }
}