using ahello_backend.Models.Login;

namespace ahello_backend.Services.Interfaces
{
    public interface ILoginService
    {
        Task<(LoginResponseDto User, string Token)?>
    LoginAsync(LoginRequestDto model);
        Task<bool> SendOtpAsync(LoginOtpRequestDto model);
        Task<(LoginResponseDto User, string Token)?> ValidateOtpAsync(
    ValidateOtpRequestDto model);
    }
}
