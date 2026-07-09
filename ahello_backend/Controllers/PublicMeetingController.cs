using ahello_backend.Models.Meeting;
using ahello_backend.Services.Classes;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublicMeetingController : ControllerBase
    {
        private readonly HundredMsService _hundredMsService;

        public PublicMeetingController(HundredMsService hundredMsService)
        {
            _hundredMsService = hundredMsService;
        }

        // =========================================================
        // Create Instant Meeting
        // =========================================================
        [HttpPost("create")]
        public async Task<IActionResult> CreateMeeting(
            [FromBody] PublicMeetingCreatePost model)
        {
            try
            {
                var meeting = await _hundredMsService.CreateInstantRoomAsync(
                    model.Title);


                var meetingLink =
    $"{Request.Scheme}://{Request.Host}/meeting/join/{meeting.RoomName}" +
    $"?roomId={Uri.EscapeDataString(meeting.RoomId)}" +
    $"&roomCode={Uri.EscapeDataString(meeting.GuestRoomCode)}";

                return Ok(new
                {
                    success = true,

                    roomName = meeting.RoomName,

                    roomId = meeting.RoomId,

                    hostRoomCode = meeting.HostRoomCode,

                    guestRoomCode = meeting.GuestRoomCode,

                    meetingLink
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
        // Generate SDK Token
        // =========================================================
        [HttpPost("sdk-token")]
        public IActionResult GenerateSdkToken(
            [FromBody] PublicMeetingTokenPost model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.RoomId))
                    return BadRequest(new
                    {
                        success = false,
                        message = "RoomId is required."
                    });

                if (string.IsNullOrWhiteSpace(model.Name))
                    return BadRequest(new
                    {
                        success = false,
                        message = "Name is required."
                    });

                var role = model.IsHost ? "host" : "client";

                var authToken = _hundredMsService.GenerateAuthToken(
                    model.RoomId,
                    role,
                    Guid.NewGuid().ToString());

                return Ok(new
                {
                    success = true,

                    authToken,

                    role,

                    roomId = model.RoomId,

                    roomCode = model.RoomCode,

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