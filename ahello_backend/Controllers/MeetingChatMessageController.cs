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
        private readonly FileUploadService _fileUpload;

        public MeetingChatMessageController(
            IMeetingChatMessageService service, FileUploadService fileUpload)
        {
            _service = service;
            _fileUpload = fileUpload;
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
        public async Task<IActionResult> Create([FromForm] MeetingChatMessageCreate model)
        {
            try
            {
                // Upload attachment if present
                if (model.File != null && model.File.Length > 0)
                {
                    model.AttachmentUrl =
                        await _fileUpload.SaveChatFileAsync(model.File);

                }
                else
                {
                    model.AttachmentUrl = null;
                    model.MessageType ??= "TEXT";
                }

                var result = await _service.CreateAsync(model);

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
                    errorMessage = "Invalid MeetingId. Meeting does not exist.";
                }
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
