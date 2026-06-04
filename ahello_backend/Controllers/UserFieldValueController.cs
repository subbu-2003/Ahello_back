using ahello_backend.Models.Users;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserFieldValueController : ControllerBase
    {
        private readonly IUserFieldValueService _service;

        public UserFieldValueController(
            IUserFieldValueService service)
        {
            _service = service;
        }

        // POST
        [HttpPost]
        public async Task<IActionResult> Create(
            CreateUserFieldValue model)
        {
            var id = await _service.CreateAsync(model);

            return Ok(new
            {
                Success = true,
                UserFieldValueId = id
            });
        }

        // PUT
        [HttpPut]
        public async Task<IActionResult> Update(
            UpdateUserFieldValue model)
        {
            bool success = await _service.UpdateAsync(model);

            if (!success)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Update failed"
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Updated successfully"
            });
        }

        // GET BY USER
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var data = await _service.GetByUserAsync(userId);

            return Ok(data);
        }

        // GET BY ID
        [HttpGet("{userFieldValueId}")]
        public async Task<IActionResult> GetById(
            int userFieldValueId)
        {
            var data = await _service.GetByIdAsync(userFieldValueId);

            if (data == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Data not found"
                });
            }

            return Ok(data);
        }
    }
}