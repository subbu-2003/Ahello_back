using ahello_backend.Services.Interfaces;
using ahello_backend.Models.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _service;

        public AdminController(IAdminService service)
        {
            _service = service;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginDto dto)
        {
            try
            {
                var token = await _service.LoginAsync(dto);

                if (token == null)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid username or password"
                    });
                }

                return Ok(new
                {
                    success = true,
                    token,
                    message = "Login successful"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] AdminCreateDto dto)
        {
            try
            {
                int adminId = await _service.CreateAdminAsync(dto);

                return Ok(new
                {
                    success = true,
                    adminId,
                    message = "Admin created successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }
    }
}
