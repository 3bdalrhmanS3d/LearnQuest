using LearnQuestV1.Api.Configuration;
using LearnQuestV1.Api.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailTemplateService> _logger;

        public EmailTemplateService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailTemplateService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public string BuildVerificationEmail(string fullName, string verificationCode, bool isResend = false)
        {
            var title = isResend ? "New Verification Code" : "Email Verification Required";
            var message = isResend
                ? "Here's your new verification code to complete your account setup:"
                : "Thank you for signing up! Please use the following code to verify your email address:";

            return BuildEmailTemplate(title, $@"
                <p>Hello, <strong>{fullName}</strong>!</p>
                <p>{message}</p>
                <div class='code'>{verificationCode}</div>
                <p>This code will expire in 30 minutes.</p>
                <p>If you did not request this email, please ignore it.</p>
            ");
        }

        public string BuildPasswordResetEmail(string fullName, string resetLink, string verificationCode)
        {
            return BuildEmailTemplate("Password Reset Request", $@"
                <p>Hello, <strong>{fullName}</strong>!</p>
                <p>We received a request to reset your password. Use the code below or click the link to reset your password:</p>
                <div class='code'>{verificationCode}</div>
                <p style='text-align: center; margin: 20px 0;'>
                    <a href='{resetLink}' class='btn'>Reset Password</a>
                </p>
                <p>This link will expire in 30 minutes.</p>
                <p>If you did not request this password reset, please ignore this email.</p>
            ");
        }

        public string BuildWelcomeEmail(string fullName)
        {
            return BuildEmailTemplate("Welcome to LearnQuest!", $@"
                <p>Hello, <strong>{fullName}</strong>!</p>
                <p>Welcome to LearnQuest! Your email has been verified successfully.</p>
                <p>You can now access all our features:</p>
                <ul style='text-align: left; margin: 20px 0;'>
                    <li>Browse and enroll in courses</li>
                    <li>Track your learning progress</li>
                    <li>Take quizzes and assessments</li>
                    <li>Earn certificates</li>
                </ul>
                <p style='text-align: center; margin: 20px 0;'>
                    <a href='{_emailSettings.WebsiteUrl}' class='btn'>Start Learning</a>
                </p>
                <p>Happy learning!</p>
            ");
        }

        public string BuildPasswordChangedEmail(string fullName)
        {
            return BuildEmailTemplate("Password Changed Successfully", $@"
                <p>Hello, <strong>{fullName}</strong>!</p>
                <p>Your password has been changed successfully.</p>
                <p><strong>When:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>
                <p>If you did not make this change, please contact our support team immediately.</p>
                <p style='text-align: center; margin: 20px 0;'>
                    <a href='mailto:{_emailSettings.SupportEmail}' class='btn' style='background-color: #dc3545;'>Contact Support</a>
                </p>
            ");
        }

        public string BuildAccountLockedEmail(string fullName, DateTime unlockTime)
        {
            return BuildEmailTemplate("Account Temporarily Locked", $@"
                <p>Hello, <strong>{fullName}</strong>!</p>
                <p>Your account has been temporarily locked due to multiple failed login attempts.</p>
                <p><strong>Unlock Time:</strong> {unlockTime:yyyy-MM-dd HH:mm} UTC</p>
                <p>For security reasons, please wait until the unlock time before attempting to log in again.</p>
                <p>If you believe this was not you, please contact our support team.</p>
                <p style='text-align: center; margin: 20px 0;'>
                    <a href='mailto:{_emailSettings.SupportEmail}' class='btn' style='background-color: #dc3545;'>Contact Support</a>
                </p>
            ");
        }

        public string BuildCustomEmail(string fullName, string content)
        {
            return BuildEmailTemplate("Message from LearnQuest", $@"
                <p>Hello, <strong>{fullName}</strong>!</p>
                {content}
            ");
        }

        private string BuildEmailTemplate(string title, string content)
        {
            var logoSection = !string.IsNullOrEmpty(_emailSettings.LogoUrl)
                ? $"<img src='{_emailSettings.LogoUrl}' alt='{_emailSettings.CompanyName}' style='max-width: 200px; height: auto; margin-bottom: 20px;'>"
                : "";

            return $@"
            <!DOCTYPE html>
            <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>{title}</title>
                    <style>
                        body {{
                            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                            line-height: 1.6;
                            margin: 0;
                            padding: 20px;
                            background-color: #f4f4f4;
                            color: #333;
                        }}
                        .email-container {{
                            max-width: 600px;
                            margin: 0 auto;
                            background-color: #ffffff;
                            border-radius: 10px;
                            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
                            overflow: hidden;
                        }}
                        .header {{
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                            color: white;
                            padding: 30px 20px;
                            text-align: center;
                        }}
                        .header h1 {{
                            margin: 0;
                            font-size: 28px;
                            font-weight: 300;
                        }}
                        .content {{
                            padding: 40px 30px;
                        }}
                        .code {{
                            font-size: 32px;
                            font-weight: bold;
                            color: #667eea;
                            background-color: #f8f9ff;
                            padding: 20px;
                            display: inline-block;
                            border-radius: 8px;
                            margin: 20px 0;
                            text-align: center;
                            width: 100%;
                            box-sizing: border-box;
                            letter-spacing: 2px;
                            border: 2px dashed #667eea;
                        }}
                        .btn {{
                            display: inline-block;
                            padding: 12px 30px;
                            background-color: #667eea;
                            color: white !important;
                            text-decoration: none;
                            border-radius: 6px;
                            font-weight: 600;
                            margin: 10px 0;
                            transition: background-color 0.3s ease;
                        }}
                        .btn:hover {{
                            background-color: #5a67d8;
                        }}
                        .footer {{
                            background-color: #f8f9fa;
                            padding: 30px;
                            text-align: center;
                            border-top: 1px solid #e9ecef;
                        }}
                        .footer p {{
                            margin: 5px 0;
                            font-size: 14px;
                            color: #6c757d;
                        }}
                        .footer a {{
                            color: #667eea;
                            text-decoration: none;
                        }}
                        ul {{
                            padding-left: 20px;
                        }}
                        li {{
                            margin: 8px 0;
                        }}
                        @media (max-width: 600px) {{
                            .email-container {{
                                margin: 0;
                                border-radius: 0;
                            }}
                            .content {{
                                padding: 20px;
                            }}
                            .code {{
                                font-size: 24px;
                                padding: 15px;
                            }}
                        }}
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='header'>
                            {logoSection}
                            <h1>{title}</h1>
                        </div>
                        <div class='content'>
                            {content}
                        </div>
                        <div class='footer'>
                            <p><strong>{_emailSettings.CompanyName}</strong></p>
                            <p>Contact us: <a href='mailto:{_emailSettings.SupportEmail}'>{_emailSettings.SupportEmail}</a></p>
                            <p><a href='{_emailSettings.WebsiteUrl}'>Visit our website</a></p>
                            <p style='font-size: 12px; color: #999; margin-top: 20px;'>
                                This email was sent automatically. Please do not reply to this email.
                            </p>
                        </div>
                    </div>
                </body>
            </html>";
        }
    }
}