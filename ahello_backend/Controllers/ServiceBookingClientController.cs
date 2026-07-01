using Microsoft.AspNetCore.Mvc;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceBookingClientController : ControllerBase
    {
        private readonly IServiceBookingClientService _service;

        public ServiceBookingClientController(IServiceBookingClientService service)
        {
            _service = service;
        }

        [HttpGet("clients/{userId}")]
        public async Task<IActionResult> GetClientsByUserId(
            int userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var result = await _service.GetClientsByUserIdAsync(
                userId,
                pageNumber,
                pageSize,
                search);

            return Ok(new
            {
                success = true,
                message = "Clients fetched successfully",
                data = result
            });
        }
    }
}