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
            await SendEmailAsync(toEmail, "Meeting Invitation - Ahllo", $@"
                <html>
                <body style='margin:0;padding:0;background:#ffffff;font-family:Arial,sans-serif;'>
                    <div style='max-width:560px;margin:40px auto;background:#ffffff;border-radius:16px;
                                border:1px solid #e8edf2;overflow:hidden;'>

                        <!-- HEADER -->
                        <div style='background:#005B71;padding:32px;text-align:center;'>
                            <h1 style='margin:0;color:#ffffff;font-size:24px;letter-spacing:0.5px;'>
                                Meeting Invitation
                            </h1>
                        </div>

                        <!-- BODY -->
                        <div style='padding:36px 40px;'>

                            <p style='margin:0 0 8px;font-size:18px;font-weight:bold;color:#1a1a1a;'>
                                Hello {userName},
                            </p>

                            <p style='margin:0 0 28px;font-size:15px;color:#555555;line-height:1.6;'>
                                Your meeting has been scheduled. Click the button below to join when it's time.
                            </p>

                            <!-- CARD -->
                            <div style='border:1px solid #e0e7ef;border-radius:12px;padding:24px;margin-bottom:28px;'>
                                <p style='margin:0 0 6px;font-size:12px;text-transform:uppercase;
                                          letter-spacing:1px;color:#999999;'>Meeting Link</p>
                                <p style='margin:0 0 20px;font-size:14px;color:#555555;word-break:break-all;'>
                                    {meetingLink}
                                </p>
                                <a href='{meetingLink}'
                                   style='display:inline-block;background:#005B71;color:#ffffff;
                                          padding:13px 28px;border-radius:8px;font-size:15px;
                                          font-weight:bold;text-decoration:none;'>
                                    Join Meeting →
                                </a>
                            </div>

                            <p style='margin:0;font-size:13px;color:#aaaaaa;line-height:1.6;'>
                                If you did not book this meeting, please ignore this email.
                            </p>
                        </div>

                        <!-- FOOTER -->
                        <div style='border-top:1px solid #f0f0f0;padding:18px;text-align:center;'>
                            <p style='margin:0;font-size:12px;color:#bbbbbb;'>
                                © 2026 Ahllo. All Rights Reserved.
                            </p>
                        </div>

                    </div>
                </body>
                </html>");
                        }

        public async Task SendBookingConfirmationEmailAsync(
     string toEmail,
     string clientName,
     string serviceName,
     string date,
     string time)
        {
            await SendEmailAsync(
                toEmail,
                "🎉 Booking Confirmed - " + serviceName,
                $@"

<!DOCTYPE html>
<html>

<body style='margin:0;padding:0;background:#F8FAFC;font-family:Segoe UI,Arial,sans-serif;'>

<table width='100%' cellpadding='0' cellspacing='0' style='background:#F8FAFC;padding:30px;'>

<tr>
<td align='center'>

<table width='700' cellpadding='0' cellspacing='0'
style='background:#ffffff;border-radius:16px;overflow:hidden;
box-shadow:0 5px 20px rgba(0,0,0,.08);'>

<!-- HEADER -->

<tr>
<td style='background:#005B71;padding:40px;color:white;'>

<h1 style='margin:0;font-size:42px;font-weight:bold;'>
Ahllo
</h1>

<p style='margin:5px 0 30px;color:#dbeafe;'>
CONNECT • CONSULT • GROW
</p>

<h2 style='margin:0;font-size:42px;font-weight:700;'>
Booking
<span style='color:#2DD4BF;'>
Confirmed!
</span>
</h2>

<p style='margin-top:20px;
font-size:20px;
color:#ffffff;'>
Your appointment is all set.<br/>
We look forward to meeting with you.
</p>

</td>
</tr>

<!-- COLOR BAR -->

<tr>
<td height='8'
style='background:linear-gradient(90deg,#2DD4BF,#5E5CE6);'>
</td>
</tr>

<!-- CONTENT -->

<tr>

<td style='padding:40px;'>

<h2 style='margin:0;color:#005B71;font-size:34px;'>

Hi {clientName},

</h2>

<p style='margin-top:20px;
font-size:18px;
color:#555;
line-height:30px;'>

Your booking has been confirmed!
<br>
We're excited to meet you.

</p>

<!-- BOOKING DETAILS -->

<table width='100%'
style='margin-top:35px;
border:1px solid #dbeafe;
border-radius:12px;
overflow:hidden;
border-collapse:collapse;'>

<tr>

<td colspan='2'
align='center'
style='background:#F8FAFC;
padding:25px;'>

<h2 style='margin:0;
color:#005B71;'>

Booking Details

</h2>

</td>

</tr>

<tr>

<td width='180'
style='padding:18px;
border-top:1px solid #eee;
font-weight:bold;
color:#005B71;'>

👜 Service

</td>

<td style='padding:18px;
border-top:1px solid #eee;
font-size:18px;'>

{serviceName}

</td>

</tr>

<tr>

<td style='padding:18px;
border-top:1px solid #eee;
font-weight:bold;
color:#005B71;'>

📅 Date

</td>

<td style='padding:18px;
border-top:1px solid #eee;
font-size:18px;'>

{date}

</td>

</tr>

<tr>

<td style='padding:18px;
border-top:1px solid #eee;
font-weight:bold;
color:#005B71;'>

🕒 Time

</td>

<td style='padding:18px;
border-top:1px solid #eee;
font-size:18px;'>

{time}

</td>

</tr>

</table>

<!-- THANK YOU -->

<table width='100%'
style='margin-top:35px;
background:#F5F3FF;
border:1px solid #DDD6FE;
border-radius:12px;'>

<tr>

<td style='padding:25px;'>

<h3 style='margin:0;color:#5E5CE6;'>

Thank You!

</h3>

<p style='margin-top:15px;
font-size:17px;
line-height:28px;
color:#555;'>

✔ You'll receive a reminder before your appointment.

<br><br>

If you have any questions,
simply reply to this email.

</p>

</td>

</tr>

</table>

<!-- FOOTER -->

<div style='margin-top:40px;
text-align:center;'>

<p style='font-size:24px;
color:#005B71;
font-weight:bold;'>

We appreciate your trust in us.

</p>

<p style='font-size:20px;
color:#2DD4BF;
font-weight:bold;'>

See you soon!

</p>

</div>

<hr style='margin:35px 0;border:none;border-top:1px solid #e5e7eb;'>

<table width='100%'>

<tr>

<td align='center'
style='color:#666;font-size:15px;'>

📧 support@ahllo.com

&nbsp;&nbsp;&nbsp;&nbsp;

📞 +91 98765 43210

&nbsp;&nbsp;&nbsp;&nbsp;

🌐 www.ahllo.com

</td>

</tr>

</table>

</td>

</tr>

</table>

</td>
</tr>

</table>

</body>
</html>

");
        }

        public async Task SendMeetingReminderEmailAsync(string toEmail, string clientName, DateTime startTime, string meetingLink, int minutesLeft)
        {
            await SendEmailAsync(toEmail, "Meeting Reminder - Ahllo", $@"
            <html>
            <body style='margin:0;padding:0;background:#ffffff;font-family:Arial,sans-serif;'>
                <div style='max-width:560px;margin:40px auto;background:#ffffff;border-radius:16px;
                            border:1px solid #e8edf2;overflow:hidden;'>

                    <!-- HEADER -->
                    <div style='background:#005B71;padding:32px;text-align:center;'>
                        <h1 style='margin:0;color:#ffffff;font-size:24px;letter-spacing:0.5px;'>
                            Meeting Reminder
                        </h1>
                    </div>

                    <!-- BODY -->
                    <div style='padding:36px 40px;'>

                        <p style='margin:0 0 8px;font-size:18px;font-weight:bold;color:#1a1a1a;'>
                            Hello {clientName},
                        </p>

                        <p style='margin:0 0 28px;font-size:15px;color:#555555;line-height:1.6;'>
                            Your meeting is starting in
                            <span style='color:#005B71;font-weight:bold;font-size:17px;'>{minutesLeft} minutes</span>.
                            Get ready to join!
                        </p>

                        <!-- MEETING CARD -->
                        <div style='border:1px solid #e0e7ef;border-radius:12px;padding:24px;margin-bottom:28px;'>
                            <p style='margin:0 0 6px;font-size:12px;text-transform:uppercase;
                                      letter-spacing:1px;color:#999999;'>Meeting Time</p>
                            <p style='margin:0 0 20px;font-size:16px;font-weight:bold;color:#1a1a1a;'>
                                {startTime:dd MMM yyyy} &nbsp;·&nbsp; {startTime:hh:mm tt}
                            </p>
                            <a href='{meetingLink}'
                               style='display:inline-block;background:#005B71;color:#ffffff;
                                      padding:13px 28px;border-radius:8px;font-size:15px;
                                      font-weight:bold;text-decoration:none;'>
                                Join Meeting →
                            </a>
                        </div>

                        <p style='margin:0;font-size:13px;color:#aaaaaa;line-height:1.6;'>
                            If you did not book this meeting, please ignore this email.
                        </p>
                    </div>

                    <!-- FOOTER -->
                    <div style='border-top:1px solid #f0f0f0;padding:18px;text-align:center;'>
                        <p style='margin:0;font-size:12px;color:#bbbbbb;'>
                            © 2026 Ahllo. All Rights Reserved.
                        </p>
                    </div>

                </div>
            </body>
            </html>");
                    }
        public async Task SendRescheduleConfirmationEmailAsync(
        string toEmail, string clientName, string serviceName, string date, string time)
        {
            await SendEmailAsync(toEmail, "Booking Rescheduled – " + serviceName, $@"
            <html>
            <body style='font-family:Arial;padding:20px;background:#f5f5f5;'>
                <div style='background:white;padding:20px;border-radius:10px;'>
                    <h2>Hi {clientName},</h2>
                    <p>Your booking has been <strong style='color:#005B71;'>rescheduled</strong>. Here are your updated details:</p>
                    <table style='border-collapse:collapse;width:100%;'>
                        <tr>
                            <td style='padding:8px;border:1px solid #ddd;'><strong>Service</strong></td>
                            <td style='padding:8px;border:1px solid #ddd;'>{serviceName}</td>
                        </tr>
                        <tr>
                            <td style='padding:8px;border:1px solid #ddd;'><strong>New Date</strong></td>
                            <td style='padding:8px;border:1px solid #ddd;'>{date}</td>
                        </tr>
                        <tr>
                            <td style='padding:8px;border:1px solid #ddd;'><strong>New Time</strong></td>
                            <td style='padding:8px;border:1px solid #ddd;'>{time}</td>
                        </tr>
                    </table>
                    <p>If you have any questions, feel free to reach out.</p>
                    <br/>
                    <p>Regards,</p>
                    <strong>Ahllo Team</strong>
                </div>
            </body>
            </html>");
                    }
    }
}