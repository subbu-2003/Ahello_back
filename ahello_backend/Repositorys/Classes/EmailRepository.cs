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
        private async Task SendEmailAsync(
    string toEmail,
    string subject,
    string body,
    byte[]? attachment = null,
    string? attachmentName = null)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(_smtp.DisplayName, _smtp.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = body
            };

            // Attach PDF
            if (attachment != null)
            {
                builder.Attachments.Add(
                    attachmentName ?? "Invoice.pdf",
                    attachment,
                    ContentType.Parse("application/pdf"));
            }

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();

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
     string time,
     byte[]? invoicePdf = null)
        {
            await SendEmailAsync(
                toEmail,
                "Booking Confirmed - " + serviceName,
            $@"
                <!DOCTYPE html>
                <html>
                <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='margin:0;padding:0;background:#f5f7fa;font-family:Segoe UI,Arial;'>
                <table width='100%' cellpadding='0' cellspacing='0'>
                <tr><td align='center'>
                <table width=""600"" cellpadding=""0"" cellspacing=""0""
       style=""background:#ffffff;
              border-radius:22px;
              overflow:hidden;"">

                <!-- Header Banner -->
                <tr>
                <td style='background:#005B71;padding:18px 20px;'>
                  <div style='white-space:nowrap;font-size:0;'>
                    <span style='font-size:20px;font-weight:bold;color:#ffffff;letter-spacing:1px;'>Ahllo</span>
                    <span style='font-size:20px;color:#ffffff;margin:0 6px;'>-</span>
                    <span style='font-size:20px;font-weight:bold;color:#4DD9C0;'>Booking Confirmed</span>
                  </div>
                </td>
                </tr>

                <!-- Body -->
                <tr>
                <td style='padding:24px 40px;background:#ffffff;'>

                  <p style='font-size:15px;color:#333;margin:0 0 8px 0;'>Hi <strong>{clientName}</strong>,</p>
                  <p style='font-size:14px;color:#555;line-height:22px;margin:0 0 18px 0;'>
                    Your appointment has been successfully scheduled. Here are the details:
                  </p>

                
             <!-- Details Card -->
                <table width='100%' cellpadding='0' cellspacing='0'
                  style='border-collapse:collapse;border:1px solid #e0e0e0;border-radius:8px;table-layout:fixed;'>
                  <tr>
                    <td colspan='2' style='padding:0;border-radius:8px;'>
                      <table width='100%' cellpadding='0' cellspacing='0'
                        style='border-collapse:collapse;font-size:14px;color:#333;table-layout:fixed;'>
                        <tr style='background:#f9f9f9;'>
                          <td valign='top' style='padding:11px 16px;font-weight:bold;color:#005B71;width:90px;border-bottom:1px solid #eee;border-right:1px solid #eee;'>Service</td>
                          <td valign='top' style='padding:11px 16px;border-bottom:1px solid #eee;'>{serviceName}</td>
                        </tr>
                        <tr>
                          <td valign='top' style='padding:11px 16px;font-weight:bold;color:#005B71;border-bottom:1px solid #eee;border-right:1px solid #eee;'>Date</td>
                          <td valign='top' style='padding:11px 16px;border-bottom:1px solid #eee;'>{date}</td>
                        </tr>
                        <tr>
                          <td valign='top' style='padding:11px 16px;font-weight:bold;color:#005B71;border-right:1px solid #eee;background:#f9f9f9;'>Time</td>
                          <td valign='top' style='padding:11px 16px;background:#f9f9f9;'>{time}</td>
                        </tr>
                      </table>
                    </td>
                  </tr>
                </table>

                  <p style='margin-top:20px;font-size:13px;color:#555;line-height:21px;'>
                    If you need to reschedule or have any questions, feel free to contact us.
                  </p>
                  <p style='margin-top:12px;font-size:14px;color:#005B71;font-weight:600;'>Thank you!</p>

                </td>
                </tr>

                <!-- Footer -->
                <tr>
                <td style='padding:14px 40px;background:#f5f7fa;border-top:1px solid #eee;font-size:12px;color:#888;'>
                  <a href='https://www.ahllo.com' style='color:#888;text-decoration:none;'>www.ahllo.com</a>
                </td>
                </tr>

                </table>
                </td></tr>
                </table>
                </body>
                </html>
                ", invoicePdf,
    "Invoice.pdf");
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