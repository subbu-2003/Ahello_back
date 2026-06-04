using ahello_backend.Models.Service;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceFieldController : ControllerBase
    {
        private readonly IServiceFieldService _service;

        public ServiceFieldController(IServiceFieldService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateServiceField model)
        {
            var id = await _service.CreateAsync(model);

            return Ok(new
            {
                Success = true,
                ServiceFieldId = id
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateServiceField model)
        {
            var updated = await _service.UpdateAsync(model);

            if (!updated)
                return BadRequest("Update failed");

            return Ok(new
            {
                Success = true,
                Message = "Updated successfully"
            });
        }

        [HttpGet("service/{serviceId}")]
        public async Task<IActionResult> GetByService(int serviceId)
        {
            var data = await _service.GetByServiceAsync(serviceId);

            return Ok(data);
        }

        [HttpGet("{serviceFieldId}")]
        public async Task<IActionResult> GetById(int serviceFieldId)
        {
            var data = await _service.GetByIdAsync(serviceFieldId);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        [HttpDelete("{serviceFieldId}")]
        public async Task<IActionResult> Delete(
            int serviceFieldId,
            [FromQuery] string modifiedBy)
        {
            var deleted = await _service.DeleteAsync(
                serviceFieldId,
                modifiedBy);

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