using ahello_backend.Models.Webinar;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebinarController : ControllerBase
    {
        private readonly IWebinarService _service;

        public WebinarController(
            IWebinarService service)
        {
            _service = service;
        }


        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateWebinarRequest request)
        {
            try
            {
                var webinarId =
                    await _service.CreateAsync(request);

                return Ok(new
                {
                    success = true,
                    message = "Webinar created successfully.",
                    webinarId
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
        }


        // GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var webinars =
                await _service.GetAllAsync();

            return Ok(new
            {
                success = true,
                data = webinars
            });
        }


        // GET BY ID
        [HttpGet("{webinarId}")]
        public async Task<IActionResult> GetById(
            int webinarId)
        {
            var webinar =
                await _service.GetByIdAsync(webinarId);

            if (webinar == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Webinar not found."
                });
            }

            return Ok(new
            {
                success = true,
                data = webinar
            });
        }


        // UPDATE
        [HttpPut("{webinarId}")]
        public async Task<IActionResult> Update(
            int webinarId,
            [FromBody] UpdateWebinarRequest request)
        {
            var result =
                await _service.UpdateAsync(
                    webinarId,
                    request);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Webinar not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Webinar updated successfully."
            });
        }


        // CANCEL
        [HttpDelete("{webinarId}")]
        public async Task<IActionResult> Delete(
            int webinarId)
        {
            var result =
                await _service.DeleteAsync(webinarId);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Webinar not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Webinar cancelled successfully."
            });
        }


        // ADMIN APPROVE WEBINAR
        [HttpPut("{webinarId}/approve")]
        public async Task<IActionResult> ApproveWebinar(
            int webinarId,
            [FromQuery] int adminId)
        {
            try
            {
                var result =
                    await _service.ApproveWebinarAsync(
                        webinarId,
                        adminId);

                return Ok(new
                {
                    success = result,
                    message = "Webinar approved successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        // ADMIN REJECT WEBINAR
        [HttpPut("{webinarId}/reject")]
        public async Task<IActionResult> RejectWebinar(
            int webinarId,
            [FromQuery] int adminId,
            [FromBody] string reason)
        {
            try
            {
                var result =
                    await _service.RejectWebinarAsync(
                        webinarId,
                        adminId,
                        reason);

                return Ok(new
                {
                    success = result,
                    message = "Webinar rejected successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        // UPLOAD RECORDED VIDEO
        [HttpPut("{webinarId}/video")]
        public async Task<IActionResult> UploadVideo(
            int webinarId,
            [FromBody] string videoUrl)
        {
            try
            {
                var result =
                    await _service.UploadVideoAsync(
                        webinarId,
                        videoUrl);

                return Ok(new
                {
                    success = result,
                    message = "Video uploaded successfully and sent for approval."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        // ADMIN APPROVE VIDEO
        [HttpPut("{webinarId}/video/approve")]
        public async Task<IActionResult> ApproveVideo(
            int webinarId,
            [FromQuery] int adminId)
        {
            try
            {
                var result =
                    await _service.ApproveVideoAsync(
                        webinarId,
                        adminId);

                return Ok(new
                {
                    success = result,
                    message = "Video approved successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        // ADMIN REJECT VIDEO
        [HttpPut("{webinarId}/video/reject")]
        public async Task<IActionResult> RejectVideo(
            int webinarId,
            [FromQuery] int adminId,
            [FromBody] string reason)
        {
            try
            {
                var result =
                    await _service.RejectVideoAsync(
                        webinarId,
                        adminId,
                        reason);

                return Ok(new
                {
                    success = result,
                    message = "Video rejected successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        // PUBLISH
        [HttpPut("{webinarId}/publish")]
        public async Task<IActionResult> Publish(
            int webinarId)
        {
            try
            {
                var result =
                    await _service.PublishAsync(
                        webinarId);

                return Ok(new
                {
                    success = result,
                    message = "Webinar published successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        // REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterWebinarRequest request)
        {
            try
            {
                var registrationId =
                    await _service.RegisterAsync(request);

                return Ok(new
                {
                    success = true,
                    message = "Webinar registration successful.",
                    registrationId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        // GET USER REGISTRATION
        [HttpGet("{webinarId}/registration/{userId}")]
        public async Task<IActionResult> GetRegistration(
            int webinarId,
            int userId)
        {
            var registration =
                await _service.GetRegistrationAsync(
                    webinarId,
                    userId);

            if (registration == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Registration not found."
                });
            }

            return Ok(new
            {
                success = true,
                data = registration
            });
        }


        // GET ALL PARTICIPANTS
        [HttpGet("{webinarId}/registrations")]
        public async Task<IActionResult> GetRegistrations(
            int webinarId)
        {
            var registrations =
                await _service.GetRegistrationsAsync(
                    webinarId);

            return Ok(new
            {
                success = true,
                data = registrations
            });
        }
    }
}
