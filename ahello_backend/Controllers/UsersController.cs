using ahello_backend.Models.Users;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend_AHLLO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _service;
        private readonly IWebHostEnvironment _env;

        public UsersController(IUsersService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        // POST
        [HttpPost]
        public async Task<IActionResult> Post([FromForm] UserPost post)
        {
            if (post.ProfileFile != null &&
                post.ProfileFile.Length > 0)
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
                    $"{Guid.NewGuid()}{Path.GetExtension(post.ProfileFile.FileName)}";

                var filePath =
                    Path.Combine(folderPath, fileName);

                await using var stream =
                    new FileStream(filePath, FileMode.Create);

                await post.ProfileFile.CopyToAsync(stream);

                post.ProfileUrl = $"/user-profiles/{fileName}";
            }

            var id = await _service.PostUserAsync(post);

            return Ok(new
            {
                success = true,
                message = "User created successfully.",
                userId = id,
                profileUrl = post.ProfileUrl
            });
        }

        // GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllUsersAsync();

            return Ok(data);
        }

        // GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetUserByIdAsync(id);

            if (data == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User not found."
                });
            }

            return Ok(data);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromForm] UserPut put)
        {
            if (put.ProfileFile != null &&
                put.ProfileFile.Length > 0)
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
                    $"{Guid.NewGuid()}{Path.GetExtension(put.ProfileFile.FileName)}";

                var filePath =
                    Path.Combine(folderPath, fileName);

                await using var stream =
                    new FileStream(filePath, FileMode.Create);

                await put.ProfileFile.CopyToAsync(stream);

                put.ProfileUrl = $"/user-profiles/{fileName}";
            }

            var updated = await _service.UpdateUserAsync(put);

            return Ok(new
            {
                success = updated,
                message = updated
                    ? "User updated successfully."
                    : "Failed to update user.",
                profileUrl = put.ProfileUrl
            });
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteUserAsync(id);

            return Ok(new
            {
                success = deleted,
                message = deleted
                    ? "User deleted successfully."
                    : "Failed to delete user."
            });
        }
        [HttpGet("services")]
        public async Task<IActionResult> GetUserServices(
        int pageNumber = 1,
        int pageSize = 10,
        string? search = null)
        {
            var result = await _service.GetUserServicesAsync(
                pageNumber,
                pageSize,
                search);

            return Ok(result);
        }
        [HttpGet("search-services")]
        public async Task<IActionResult> SearchUserServices(
        string? keyword,
        int pageNumber = 1,
        int pageSize = 10)
        {
            var result =
                await _service.SearchUserServicesAsync(
                    keyword,
                    pageNumber,
                    pageSize);

            return Ok(result);
        }
    }
}