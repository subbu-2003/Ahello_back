using ahello_backend.Models.Meeting;
using ahello_backend.Services.Classes;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using ahello_backend.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InstantMeetingController : ControllerBase
    {
        private readonly HundredMsService _hundredMsService;
        private readonly IInstantMeetingService _instantMeetingService;
        private readonly IHubContext<MeetingHub> _hubContext;

        public InstantMeetingController(
            HundredMsService hundredMsService,
            IInstantMeetingService instantMeetingService, IHubContext<MeetingHub> hubContext)

        {
            _hundredMsService = hundredMsService;
            _instantMeetingService = instantMeetingService;
            _hubContext = hubContext;
        }

        // =========================================================
        // Create Instant Meeting
        // =========================================================
        [HttpPost("create")]
        public async Task<IActionResult> CreateMeeting(
            [FromBody] InstantMeetingCreatePost model, [FromQuery] int userId)
        {
            try
            {
                var meeting = await _hundredMsService.CreateInstantRoomAsync(
                    model.Title);

                var meetingLink =
                    $"https://ahllo.com/meeting/join/{meeting.RoomName}?roomId={meeting.RoomId}";

                await _instantMeetingService.CreateAsync(new InstantMeeting
                {
                    RoomId = meeting.RoomId,
                    RoomName = meeting.RoomName,
                    MeetingLink = meetingLink,
                    UserId = userId,
                    HostKey = ""
                });

                return Ok(new
                {
                    success = true,

                    roomName = meeting.RoomName,

                    roomId = meeting.RoomId,

                    meetingLink,

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
            var meetingLink = meeting.MeetingLink;

            return Ok(new
            {
                success = true,

                roomId = meeting.RoomId,

                roomName = meeting.RoomName,
                meetingLink,
                hostUserId = meeting.UserId,
            });
        }

        // =========================================================
        // Generate SDK Token
        // =========================================================
        [HttpPost("sdk-token")]
        public async Task<IActionResult> GenerateSdkToken(
            [FromBody] InstantMeetingTokenPost model, [FromQuery] int userId)
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

                var currentUserId = userId;

                var role =
                    meeting.UserId == currentUserId
                        ? "host"
                        : "client";

                if (role == "client")
                {

                    var request = await _instantMeetingService
    .GetByMeetingAndUserAsync(
        meeting.Id,
        currentUserId);

                    if (request == null)
                    {
                        return BadRequest(new
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
userId.ToString());

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
    [FromBody] InstantMeetingJoinRequestPost model, [FromQuery] int userId)
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
            var existing = await _instantMeetingService
    .GetByMeetingAndUserAsync(meeting.Id, userId);

            if (existing != null)
            {
                return Ok(new
                {
                    success = true,
                    requestId = existing.Id,
                    status = existing.Status
                });
            }
            if (meeting.UserId == userId)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Host does not need a join request."
                });
            }

            var requestId = await _instantMeetingService
                .CreateJoinRequestAsync(new InstantMeetingJoinRequest
                {
                    InstantMeetingId = meeting.Id,
                    UserId = userId,
                    Status = "Waiting"
                });
            await _hubContext.Clients
    .Group($"host_{meeting.RoomName}")
    .SendAsync(
        "JoinRequestReceived",
        new
        {
            requestId,
            userId,
            status = "Waiting"
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
    string roomName, [FromQuery] int userId)
        {
            var meeting = await _instantMeetingService
                .GetByRoomNameAsync(roomName);

            if (meeting == null)
            {
                return NotFound();
            }
            if (meeting.UserId != userId)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Only the host can view waiting users."
                });
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
            var request = await _instantMeetingService
                .GetJoinRequestByIdAsync(requestId);

            if (request == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Join request not found."
                });
            }

            await _instantMeetingService
                .UpdateJoinRequestStatusAsync(
                    requestId,
                    model.Status);

            // Notify the specific client
            await _hubContext.Clients
                .Group($"user_{request.UserId}")
                .SendAsync(
                    "JoinRequestStatusChanged",
                    new
                    {
                        requestId = request.Id,
                        status = model.Status
                    });

            return Ok(new
            {
                success = true
            });
        }

        [HttpPut("join-requests/status")]
        public async Task<IActionResult> UpdateAllJoinRequestStatus(
    [FromBody] InstantMeetingJoinRequestsStatusPut model)
        {
            try
            {
                var meeting =
                    await _instantMeetingService
                        .GetByRoomNameAsync(model.RoomName);

                if (meeting == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Meeting not found."
                    });
                }

                // Get all WAITING requests BEFORE updating them
                var waitingRequests =
                    await _instantMeetingService
                        .GetWaitingUsersAsync(meeting.Id);

                if (waitingRequests == null ||
                    !waitingRequests.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        approvedCount = 0,
                        message = "No waiting participants."
                    });
                }

                // Update all waiting requests
                await _instantMeetingService
                    .UpdateAllJoinRequestStatusAsync(
                        meeting.Id,
                        model.Status
                    );

                // Notify every waiting user
                foreach (var request in waitingRequests)
                {
                    await _hubContext.Clients
                        .Group($"user_{request.UserId}")
                        .SendAsync(
                            "JoinRequestStatusChanged",
                            new
                            {
                                requestId = request.Id,
                                userId = request.UserId,
                                status = model.Status
                            }
                        );
                }

                return Ok(new
                {
                    success = true,
                    approvedCount = waitingRequests.Count()
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