using ahello_backend.Models.Login;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface ILoginRepository
    {
        Task<LoginResponseDto> LoginAsync(string email);
        Task<bool> SendOtpAsync(string email);
        Task<LoginResponseDto> ValidateOtpAsync(
    string email,
    string otp);

         string GenerateJwtToken(LoginResponseDto user);
    }
}
