using ahello_backend.Models.WelcomeTour;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WelcomeTourController : ControllerBase
    {
        private readonly IWelcomeTourService _service;

        public WelcomeTourController(IWelcomeTourService service)
        {
            _service = service;
        }

        // GET: api/WelcomeTour
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _service.GetAsync();

            return Ok(result);
        }

        // GET: api/WelcomeTour/user/1
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var result = await _service.GetByUserIdAsync(userId);

            return Ok(result);
        }

        // POST: api/WelcomeTour
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] WelcomeTour model)
        {
            var id = await _service.PostAsync(model);

            return Ok(new
            {
                Message = "Welcome tour created successfully",
                WelcomeTourId = id
            });
        }

        // PUT: api/WelcomeTour/1
        [HttpPut("{WelcomeTourId}")]
        public async Task<IActionResult> Put(
            int WelcomeTourId,
            [FromBody] WelcomeTour model)
        {
            model.WelcomeTourId = WelcomeTourId;

            var result = await _service.PutAsync(model);

            if (!result)
                return NotFound(new
                {
                    Message = "Welcome tour not found"
                });

            return Ok(new
            {
                Message = "Welcome tour updated successfully"
            });
        }

        // PUT: api/WelcomeTour/user/1/isactive
        [HttpPut("user/{userId}/isactive")]
        public async Task<IActionResult> PutByUserId(
            int userId,
            [FromBody] UpdateWelcomeTourStatus model)
        {
            var result = await _service.PutByUserIdAsync(
                userId,
                model.IsActive,
                model.ModifiedBy
            );

            if (!result)
                return NotFound(new
                {
                    Message = "Welcome tour not found for this user"
                });

            return Ok(new
            {
                Message = "Welcome tour status updated successfully"
            });
        }
        // PUT: api/WelcomeTour/user/1/item/isactive
        [HttpPut("user/{userId}/item/isactive")]
        public async Task<IActionResult> PutByUserIdAndItemKey(
            int userId,
            [FromBody] UpdateWelcomeTourItemStatus model)
        {
            var result = await _service.PutByUserIdAndItemKeyAsync(
                userId,
                model.ItemKey,
                model.IsActive,
                model.ModifiedBy
            );
            if (!result)
                return NotFound(new
                {
                    Message = "Welcome tour item not found for this user"
                });
            return Ok(new
            {
                Message = "Welcome tour item status updated successfully"
            });
        }
    }

    public class UpdateWelcomeTourStatus
    {
        public bool IsActive { get; set; }
        public int ModifiedBy { get; set; }
    }
    public class UpdateWelcomeTourItemStatus
    {
        public string ItemKey { get; set; }
        public bool IsActive { get; set; }
        public int ModifiedBy { get; set; }
    }
}

