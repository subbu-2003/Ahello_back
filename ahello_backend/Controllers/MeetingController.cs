using ahello_backend.DbContexts;
using ahello_backend.Models.Meeting;
using ahello_backend.Services.Classes;
using ahello_backend.Services.Interfaces;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingController : ControllerBase
    {
        private readonly IMeetingService _service;
        private readonly HundredMsService _hundredMsService;
        private readonly IBookingService _bookingService;
        private readonly DbContextConnection _dbConn;

        public MeetingController(
            IMeetingService service,
            IBookingService bookingService,
            HundredMsService hundredMsService,
            DbContextConnection dbConn)
        {
            _service = service;
            _bookingService = bookingService;
            _hundredMsService = hundredMsService;
            _dbConn = dbConn;
        }
        [HttpGet("join-info/{roomName}")]
        public async Task<IActionResult> GetJoinInfo(string roomName, [FromQuery] int userId)
        {
            if (userId <= 0)
                return BadRequest(new { reason = "Invalid userId." });

            var meeting = await _service.GetByRoomNameAsync(roomName);

            if (meeting == null)
                return NotFound(new { reason = "Meeting not found." });

            if (meeting.UserId != userId && meeting.ClientId != userId)
                return StatusCode(403, new { reason = "You are not part of this meeting." });

            var isHost = meeting.UserId == userId;

            var roomCode = isHost
                ? meeting.HostRoomCode
                : meeting.ClientRoomCode;

            var displayName = isHost
                ? meeting.UserName
                : meeting.ClientName;

            if (string.IsNullOrWhiteSpace(roomCode))
                return BadRequest(new { reason = "100ms room code not found. Create a new booking." });

            return Ok(new
            {
                meetingId = meeting.MeetingId,
                roomCode,
                role = isHost ? "host" : "client",
                name = displayName
            });
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
                // 6. Block if too early, more than 10 minutes before start
                var istNow = DateTime.UtcNow.AddHours(5).AddMinutes(30);
                if (istNow < meeting.StartTime)
                {
                    var remaining = meeting.StartTime - istNow;
                    var remainingMinutes = (int)Math.Ceiling(remaining.TotalMinutes);

                    return StatusCode(425, new
                    {
                        allowed = false,
                        reason = $"Meeting hasn't started yet. Starts in {remainingMinutes} minutes.",
                        startsAt = meeting.StartTime,
                        remainingMinutes
                    });
                }

                // 7. Block if meeting end time passed
                if (istNow > meeting.EndTime)
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

                // 10. Payment check — uncomment when payments go live
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
            DateTime? startDate = null)
        {
            try
            {
                var result = await _service.GetByUserIdAsync(
                    userId,
                    pageNumber,
                    pageSize,
                    status,
                    startDate);

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

                if (ex.Message.Contains("FOREIGN KEY"))
                    errorMessage = "Invalid BookingId. Booking does not exist.";
                else if (ex.Message.Contains("Duplicate"))
                    errorMessage = "Duplicate data already exists.";

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
                if (model == null)
                    return BadRequest(new { Message = "Request body is required." });

                if (model.MeetingId <= 0)
                    return BadRequest(new { Message = "MeetingId is required." });

                if (model.UserId <= 0)
                    return BadRequest(new { Message = "UserId is required." });

                if (model.BookingId <= 0)
                    return BadRequest(new { Message = "BookingId is required." });

                if (string.IsNullOrWhiteSpace(model.RoomId))
                    return BadRequest(new { Message = "RoomId is required." });

                if (model.StartTime == default)
                    return BadRequest(new { Message = "StartTime is required." });

                if (model.EndTime == default)
                    return BadRequest(new { Message = "EndTime is required." });

                if (string.IsNullOrWhiteSpace(model.Status))
                    return BadRequest(new { Message = "Status is required." });

                if (string.IsNullOrWhiteSpace(model.ModifiedBy))
                    return BadRequest(new { Message = "ModifiedBy is required." });

                var result = await _service.UpdateAsync(model);

                return Ok(new
                {
                    Success = true,
                    Data = result
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

        [HttpDelete("{meetingId}")]
        public async Task<IActionResult> Delete(int meetingId)
        {
            try
            {
                var result = await _service.DeleteAsync(meetingId);
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
                    errorMessage = "This meeting is already in use.";

                return StatusCode(500, new
                {
                    Success = false,
                    Message = errorMessage
                });
            }
        }
        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus(MeetingStatusPut request)
        {
            try
            {
                var result = await _service.UpdateStatusAsync(
                    request.MeetingId,
                    request.Status,
                    request.ModifiedBy);

                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Meeting not found."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Meeting status updated successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
        [HttpGet("sdk-token/{roomName}")]
        public async Task<IActionResult> GetSdkToken(string roomName, [FromQuery] int userId)
        {
            var meeting = await _service.GetByRoomNameAsync(roomName);

            if (meeting == null)
                return NotFound(new { message = "Meeting not found" });

            if (meeting.UserId != userId && meeting.ClientId != userId)
                return Unauthorized(new { message = "Unauthorized" });

            bool isHost = meeting.UserId == userId;

            string role = isHost ? "host" : "client";

            string name = isHost
                ? meeting.UserName
                : meeting.ClientName;

            var token = _hundredMsService.GenerateAuthToken(
                meeting.RoomId,
                role,
                userId.ToString()
            );

            return Ok(new
            {
                authToken = token,
                userName = name,
                role,
                roomId = meeting.RoomId
            });
        }
    }
}