using ahello_backend.Models.Servicetype;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceTypeController : ControllerBase
    {
        private readonly IServiceTypeService _service;

        public ServiceTypeController(
            IServiceTypeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();

            return Ok(data);
        }

        [HttpGet("{serviceTypeId}")]
        public async Task<IActionResult> GetById(
            int serviceTypeId)
        {
            var data = await _service.GetByIdAsync(
                serviceTypeId);

            if (data == null)
                return NotFound("Service Type not found");

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            ServiceTypePost model)
        {
            var id = await _service.CreateAsync(model);

            return Ok(new
            {
                Success = true,
                ServiceTypeId = id
            });
        }

        [HttpPut("{serviceTypeId}")]
        public async Task<IActionResult> Update(
            int serviceTypeId,
            ServiceTypePut model)
        {
            var updated = await _service.UpdateAsync(
                serviceTypeId,
                model);

            if (!updated)
                return BadRequest("Update failed");

            return Ok(new
            {
                Success = true,
                Message = "Updated successfully"
            });
        }

        [HttpDelete("{serviceTypeId}")]
        public async Task<IActionResult> Delete(
            int serviceTypeId)
        {
            var deleted = await _service.DeleteAsync(
                serviceTypeId);

            if (!deleted)
                return BadRequest("Delete failed");

            return Ok(new
            {
                Success = true,
                Message = "Deleted successfully"
            });
        }
    }
}