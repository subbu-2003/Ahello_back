using ahello_backend.Models.Users;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserDropdownOptionController : ControllerBase
    {
        private readonly IUserDropdownOptionService _service;

        public UserDropdownOptionController(
            IUserDropdownOptionService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            CreateUserDropdownOption model)
        {
            var result = await _service.CreateAsync(model);

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(
            UpdateUserDropdownOption model)
        {
            var result = await _service.UpdateAsync(model);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            return Ok(result);
        }

        [HttpGet("field/{userFieldId}")]
        public async Task<IActionResult> GetByFieldId(
            int userFieldId,
            [FromQuery] int? userId)
        {
            var result = await _service
                .GetByFieldIdAsync(userFieldId, userId);

            return Ok(result);
        }
    }
}
