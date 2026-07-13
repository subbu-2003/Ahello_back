using ahello_backend.Models.Meeting;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstantChatMessageController: ControllerBase
    {
        private readonly IInstantChatMessageService _service;
        private readonly FileUploadService _fileUpload;

        public InstantChatMessageController(
            IInstantChatMessageService service,
            FileUploadService fileUpload)
        {
            _service = service;
            _fileUpload = fileUpload;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();

            return Ok(result);
        }

        [HttpGet("room/{roomId}")]
        public async Task<IActionResult>
            GetByRoomId(string roomId)
        {
            var result = await _service .GetByRoomIdAsync(roomId);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] InstantChatMessageCreate model)
        {
            try
            {
                if (model.File != null && model.File.Length > 0)
                {
                    model.AttachmentUrl =
                        await _fileUpload.SaveChatFileAsync(model.File);
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
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(InstantChatMessageUpdate model)
        {
            var result = await _service.UpdateAsync(model);

            return Ok(new
            {
                Success = true,
                Data = result
            });
        }

        [HttpDelete("{instantMessageId}")]
        public async Task<IActionResult> Delete( int instantMessageId)
        {
            var result =await _service .DeleteAsync( instantMessageId);

            return Ok(new
            {
                Success = true,
                Data = result
            });
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult>  GetUnreadCount( string roomId)
        {
            var count = await _service.GetUnreadCountAsync( roomId);

            return Ok(new
            {
                count
            });
        }

        [HttpPut("mark-read")]
        public async Task<IActionResult> MarkRead( string roomId)
        {
            await _service.MarkAsReadAsync( roomId);

            return Ok(new
            {
                Success = true
            });
        }
    }
}