using ahello_backend.Models.Meeting;
using ahello_backend.Services.Classes;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InstantMeetingController : ControllerBase
    {
        private readonly HundredMsService _hundredMsService;
        private readonly IInstantMeetingService _instantMeetingService;

        public InstantMeetingController(
            HundredMsService hundredMsService,
            IInstantMeetingService instantMeetingService)
        {
            _hundredMsService = hundredMsService;
            _instantMeetingService = instantMeetingService;
        }

        // =========================================================
        // Create Instant Meeting
        // =========================================================
        [HttpPost("create")]
        public async Task<IActionResult> CreateMeeting(
            [FromBody] InstantMeetingCreatePost model)
        {
            try
            {
                var meeting = await _hundredMsService.CreateInstantRoomAsync(
                    model.Title);

                // Generate secure host key
                var hostKey = Convert.ToHexString(
                    RandomNumberGenerator.GetBytes(32));

                // Save meeting
                await _instantMeetingService.CreateAsync(new InstantMeeting
                {
                    RoomId = meeting.RoomId,
                    RoomName = meeting.RoomName,
                    HostKey = hostKey
                });

                var meetingLink =
                    $"https://ahllo.com/meeting/join/{meeting.RoomName}";

                return Ok(new
                {
                    success = true,

                    roomName = meeting.RoomName,

                    roomId = meeting.RoomId,

                    meetingLink,

                    hostKey
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

        // =========================================================
        // Get Meeting Details
        // =========================================================
        [HttpGet("{roomName}")]
        public async Task<IActionResult> GetMeeting(string roomName)
        {
            var meeting = await _instantMeetingService
                .GetByRoomNameAsync(roomName);

            if (meeting == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Meeting not found."
                });
            }
            var meetingLink = $"https://ahllo.com/meeting/join/{meeting.RoomName}";

            return Ok(new
            {
                success = true,

                roomId = meeting.RoomId,

                roomName = meeting.RoomName,
                meetingLink
            });
        }

        // =========================================================
        // Generate SDK Token
        // =========================================================
        [HttpPost("sdk-token")]
        public async Task<IActionResult> GenerateSdkToken(
            [FromBody] InstantMeetingTokenPost model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.RoomId))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "RoomId is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(model.RoomName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "RoomName is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Name is required."
                    });
                }

                var meeting = await _instantMeetingService
                    .GetByRoomNameAsync(model.RoomName);

                if (meeting == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Meeting not found."
                    });
                }
                if (meeting.RoomId != model.RoomId)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid meeting."
                    });
                }

                var role =
                    !string.IsNullOrWhiteSpace(model.HostKey) &&
                    meeting.HostKey == model.HostKey
                        ? "host"
                        : "client";

                var authToken = _hundredMsService.GenerateAuthToken(
                    meeting.RoomId,
                    role,
                    Guid.NewGuid().ToString());

                return Ok(new
                {
                    success = true,

                    authToken,

                    role,

                    roomId = meeting.RoomId,

                    roomName = meeting.RoomName,

                    userName = model.Name
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