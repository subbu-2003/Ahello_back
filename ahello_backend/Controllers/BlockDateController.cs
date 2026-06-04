using ahello_backend.Models.Blockdate;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlockDateController : ControllerBase
    {
        private readonly IBlockDateService _service;

        public BlockDateController(
            IBlockDateService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            BlockDatePost model)
        {
            var id = await _service.Post(model);

            return Ok(id);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();

            return Ok(result);
        }

        [HttpGet("{blockDateId}")]
        public async Task<IActionResult> GetById(
            int blockDateId)
        {
            var result =
                await _service.GetById(blockDateId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(
            BlockDatePut model)
        {
            var result =
                await _service.Update(model);

            return Ok(result);
        }

        [HttpDelete("{blockDateId}")]
        public async Task<IActionResult> Delete(
            int blockDateId)
        {
            var result =
                await _service.Delete(blockDateId);

            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(
            int userId)
        {
            var result =
                await _service.GetByUserId(userId);

            return Ok(result);
        }

        [HttpGet("service/{serviceId}")]
        public async Task<IActionResult> GetByServiceId(
            int serviceId)
        {
            var result =
                await _service.GetByServiceId(serviceId);

            return Ok(result);
        }
    }
}