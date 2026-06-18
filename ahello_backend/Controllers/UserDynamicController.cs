using ahello_backend.Models.User;
using ahello_backend.Models.Users;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserDynamicController : ControllerBase
    {
        private readonly IUserDynamicService _service;
        private readonly IWebHostEnvironment _env;

        public UserDynamicController(IUserDynamicService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();

            return Ok(data);
        }

        [HttpGet("pagination")]
        public async Task<IActionResult> GetAllPagination(
            int pageNumber = 1,
            int pageSize = 10, string? search = null)
        {
            var data = await _service.GetAllPaginationAsync(
                pageNumber,
                pageSize, search);

            return Ok(data);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetById(int userId)
        {
            var data = await _service.GetByIdAsync(userId);

            if (data == null)
                return NotFound("User not found");

            return Ok(data);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategoryId(int categoryId)
        {
            var result =
                await _service.GetByCategoryIdAsync(categoryId);

            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> Create(
     [FromForm] UserDynamicPost model)
        {
            if (model.ProfileFile != null &&
                model.ProfileFile.Length > 0)
            {
                var rootPath = _env.WebRootPath
                    ?? Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot");

                var folderPath =
                    Path.Combine(rootPath, "user-profiles");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName =
                    $"{Guid.NewGuid()}{Path.GetExtension(model.ProfileFile.FileName)}";

                var filePath =
                    Path.Combine(folderPath, fileName);

                await using var stream =
                    new FileStream(filePath, FileMode.Create);

                await model.ProfileFile.CopyToAsync(stream);

                model.ProfileUrl = $"/user-profiles/{fileName}";
            }

            var id = await _service.CreateAsync(model);

            return Ok(new
            {
                Success = true,
                UserId = id,
                ProfileUrl = model.ProfileUrl
            });
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> Update(
        int userId,
        [FromForm] UserDynamicPut model)
        {
            if (model.ProfileFile != null &&
                model.ProfileFile.Length > 0)
            {
                var rootPath = _env.WebRootPath
                    ?? Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot");

                var folderPath =
                    Path.Combine(rootPath, "user-profiles");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName =
                    $"{Guid.NewGuid()}{Path.GetExtension(model.ProfileFile.FileName)}";

                var filePath =
                    Path.Combine(folderPath, fileName);

                await using var stream =
                    new FileStream(filePath, FileMode.Create);

                await model.ProfileFile.CopyToAsync(stream);

                model.ProfileUrl = $"/user-profiles/{fileName}";
            }

            var updated = await _service.UpdateAsync(
                userId,
                model);

            if (!updated)
                return BadRequest("Update failed");

            return Ok(new
            {
                Success = true,
                Message = "Updated successfully",
                ProfileUrl = model.ProfileUrl
            });
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> Delete(int userId)
        {
            var deleted = await _service.DeleteAsync(userId);

            if (!deleted)
                return BadRequest("Delete failed");

            return Ok(new
            {
                Success = true,
                Message = "Deleted successfully"
            });
        }

        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetUserProfile(
        int userId,
        int pageNumber = 1,
        int pageSize = 10,
        string search = "")
        {
            var result = await _service.GetUserProfileAsync(
                userId,
                pageNumber,
                pageSize,
                search);

            if (result == null)
            {
                return NotFound(new
                {
                    Message = "User not found"
                });
            }

            return Ok(result);
        }
    }
}