using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceDynamicFieldFormController : ControllerBase
    {
        private readonly IServiceDynamicFieldFormService _service;

        public ServiceDynamicFieldFormController(
            IServiceDynamicFieldFormService service)
        {
            _service = service;
        }

        // GET FORM
        [HttpGet("{serviceId}")]
        public async Task<IActionResult> GetForm(int serviceId)
        {
            try
            {
                if (serviceId <= 0)
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Invalid ServiceId"
                    });

                var result =
                    await _service
                    .GetServiceDynamicFieldFormAsync(serviceId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}