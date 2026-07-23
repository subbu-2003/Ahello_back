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
                    $"https://ahllo.com/meeting/join/{meeting.RoomName}?roomId={meeting.RoomId}";

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
            var meetingLink = $"https://ahllo.com/meeting/join/{meeting.RoomName}?roomId={meeting.RoomId}";

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

                if (role == "client")
                {
                    if (!model.RequestId.HasValue)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "RequestId is required."
                        });
                    }

                    var request = await _instantMeetingService
                        .GetJoinRequestByIdAsync(model.RequestId.Value);

                    if (request == null)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "Join request not found."
                        });
                    }

                    if (request.Status != "Approved")
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Waiting for host approval."
                        });
                    }
                }

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
        [HttpPost("join-request")]
        public async Task<IActionResult> JoinRequest(
    [FromBody] InstantMeetingJoinRequestPost model)
        {
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

            var requestId = await _instantMeetingService
                .CreateJoinRequestAsync(new InstantMeetingJoinRequest
                {
                    InstantMeetingId = meeting.Id,
                    UserName = model.UserName,
                    Status = "Waiting"
                });

            return Ok(new
            {
                success = true,
                requestId,
                status = "Waiting"
            });
        }

        [HttpGet("waiting-users/{roomName}")]
        public async Task<IActionResult> GetWaitingUsers(
    string roomName)
        {
            var meeting = await _instantMeetingService
                .GetByRoomNameAsync(roomName);

            if (meeting == null)
            {
                return NotFound();
            }

            var users = await _instantMeetingService
                .GetWaitingUsersAsync(meeting.Id);

            return Ok(users);
        }

        [HttpPut("join-request/{requestId}/status")]
        public async Task<IActionResult> UpdateJoinRequestStatus(
    int requestId,
    [FromBody] InstantMeetingJoinRequestStatusPut model)
        {
            await _instantMeetingService
                .UpdateJoinRequestStatusAsync(
                    requestId,
                    model.Status);

            return Ok(new
            {
                success = true
            });
        }

        [HttpPut("join-requests/status")]
        public async Task<IActionResult> UpdateAllJoinRequestStatus(
    [FromBody] InstantMeetingJoinRequestsStatusPut model)
        {
            var meeting = await _instantMeetingService.GetByRoomNameAsync(model.RoomName);

            if (meeting == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Meeting not found."
                });
            }

            await _instantMeetingService.UpdateAllJoinRequestStatusAsync(
                meeting.Id,
                model.Status);

            return Ok(new
            {
                success = true
            });
        }

        [HttpGet("request-status/{requestId}")]
        public async Task<IActionResult> GetRequestStatus(
    int requestId)
        {
            var request = await _instantMeetingService
                .GetJoinRequestByIdAsync(requestId);

            if (request == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                success = true,
                status = request.Status
            });
        }
    }
}