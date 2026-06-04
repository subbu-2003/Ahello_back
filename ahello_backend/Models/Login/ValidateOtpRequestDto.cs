namespace ahello_backend.Models.Login
{
    public class ValidateOtpRequestDto
    {
        public string Email { get; set; }

        public string Otp { get; set; }
    }
}
