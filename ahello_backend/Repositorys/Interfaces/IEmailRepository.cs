namespace ahello_backend.Repositorys.Interfaces
{
    public interface IEmailRepository
    {
        Task SendLoginOtpEmailAsync(string toEmail, string otp);

        Task SendBookingConfirmationEmailAsync(string toEmail,string clientName,string serviceName,string date,string time);
        Task SendMeetingReminderEmailAsync(string toEmail, string clientName, DateTime startTime, string meetingLink);
        Task SendMeetingInviteEmailAsync(
    string toEmail,
    string userName,
    string meetingLink);
    }
}

    
