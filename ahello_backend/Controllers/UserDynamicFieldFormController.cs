using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserDynamicFieldFormController : ControllerBase
    {
        private readonly IUserDynamicFieldFormService _service;

        public UserDynamicFieldFormController(
            IUserDynamicFieldFormService service)
        {
            _service = service;
        }

        // GET FORM
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetForm(int userId)
        {
            try
            {
                if (userId <= 0)
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Invalid UserId"
                    });

                var result =
                    await _service
                    .GetUserDynamicFieldFormAsync(userId);

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
