using ahello_backend.Models.Reviews;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _service;

        public ReviewController(IReviewService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();

            return Ok(result);
        }

        [HttpGet("{reviewId}")]
        public async Task<IActionResult> GetById(int reviewId)
        {
            var result = await _service.GetByIdAsync(reviewId);

            if (result == null)
                return NotFound("Review not found");

            return Ok(result);
        }

        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetByBookingId(int bookingId)
        {
            var result = await _service.GetByBookingIdAsync(bookingId);

            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var result = await _service.GetByUserIdAsync(userId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] ReviewRequest request)
        {
            var result = await _service.CreateAsync(request);

            return Ok(new
            {
                Message = "Review created successfully",
                Result = result
            });
        }

        [HttpPut("{reviewId}")]
        public async Task<IActionResult> Update(
            int reviewId,
            [FromBody] ReviewUpdateRequest request)
        {
            var result = await _service.UpdateAsync(reviewId, request);

            if (result == 0)
                return NotFound("Review not found");

            return Ok(new
            {
                Message = "Review updated successfully"
            });
        }

        [HttpDelete("{reviewId}")]
        public async Task<IActionResult> Delete(int reviewId)
        {
            var result = await _service.DeleteAsync(reviewId);

            if (result == 0)
                return NotFound("Review not found");

            return Ok(new
            {
                Message = "Review deleted successfully"
            });
        }
    }
}
