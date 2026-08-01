using ahello_backend.Models.Login;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface ILoginRepository
    {
        Task<LoginResponseDto> LoginAsync(string email);
        Task<LoginResponseDto> GoogleLoginAsync(string idToken);
        Task<bool> SendOtpAsync(string email);

        Task<(bool Exists, string Token, LoginResponseDto User)> CheckUserAsync(string email);
        Task<LoginResponseDto> ValidateOtpAsync(
    string email,
    string otp);

         string GenerateJwtToken(LoginResponseDto user);
    }
}
