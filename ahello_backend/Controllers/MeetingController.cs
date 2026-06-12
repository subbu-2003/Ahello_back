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
            try
            {
                return Ok(await _service.GetAllAsync());
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

        [HttpGet("{meetingId}")]
        public async Task<IActionResult> GetById(int meetingId)
        {
            try
            {
                return Ok(await _service.GetByIdAsync(meetingId));
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
                return Ok(await _service.GetByBookingIdAsync(bookingId));
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

        [HttpGet("verify/{roomName}")]
        public async Task<IActionResult> VerifyAccess(string roomName, [FromQuery] int userId)
        {
            try
            {
                // 1. Fetch meeting
                var meeting = await _service.GetByRoomNameAsync(roomName);

                if (meeting == null)
                    return NotFound(new
                    {
                        allowed = false,
                        reason = "Meeting not found."
                    });

                // 2. Block if already completed
                if (meeting.Status == "Completed")
                    return StatusCode(410, new
                    {
                        allowed = false,
                        reason = "This meeting has already ended."
                    });

                // 3. Block if cancelled
                if (meeting.Status == "Cancelled")
                    return StatusCode(410, new
                    {
                        allowed = false,
                        reason = "This meeting was cancelled."
                    });

                // 4. Block old rescheduled meeting link
                if (meeting.Status == "Rescheduled")
                    return StatusCode(410, new
                    {
                        allowed = false,
                        reason = "This meeting was rescheduled. Please use the new meeting link."
                    });

                // 5. Block missed meeting
                if (meeting.Status == "Missed")
                    return StatusCode(410, new
                    {
                        allowed = false,
                        reason = "This meeting was missed."
                    });

                // 6. Block if too early, more than 10 minutes before start
                if (DateTime.Now < meeting.StartTime.AddMinutes(-10))
                    return StatusCode(425, new
                    {
                        allowed = false,
                        reason = "Meeting hasn't started yet.",
                        startsAt = meeting.StartTime
                    });

                // 7. Block if meeting end time passed
                if (DateTime.Now > meeting.EndTime)
                    return StatusCode(410, new
                    {
                        allowed = false,
                        reason = "This meeting has already ended."
                    });

                // 8. Fetch booking and verify user belongs to it
                var booking = await _bookingService.GetByIdAsync(meeting.BookingId);

                if (booking == null)
                    return NotFound(new
                    {
                        allowed = false,
                        reason = "Booking not found."
                    });

                if (booking.UserId != userId && booking.ClientId != userId)
                    return Unauthorized(new
                    {
                        allowed = false,
                        reason = "You are not part of this meeting."
                    });

                // 9. Block if booking itself is cancelled/rejected/rescheduled/no-show
                if (booking.Status == "Cancelled")
                    return StatusCode(410, new
                    {
                        allowed = false,
                        reason = "This booking was cancelled."
                    });

                if (booking.Status == "Rejected")
                    return StatusCode(410, new
                    {
                        allowed = false,
                        reason = "This booking was rejected."
                    });

                if (booking.Status == "Rescheduled")
                    return StatusCode(410, new
                    {
                        allowed = false,
                        reason = "This booking was rescheduled. Please use the new meeting link."
                    });

                if (booking.Status == "NoShow")
                    return StatusCode(410, new
                    {
                        allowed = false,
                        reason = "This booking was marked as no-show."
                    });

                // 10. Payment check, uncomment when payments go live
                // if (booking.PaymentStatus != "Paid")
                //     return StatusCode(402, new
                //     {
                //         allowed = false,
                //         reason = "Payment not completed."
                //     });

                return Ok(new
                {
                    allowed = true,
                    meetingId = meeting.MeetingId,
                    bookingId = meeting.BookingId,
                    startTime = meeting.StartTime,
                    endTime = meeting.EndTime
                });
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
        public async Task<IActionResult> GetByUserId(
            int userId,
            int pageNumber = 1,
            int pageSize = 10,
            string? status = null,
            DateTime? createdDate = null)
        {
            try
            {
                var result =
                    await _service.GetByUserIdAsync(
                        userId,
                        pageNumber,
                        pageSize, status,
                        createdDate);

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
        public async Task<IActionResult> Create(MeetingPost model)
        {
            try
            {
                var id = await _service.CreateAsync(model);

                return Ok(new
                {
                    Success = true,
                    MeetingId = id
                });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;

                // Foreign Key Error
                if (ex.Message.Contains("FOREIGN KEY"))
                {
                    errorMessage = "Invalid BookingId. Booking does not exist.";
                }

                // Duplicate Error
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

        [HttpPut]
        public async Task<IActionResult> Update(MeetingPut model)
        {
            try
            {
                var result =
                    await _service.UpdateAsync(model);

                return Ok(new
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;

                // Foreign Key Error
                if (ex.Message.Contains("FOREIGN KEY"))
                {
                    errorMessage = "Invalid BookingId. Booking does not exist.";
                }

                // Duplicate Error
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

        [HttpDelete("{meetingId}")]
        public async Task<IActionResult> Delete(int meetingId)
        {
            try
            {
                var result =
                    await _service.DeleteAsync(meetingId);

                return Ok(new
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;

                if (ex.Message.Contains("FOREIGN KEY"))
                {
                    errorMessage = "This meeting is already in use.";
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