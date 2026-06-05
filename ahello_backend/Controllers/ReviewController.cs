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
            try
            {
                var result = await _service.GetAllAsync();

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

        [HttpGet("{reviewId}")]
        public async Task<IActionResult> GetById(int reviewId)
        {
            try
            {
                var result = await _service.GetByIdAsync(reviewId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Review not found"
                    });
                }

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

        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetByBookingId(int bookingId)
        {
            try
            {
                var result = await _service.GetByBookingIdAsync(bookingId);

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

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            try
            {
                var result = await _service.GetByUserIdAsync(userId);

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

        [HttpPost]
        public async Task<IActionResult> Create(
             [FromBody] ReviewRequest request)
        {
            try
            {
                var result = await _service.CreateAsync(request);

                return Ok(new
                {
                    Success = true,
                    Message = "Review created successfully",
                    Result = result
                });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;

                // Foreign Key Error
                if (ex.Message.Contains("FOREIGN KEY"))
                {
                    errorMessage = "Invalid BookingId or UserId.";
                }

                // Duplicate Error
                else if (ex.Message.Contains("Duplicate"))
                {
                    errorMessage = "Review already exists.";
                }

                return StatusCode(500, new
                {
                    Success = false,
                    Message = errorMessage
                });
            }
        }

        [HttpPut("{reviewId}")]
        public async Task<IActionResult> Update(
            int reviewId,
            [FromBody] ReviewUpdateRequest request)
        {
            try
            {
                var result =
                    await _service.UpdateAsync(reviewId, request);

                if (result == 0)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Review not found"
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Review updated successfully"
                });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;

                if (ex.Message.Contains("FOREIGN KEY"))
                {
                    errorMessage = "Invalid reference data.";
                }
                else if (ex.Message.Contains("Duplicate"))
                {
                    errorMessage = "Duplicate data already exists.";
                }

                return StatusCode(500, new
                {
                    Success = false,
                    Message = errorMessage
                });
            }
        }


        [HttpDelete("{reviewId}")]
        public async Task<IActionResult> Delete(int reviewId)
        {
            try
            {
                var result = await _service.DeleteAsync(reviewId);

                if (result == 0)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Review not found"
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Review deleted successfully"
                });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;

                if (ex.Message.Contains("FOREIGN KEY"))
                {
                    errorMessage = "This review is already in use.";
                }

                return StatusCode(500, new
                {
                    Success = false,
                    Message = errorMessage
                });
            }
        }
    }
}
