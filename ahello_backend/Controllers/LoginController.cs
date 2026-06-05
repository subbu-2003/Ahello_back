using ahello_backend.Models.Login;
using ahello_backend.Services.Classes;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;

        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpPost]
        public async Task<IActionResult> Login(
          [FromBody] LoginRequestDto model)
        {
            var result =
                await _loginService.LoginAsync(model);

            if (result == null)
            {
                return Unauthorized(new
                {
                    status = false,
                    message = "Login failed."
                });
            }

            return Ok(new
            {
                status = true,
                message = "Login successful",

                data = new
                {
                    token = result.Value.Token,

                    user = new
                    {
                        result.Value.User.UserId,
                        result.Value.User.Email,
                        result.Value.User.UserName
                    }
                }
            });
        }
        // Controllers/LoginController.cs


        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin(
        [FromBody] GoogleLoginRequest request)
        {
            var result =
                await _loginService.GoogleLoginAsync(
                    request.IdToken);

            if (result == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid Google token"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Login successful",
                data = new
                {
                    token = result.Value.Token,
                    user = new
                    {
                        result.Value.User.UserId,
                        result.Value.User.Email,
                        result.Value.User.UserName
                    }
                }
            });
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp(
            [FromBody] LoginOtpRequestDto model)
        {
            var result = await _loginService.SendOtpAsync(model);

            if (!result)
            {
                return BadRequest(new
                {
                    status = false,
                    message = "Invalid or expired OTP"
                });
            }

            return Ok(new
            {
                status = true,
                message = "OTP sent successfully"
            });
        }
        [HttpPost("validate-otp")]
        public async Task<IActionResult> ValidateOtp(
    [FromBody] ValidateOtpRequestDto model)
        {
            var result = await _loginService.ValidateOtpAsync(model);

            if (result == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid or expired OTP."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Login successful",
                data = new
                {
                    token = result.Value.Token,

                    user = new
                    {
                        result.Value.User.UserId,
                        result.Value.User.Email,
                        result.Value.User.UserName
                    }
                }
            });
        }

    }
}
