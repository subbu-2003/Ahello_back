using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FieldSearchController : ControllerBase
    {
        private readonly IFieldSearchService _service;

        public FieldSearchController(
            IFieldSearchService service)
        {
            _service = service;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] int userId,
            [FromQuery] string keyword)
        {
            var result =
                await _service.SearchAsync(
                    userId,
                    keyword);

            return Ok(new
            {
                success = true,
                userId,
                keyword,
                count = result.Count(),
                data = result
            });
        }
    }
}
