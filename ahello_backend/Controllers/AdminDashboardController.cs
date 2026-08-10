using ahello_backend.Models.Admin;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _adminDashboardService;

        public AdminDashboardController(
            IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<AdminDashboardModel>> GetAll()
        {
            try
            {
                var result = await _adminDashboardService
                    .GetAdminDashboardAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "An error occurred while retrieving the admin dashboard.",
                        error = ex.Message
                    });
            }
        }
    }
}
