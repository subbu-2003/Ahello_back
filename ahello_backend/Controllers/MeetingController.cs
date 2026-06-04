using ahello_backend.Models.Meeting;
using ahello_backend.Services.Classes;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingController : ControllerBase
    {
        private readonly IMeetingService _service;
        private readonly IBookingService _bookingService;

        public MeetingController(IMeetingService service, IBookingService bookingService)
        {
            _service = service;
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{meetingId}")]
        public async Task<IActionResult> GetById(int meetingId)
        {
            return Ok(await _service.GetByIdAsync(meetingId));
        }

        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetByBookingId(int bookingId)
        {
            return Ok(await _service.GetByBookingIdAsync(bookingId));
        }

        [HttpGet("verify/{roomName}")]
        public async Task<IActionResult> VerifyAccess(string roomName, [FromQuery] int userId)
        {
            try
            {
                // 1. Meeting fetch
                var meeting = await _service.GetByRoomNameAsync(roomName);

                if (meeting == null)
                    return NotFound("Meeting not found");

                // 2. Booking fetch
                var booking = await _bookingService.GetByIdAsync(meeting.BookingId);

                if (booking == null)
                    return NotFound("Booking not found");

                // 3. User check — UserId (4) or ClientId (1)
                if (booking.UserId != userId && booking.ClientId != userId)
                    return Unauthorized("You are not part of this meeting");

                // 4. Payment check
                // if (booking.Status != "Confirmed")
                //     return Unauthorized("Payment not completed");

                return Ok(new { allowed = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(
     int userId,
     int pageNumber = 1,
     int pageSize = 10)
        {
            var result =
                await _service.GetByUserIdAsync(
                    userId,
                    pageNumber,
                    pageSize);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(MeetingPost model)
        {
            var id = await _service.CreateAsync(model);

            return Ok(id);
        }

        [HttpPut]
        public async Task<IActionResult> Update(MeetingPut model)
        {
            return Ok(await _service.UpdateAsync(model));
        }

        [HttpDelete("{meetingId}")]
        public async Task<IActionResult> Delete(int meetingId)
        {
            return Ok(await _service.DeleteAsync(meetingId));
        }
    }
}