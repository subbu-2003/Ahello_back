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
                    Message = ex.Message,
                });
            }
        }

        [HttpGet("meeting/{meetingId}")]
        public async Task<IActionResult> GetByMeetingId(int meetingId)
        {
            try
            {
                var result =
                    await _service.GetByMeetingIdAsync(meetingId);

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
                var result =
                    await _service.GetByUserIdAsync(userId);

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
              MeetingChatMessageCreate model)
        {
            try
            {
                var result =
                    await _service.CreateAsync(model);

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
                    errorMessage = "Invalid MeetingId. Meeting does not exist.";
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
        public async Task<IActionResult> Update(
            MeetingChatMessageUpdate model)
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
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message
               
                });
            }
        }

        [HttpDelete("{chatMessageId}")]
        public async Task<IActionResult> Delete(int chatMessageId)
        {
            try
            {
                var result =
                    await _service.DeleteAsync(chatMessageId);

                return Ok(new
                {
                    Success = true,
                    Data = result
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
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount(int meetingId, int userId)
        {
            try
            {
                var count = await _service.GetUnreadCountAsync(meetingId, userId);

                return Ok(new { count });
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

        [HttpPut("mark-read")]
        public async Task<IActionResult> MarkRead(int meetingId, int userId)
        {
            try
            {
                await _service.MarkAsReadAsync(meetingId, userId);

                return Ok(new
                {
                    Success = true
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
    }
}
