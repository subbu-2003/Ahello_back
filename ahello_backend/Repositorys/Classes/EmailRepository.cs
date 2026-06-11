using ahello_backend.Models.Login;
using ahello_backend.Repositorys.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ahello_backend.Repositorys.Classes
{
    public class EmailRepository : IEmailRepository
    {
        private readonly EmailCon _smtp;

        public EmailRepository(EmailCon smtp)
        {
            _smtp = smtp;
        }

        // PRIVATE HELPER — truly async with MailKit
        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtp.DisplayName, _smtp.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();

            // Use StartTls if port 587, Ssl if port 465, None if port 25
            var secureOption = _smtp.Port switch
            {
                465 => SecureSocketOptions.SslOnConnect,
                587 => SecureSocketOptions.StartTls,
                _ => SecureSocketOptions.Auto
            };

            await client.ConnectAsync(_smtp.Host, _smtp.Port, secureOption);
            await client.AuthenticateAsync(_smtp.Username, _smtp.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendLoginOtpEmailAsync(string toEmail, string otp)
        {
            await SendEmailAsync(toEmail, "Login OTP - Ahllo", $@"
<html>
<body style='font-family:Arial;padding:20px;background:#f5f5f5;'>
    <div style='background:white;padding:20px;border-radius:10px;'>
        <h2>Login OTP</h2>
        <p>Your OTP for login is:</p>
        <h1 style='color:#005B71;'>{otp}</h1>
        <p>This OTP is valid for 5 minutes. Do not share it.</p>
        <br/>
        <p>Regards,</p>
        <strong>Ahllo Team</strong>
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
               style='background:#005B71;color:white;padding:10px 20px;
                      text-decoration:none;border-radius:5px;'>
                Join Meeting
            </a>
        </p>
        <br/>
        <p>Regards,</p>
        <strong>Ahllo Team</strong>
    </div>
</body>
</html>");
        }

        public async Task SendBookingConfirmationEmailAsync(
            string toEmail, string clientName, string serviceName, string date, string time)
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
        <p>We look forward to seeing you. Feel free to reach out with any questions.</p>
        <br/>
        <p>Regards,</p>
        <strong>Ahllo Team</strong>
    </div>
</body>
</html>");
        }

        public async Task SendMeetingReminderEmailAsync(string toEmail, string clientName, DateTime startTime, string meetingLink, int minutesLeft)
        {
            await SendEmailAsync(toEmail, "Meeting Reminder - Ahllo", $@"
<html>
<body style='margin:0;padding:0;background:#f4f6f9;font-family:Arial,sans-serif;'>
    <div style='max-width:600px;margin:40px auto;background:#ffffff;border-radius:12px;
                overflow:hidden;box-shadow:0 4px 10px rgba(0,0,0,0.1);'>
        <div style='background:#005B71;color:white;padding:25px;text-align:center;'>
            <h1 style='margin:0;font-size:26px;'>Meeting Reminder</h1>
        </div>
        <div style='padding:30px;'>
            <h2 style='color:#333;'>Hello {clientName},</h2>
             <p style='font-size:16px;color:#555;line-height:1.6;'>
            Your meeting starts in <strong style='color:#005B71;'>{minutesLeft} minutes</strong>.
        </p>
            <div style='background:#f8f9fc;border-left:5px solid #005B71;padding:20px;
                        margin-top:25px;border-radius:8px;'>
                <p style='margin:10px 0;font-size:15px;'>
                    <strong>Meeting Time:</strong> {startTime:dd MMM yyyy hh:mm tt}
                </p>
                <a href='{meetingLink}'
                   style='display:inline-block;margin-top:10px;background:#005B71;color:white;
                          padding:12px 20px;text-decoration:none;border-radius:6px;font-weight:bold;'>
                    Join Meeting
                </a>
            </div>
        </div>
        <div style='background:#f1f1f1;text-align:center;padding:15px;font-size:13px;color:#888;'>
            © 2026 Ahllo. All Rights Reserved.
        </div>
    </div>
</body>
</html>");
        }
    }
}