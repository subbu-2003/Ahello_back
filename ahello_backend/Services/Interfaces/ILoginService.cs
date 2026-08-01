using ahello_backend.Models.Login;

public interface ILoginService
{
    Task<(LoginResponseDto User, string Token)?>
        LoginAsync(LoginRequestDto model);

    Task<bool> SendOtpAsync(LoginOtpRequestDto model);

    Task<(LoginResponseDto User, string Token)?>
        ValidateOtpAsync(ValidateOtpRequestDto model);

    Task<(bool Exists, string Token, LoginResponseDto User)> CheckUserAsync(string email);

    Task<(LoginResponseDto User, string Token)?>
        GoogleLoginAsync(string idToken);
}