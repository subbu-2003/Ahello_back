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