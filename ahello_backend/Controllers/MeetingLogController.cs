using ahello_backend.Models.MeetingParticipant;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingLogController : ControllerBase
    {
        private readonly IMeetingParticipantService _service;

        public MeetingLogController(
            IMeetingParticipantService service)
        {
            _service = service;
        }

        [HttpPost("join")]
        public async Task<IActionResult> Join(
            [FromBody] MeetingParticipant model)
        {
            try
            {
                var participantId =
                    await _service.CreateAsync(model);

                return Ok(new
                {
                    success = true,
                    meetingParticipantId = participantId,
                    message =
                        "Participant session created successfully."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost("leave")]
        public async Task<IActionResult> Leave(
            [FromBody] MeetingParticipantLeave model)
        {
            try
            {
                var result =
                    await _service.LeaveAsync(model);

                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message =
                            "Active participant session not found."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message =
                        "Participant session closed successfully."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("{meetingId}")]
        public async Task<IActionResult> GetMeetingLog(
            int meetingId)
        {
            try
            {
                var result =
                    await _service.GetMeetingLogAsync(
                        meetingId);

                return Ok(new
                {
                    success = true,
                    meetingId = meetingId,
                    participants = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("{meetingId}/participant/{userId}")]
        public async Task<IActionResult> GetParticipantHistory(
    int meetingId,
    int userId)
        {
            try
            {
                var result =
                    await _service
                        .GetParticipantHistoryAsync(
                            meetingId,
                            userId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Participant history not found."
                    });
                }

                return Ok(new
                {
                    success = true,
                    meetingId = result.MeetingId,
                    userId = result.UserId,
                    userName = result.UserName,
                    participantType = result.ParticipantType,
                    totalDurationSeconds = result.TotalDurationSeconds,
                    totalDuration = result.TotalDuration,
                    sessions = result.Sessions
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}