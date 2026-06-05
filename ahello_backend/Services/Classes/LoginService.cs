using ahello_backend.Models.Login;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class LoginService : ILoginService
    {
        private readonly ILoginRepository _loginRepository;

        public LoginService(ILoginRepository loginRepository)
        {
            _loginRepository = loginRepository;
        }

        public async Task<(LoginResponseDto User, string Token)?>
      LoginAsync(LoginRequestDto model)
        {
            var user =
                await _loginRepository.LoginAsync(
                    model.Email);

            if (user == null)
                return null;

            var token =
                _loginRepository.GenerateJwtToken(user);

            return (user, token);
        }


        public async Task<(LoginResponseDto User, string Token)?>
    GoogleLoginAsync(string idToken)
        {
            var user =
                await _loginRepository.GoogleLoginAsync(idToken);

            if (user == null)
                return null;

            var token =
                _loginRepository.GenerateJwtToken(user);

            return (user, token);
        }
        public async Task<bool> SendOtpAsync(
    LoginOtpRequestDto model)
        {
            return await _loginRepository.SendOtpAsync(
                model.Email);
        }

        public async Task<(LoginResponseDto User, string Token)?>
            ValidateOtpAsync(
                ValidateOtpRequestDto model)
        {
            var user =
                await _loginRepository.ValidateOtpAsync(
                    model.Email,
                    model.Otp);

            if (user == null)
                return null;

            var token =
                _loginRepository.GenerateJwtToken(user);

            return (user, token);
        }
    }
}
