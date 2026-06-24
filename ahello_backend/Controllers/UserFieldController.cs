using ahello_backend.Models.Users;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserFieldController : ControllerBase
    {
        private readonly IUserFieldService _service;

        public UserFieldController(IUserFieldService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserField model)
        {
            try
            {
                var id = await _service.CreateAsync(model);

                return Ok(new
                {
                    Success = true,
                    Message = "User field created successfully",
                    UserFieldId = id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateUserField model)
        {
            try
            {
                var result = await _service.UpdateAsync(model);

                return Ok(new
                {
                    Success = result,
                    Message = "User field updated successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var data = await _service.GetByUserAsync(userId);

            return Ok(data);
        }
        [HttpGet("userfieldform/{userId}")]
        public async Task<IActionResult> GetByUserAll(int userId)
        {
            var data = await _service.GetByUserAllAsync(userId);

            return Ok(data);
        }

        [HttpGet("{userFieldId}")]
        public async Task<IActionResult> GetById(int userFieldId)
        {
            var data = await _service.GetByIdAsync(userFieldId);

            return Ok(data);
        }

        [HttpDelete("{userFieldId}")]
        public async Task<IActionResult> Delete(
            int userFieldId,
            [FromQuery] string modifiedBy)
        {
            var result = await _service.DeleteAsync(
                userFieldId,
                modifiedBy
            );

            return Ok(new
            {
                Success = result,
                Message = "User field deleted successfully"
            });
        }
        [HttpGet("with-options/{userId}")]
        public async Task<IActionResult>GetFieldsWithOptions(int userId)
        {
            var result =
                await _service.GetFieldsWithOptionsAsync(userId);

            return Ok(result);
        }
    }
}