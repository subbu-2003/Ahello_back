using ahello_backend.Models.Login;
using ahello_backend.Repositorys.Interfaces;
using System.Net;
using System.Net.Mail;

namespace ahello_backend.Repositorys.Classes
{
    public class EmailRepository : IEmailRepository
    {
        private readonly EmailCon _smtp;

        public EmailRepository(EmailCon smtp)
        {
            _smtp = smtp;
        }

        // PRIVATE HELPER
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(_smtp.FromEmail, _smtp.DisplayName),
                Subject = subject,
                IsBodyHtml = true,
                Body = body
            };

            mail.To.Add(toEmail);

            using var smtpClient = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                Credentials = new NetworkCredential(_smtp.Username, _smtp.Password),
                EnableSsl = _smtp.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            await smtpClient.SendMailAsync(mail);
        }

        public async Task SendLoginOtpEmailAsync(string toEmail, string otp)
        {
            await SendEmailAsync(toEmail, "Login OTP - Ahllo", $@"
<html>
<body style='font-family:Arial;padding:20px;background:#f5f5f5;'>
    <div style='background:white;padding:20px;border-radius:10px;'>
        <h2>Login OTP</h2>
        <p>Your OTP for login is:</p>
        <h1 style='color:#2d6cdf;'>{otp}</h1>
        <p>This OTP is valid for 5 minutes.</p>
        <p>Please do not share this OTP.</p>
        <br/>
        <p>Regards,</p>
        <strong>Ahello Team</strong>
    </div>
</body>
</html>");
        }

        public async Task SendMeetingInviteEmailAsync(string toEmail, string userName, string meetingLink)
        {
            await SendEmailAsync(toEmail, "Ahllo Meeting Invitation", $@"
<html>
<body style='font-family:Arial;padding:20px;background:#f5f5f5;'>
    <div style='background:white;padding:20px;border-radius:10px;'>
        <h2>Meeting Scheduled</h2>
        <p>Hello {userName},</p>
        <p>Your meeting has been scheduled. Click below to join:</p>
        <p>
            <a href='{meetingLink}'
               style='background:#2d6cdf;color:white;padding:10px 20px;text-decoration:none;border-radius:5px;'>
                Join Meeting
            </a>
        </p>
        <p>Meeting Link: {meetingLink}</p>
        <br/>
        <p>Regards,</p>
        <strong>Ahello Team</strong>
    </div>
</body>
</html>");
        }

        public async Task SendBookingConfirmationEmailAsync(string toEmail, string clientName, string serviceName, string date, string time)
        {
            await SendEmailAsync(toEmail, "Booking Confirmed – " + serviceName, $@"
<html>
<body style='font-family:Arial;padding:20px;background:#f5f5f5;'>
    <div style='background:white;padding:20px;border-radius:10px;'>
        <h2>Hi {clientName},</h2>
        <p>Your booking has been confirmed! Here are your details:</p>
        <table style='border-collapse:collapse;width:100%;'>
            <tr>
                <td style='padding:8px;border:1px solid #ddd;'><strong>Service</strong></td>
                <td style='padding:8px;border:1px solid #ddd;'>{serviceName}</td>
            </tr>
            <tr>
                <td style='padding:8px;border:1px solid #ddd;'><strong>Date</strong></td>
                <td style='padding:8px;border:1px solid #ddd;'>{date}</td>
            </tr>
            <tr>
                <td style='padding:8px;border:1px solid #ddd;'><strong>Time</strong></td>
                <td style='padding:8px;border:1px solid #ddd;'>{time}</td>
            </tr>
        </table>
        <p>We look forward to seeing you. If you have any questions, feel free to reach out.</p>
        <br/>
        <p>Regards,</p>
        <strong>Ahllo Team</strong>
    </div>
</body>
</html>");
        }
    }
}