using ahello_backend.Models.Service;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceFieldValueController : ControllerBase
    {
        private readonly IServiceFieldValueService _service;

        public ServiceFieldValueController(
            IServiceFieldValueService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            CreateServiceFieldValue model)
        {
            var id = await _service.CreateAsync(model);

            return Ok(new
            {
                Success = true,
                ServiceFieldValueId = id
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update(
            UpdateServiceFieldValue model)
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

        [HttpGet("{serviceFieldValueId}")]
        public async Task<IActionResult> GetById(int serviceFieldValueId)
        {
            var data = await _service.GetByIdAsync(serviceFieldValueId);

            if (data == null)
                return NotFound();

            return Ok(data);
        }
    }
}
