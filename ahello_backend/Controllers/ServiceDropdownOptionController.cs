using ahello_backend.Models.Service;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceDropdownOptionController
        : ControllerBase
    {
        private readonly IServiceDropdownOptionService
            _service;

        public ServiceDropdownOptionController(
            IServiceDropdownOptionService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            CreateServiceDropdownOption model)
        {
            var result = await _service
                .CreateAsync(model);

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(
            UpdateServiceDropdownOption model)
        {
            var result = await _service
                .UpdateAsync(model);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(
            int id)
        {
            var result = await _service
                .GetByIdAsync(id);

            return Ok(result);
        }

        [HttpGet("field/{serviceFieldId}")]
        public async Task<IActionResult> GetByFieldId(
            int serviceFieldId,
            [FromQuery] int? serviceId)
        {
            var result = await _service
                .GetByFieldIdAsync(
                    serviceFieldId,
                    serviceId);

            return Ok(result);
        }
    }
}
