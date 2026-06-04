using ahello_backend.Models.Meeting;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeetingChatMessageController
        : ControllerBase
    {
        private readonly
            IMeetingChatMessageService
            _service;

        public MeetingChatMessageController(
            IMeetingChatMessageService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult>
            GetAll()
        {
            var result =
                await _service.GetAllAsync();

            return Ok(result);
        }

        [HttpGet("meeting/{meetingId}")]
        public async Task<IActionResult>
            GetByMeetingId(int meetingId)
        {
            var result =
                await _service
                    .GetByMeetingIdAsync(
                        meetingId);

            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult>
            GetByUserId(int userId)
        {
            var result =
                await _service
                    .GetByUserIdAsync(
                        userId);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult>
            Create(
                MeetingChatMessageCreate model)
        {
            var result =
                await _service
                    .CreateAsync(model);

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult>
            Update(
                MeetingChatMessageUpdate model)
        {
            var result =
                await _service
                    .UpdateAsync(model);

            return Ok(result);
        }

        [HttpDelete("{chatMessageId}")]
        public async Task<IActionResult>
            Delete(int chatMessageId)
        {
            var result =
                await _service
                    .DeleteAsync(
                        chatMessageId);

            return Ok(result);
        }
    }
}
