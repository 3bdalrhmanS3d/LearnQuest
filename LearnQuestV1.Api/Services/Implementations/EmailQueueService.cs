﻿using LearnQuestV1.Api.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Collections.Concurrent;


namespace LearnQuestV1.Api.Services.Implementations
{
    public class EmailQueueService : IEmailQueueService
    {
        private readonly ConcurrentQueue<(string Email, string FullName, string Code, string ResetLink, bool IsResend)> _emailQueue
            = new();

        private readonly IConfiguration _config;

        public EmailQueueService(IConfiguration config)
        {
            _config = config;
        }

        public void QueueEmail(string email, string fullName, string code, string resetLink = null)
        {
            _emailQueue.Enqueue((email, fullName, code, resetLink, false));
        }

        public void QueueResendEmail(string email, string fullName, string code)
        {
            _emailQueue.Enqueue((email, fullName, code, null, true));
        }

        public async Task ProcessQueueAsync()
        {
            while (_emailQueue.TryDequeue(out var item))
            {
                await SendVerificationEmailAsync(item.Email, item.FullName, item.Code, item.ResetLink, item.IsResend);
            }
        }

        private async Task SendVerificationEmailAsync(
            string emailAddress, string fullName, string verificationCode, string resetLink, bool isResend)
        {
            try
            {
                var smtpServer = _config["EmailSettings:SmtpServer"];
                var port = Convert.ToInt32(_config["EmailSettings:Port"]);
                var email = _config["EmailSettings:Email"];
                var password = _config["EmailSettings:Password"];

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Graduation Project", email));
                message.To.Add(new MailboxAddress(fullName, emailAddress));

                if (!string.IsNullOrEmpty(resetLink))
                {
                    message.Subject = "🔐 Reset Your Password";
                    message.Body = new TextPart("html") { Text = BuildResetPasswordEmail(fullName, resetLink) };
                }
                else
                {
                    message.Subject = isResend ? "🔄 Resend: Email Verification Code" : "✅ Your Email Verification Code";
                    message.Body = new TextPart("html") { Text = BuildVerificationEmail(fullName, verificationCode, isResend) };
                }

                using var client = new SmtpClient();
                await client.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(email, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                Console.WriteLine($"Email sent successfully to {emailAddress}. Resend: {isResend}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email to {emailAddress}: {ex.Message}");
            }
        }

        private string BuildVerificationEmail(string fullName, string code, bool isResend)
        {
            string title = isResend ? "🔄 Resend: Your New Verification Code" : "✅ Your Email Verification Code";
            string message = isResend
                ? "You have requested a new verification code. Please use the code below:"
                : "Please use the following verification code to complete your registration:";

            return $@"
            <html>
                <head>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            background-color: #f4f4f4;
                            text-align: center;
                        }}
                        .container {{
                            max-width: 500px;
                            margin: 20px auto;
                            padding: 20px;
                            background-color: #ffffff;
                            border-radius: 10px;
                            box-shadow: 0px 4px 6px rgba(0, 0, 0, 0.1);
                        }}
                        h2 {{
                            color: #2d89ef;
                        }}
                        .code {{
                            font-size: 24px;
                            font-weight: bold;
                            color: #d9534f;
                            background-color: #f8d7da;
                            padding: 10px 20px;
                            display: inline-block;
                            border-radius: 5px;
                        }}
                        .footer {{
                            margin-top: 20px;
                            font-size: 12px;
                            color: #777;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>{title}</h2>
                        <p>Hello, {fullName}</p>
                        <p>{message}</p>
                        <div class='code'>{code}</div>
                        <p>If you did not request this email, please ignore it.</p>
                        <div class='footer'>
                            <p>Graduation Project Team</p>
                            <p>Contact us: <a href='mailto:support@graduationproject.com'>support@graduationproject.com</a></p>
                        </div>
                    </div>
                </body>
            </html>";
        }

        private string BuildResetPasswordEmail(string fullName, string resetLink)
        {
            return $@"
            <html>
                <head>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            background-color: #f4f4f4;
                            text-align: center;
                        }}
                        .container {{
                            max-width: 500px;
                            margin: 20px auto;
                            padding: 20px;
                            background-color: #ffffff;
                            border-radius: 10px;
                            box-shadow: 0px 4px 6px rgba(0, 0, 0, 0.1);
                        }}
                        h2 {{
                            color: #2d89ef;
                        }}
                        .footer {{
                            margin-top: 20px;
                            font-size: 12px;
                            color: #777;
                        }}
                        .btn {{
                            display: inline-block;
                            padding: 10px 20px;
                            background-color: #28a745;
                            color: white;
                            text-decoration: none;
                            border-radius: 5px;
                            font-weight: bold;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>🔐 Reset Your Password</h2>
                        <p>Hello, {fullName}</p>
                        <p>You requested to reset your password. Click the button below:</p>
                        <a href='{resetLink}' class='btn'>Reset Password</a>
                        <p>If you did not request this email, please ignore it.</p>
                        <div class='footer'>
                            <p>Graduation Project Team</p>
                            <p>Contact us: <a href='mailto:support@graduationproject.com'>support@graduationproject.com</a></p>
                        </div>
                    </div>
                </body>
            </html>";
        }

        public async Task SendCustomEmailAsync(string emailAddress, string fullName, string subject, string bodyMessage)
        {
            try
            {
                var smtpServer = _config["EmailSettings:SmtpServer"];
                var port = Convert.ToInt32(_config["EmailSettings:Port"]);
                var email = _config["EmailSettings:Email"];
                var password = _config["EmailSettings:Password"];

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Graduation Project", email));
                message.To.Add(new MailboxAddress(fullName, emailAddress));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = BuildCustomHtmlEmail(fullName, bodyMessage) };

                using var client = new SmtpClient();
                await client.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(email, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }

        private string BuildCustomHtmlEmail(string fullName, string bodyMessage)
        {
            return $@"
            <html>
                <head>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            background-color: #f4f4f4;
                            text-align: center;
                        }}
                        .container {{
                            max-width: 500px;
                            margin: 20px auto;
                            padding: 20px;
                            background-color: #ffffff;
                            border-radius: 10px;
                            box-shadow: 0px 4px 6px rgba(0, 0, 0, 0.1);
                        }}
                        .footer {{
                            margin-top: 20px;
                            font-size: 12px;
                            color: #777;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>Hello, {fullName}</h2>
                        <p>{bodyMessage}</p>
                        <div class='footer'>
                            Graduation Project Team
                        </div>
                    </div>
                </body>
            </html>";
        }

    }
}
