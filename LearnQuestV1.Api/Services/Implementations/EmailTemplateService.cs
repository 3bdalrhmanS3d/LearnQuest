using LearnQuestV1.Api.Configuration;
using LearnQuestV1.Api.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly EmailSettings _emailSettings;

        public EmailTemplateService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public string BuildVerificationEmail(string fullName, string verificationCode, bool isResend = false)
        {
            var title = isResend ? "🔄 New Verification Code" : "✅ Email Verification Required";
            var message = isResend
                ? "You requested a new verification code. Here's your new code:"
                : "Welcome! Please verify your email address using the code below:";

            var template = GetBaseTemplate();
            return template
                .Replace("{{TITLE}}", title)
                .Replace("{{FULL_NAME}}", fullName)
                .Replace("{{MESSAGE}}", message)
                .Replace("{{CONTENT}}", $@"
                    <div class='verification-code'>
                        <div class='code-label'>Your Verification Code:</div>
                        <div class='code'>{verificationCode}</div>
                        <div class='code-note'>This code expires in 30 minutes</div>
                    </div>
                ")
                .Replace("{{FOOTER_MESSAGE}}", "If you didn't create an account, please ignore this email.");
        }

        public string BuildPasswordResetEmail(string fullName, string resetLink, string verificationCode)
        {
            var template = GetBaseTemplate();
            return template
                .Replace("{{TITLE}}", "🔐 Password Reset Request")
                .Replace("{{FULL_NAME}}", fullName)
                .Replace("{{MESSAGE}}", "You requested to reset your password. Use the button below or the verification code:")
                .Replace("{{CONTENT}}", $@"
                    <div class='action-section'>
                        <a href='{resetLink}' class='btn btn-primary'>Reset Password</a>
                        <div class='or-divider'>
                            <span>OR</span>
                        </div>
                        <div class='verification-code'>
                            <div class='code-label'>Verification Code:</div>
                            <div class='code'>{verificationCode}</div>
                        </div>
                        <div class='code-note'>This link and code expire in 30 minutes</div>
                    </div>
                ")
                .Replace("{{FOOTER_MESSAGE}}", "If you didn't request a password reset, please ignore this email and contact support if you're concerned.");
        }

        public string BuildWelcomeEmail(string fullName)
        {
            var template = GetBaseTemplate();
            return template
                .Replace("{{TITLE}}", "🎉 Welcome to LearnQuest!")
                .Replace("{{FULL_NAME}}", fullName)
                .Replace("{{MESSAGE}}", "Your account has been successfully verified. Welcome to our learning platform!")
                .Replace("{{CONTENT}}", $@"
                    <div class='welcome-section'>
                        <div class='welcome-icon'>🚀</div>
                        <h3>What's Next?</h3>
                        <ul class='feature-list'>
                            <li>📚 Browse our course catalog</li>
                            <li>🎯 Set your learning goals</li>
                            <li>👨‍🏫 Connect with instructors</li>
                            <li>🏆 Track your progress</li>
                        </ul>
                        <a href='{_emailSettings.FrontendUrl}/dashboard' class='btn btn-primary'>Get Started</a>
                    </div>
                ")
                .Replace("{{FOOTER_MESSAGE}}", "Happy learning!");
        }

        public string BuildPasswordChangedEmail(string fullName)
        {
            var template = GetBaseTemplate();
            return template
                .Replace("{{TITLE}}", "🔒 Password Changed Successfully")
                .Replace("{{FULL_NAME}}", fullName)
                .Replace("{{MESSAGE}}", "Your password has been changed successfully.")
                .Replace("{{CONTENT}}", $@"
                    <div class='security-alert'>
                        <div class='alert-icon'>🛡️</div>
                        <p><strong>Security Notice:</strong> Your password was changed on {DateTime.UtcNow:MMM dd, yyyy} at {DateTime.UtcNow:HH:mm} UTC.</p>
                        <p>If you didn't make this change, please contact our support team immediately.</p>
                        <a href='{_emailSettings.FrontendUrl}/contact' class='btn btn-warning'>Contact Support</a>
                    </div>
                ")
                .Replace("{{FOOTER_MESSAGE}}", "Keep your account secure by using a strong, unique password.");
        }

        public string BuildAccountLockedEmail(string fullName, DateTime unlockTime)
        {
            var template = GetBaseTemplate();
            return template
                .Replace("{{TITLE}}", "🔐 Account Temporarily Locked")
                .Replace("{{FULL_NAME}}", fullName)
                .Replace("{{MESSAGE}}", "Your account has been temporarily locked due to multiple failed login attempts.")
                .Replace("{{CONTENT}}", $@"
                    <div class='security-alert'>
                        <div class='alert-icon'>⚠️</div>
                        <p><strong>Account Status:</strong> Temporarily Locked</p>
                        <p><strong>Unlock Time:</strong> {unlockTime:MMM dd, yyyy HH:mm} UTC</p>
                        <p>For your security, we've temporarily locked your account. You can try logging in again after the unlock time.</p>
                        <p>If you believe this was not you, please contact our support team.</p>
                        <a href='{_emailSettings.FrontendUrl}/contact' class='btn btn-warning'>Contact Support</a>
                    </div>
                ")
                .Replace("{{FOOTER_MESSAGE}}", "If you're experiencing issues, our support team is here to help.");
        }

        public string BuildCustomEmail(string fullName, string subject, string bodyMessage)
        {
            var template = GetBaseTemplate();
            return template
                .Replace("{{TITLE}}", subject)
                .Replace("{{FULL_NAME}}", fullName)
                .Replace("{{MESSAGE}}", bodyMessage)
                .Replace("{{CONTENT}}", "")
                .Replace("{{FOOTER_MESSAGE}}", "");
        }

        public string BuildNotificationEmail(string fullName, string title, string message, string? actionUrl = null)
        {
            var actionButton = string.IsNullOrEmpty(actionUrl)
                ? ""
                : $"<a href='{actionUrl}' class='btn btn-primary'>View Details</a>";

            var template = GetBaseTemplate();
            return template
                .Replace("{{TITLE}}", title)
                .Replace("{{FULL_NAME}}", fullName)
                .Replace("{{MESSAGE}}", message)
                .Replace("{{CONTENT}}", actionButton)
                .Replace("{{FOOTER_MESSAGE}}", "");
        }

        private string GetBaseTemplate()
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{{{{TITLE}}}}</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f5f7fa;
            color: #333;
            line-height: 1.6;
        }}
        
        .email-container {{
            max-width: 600px;
            margin: 20px auto;
            background-color: #ffffff;
            border-radius: 12px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
            overflow: hidden;
        }}
        
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            text-align: center;
            padding: 30px 20px;
        }}
        
        .header h1 {{
            font-size: 24px;
            font-weight: 600;
            margin-bottom: 5px;
        }}
        
        .header .subtitle {{
            font-size: 14px;
            opacity: 0.9;
        }}
        
        .content {{
            padding: 40px 30px;
        }}
        
        .greeting {{
            font-size: 18px;
            margin-bottom: 20px;
            color: #2c3e50;
        }}
        
        .message {{
            font-size: 16px;
            margin-bottom: 30px;
            color: #555;
        }}
        
        .verification-code {{
            text-align: center;
            margin: 30px 0;
        }}
        
        .code-label {{
            font-size: 14px;
            color: #666;
            margin-bottom: 10px;
        }}
        
        .code {{
            display: inline-block;
            font-size: 28px;
            font-weight: bold;
            color: #e74c3c;
            background-color: #fef5f5;
            padding: 15px 25px;
            border-radius: 8px;
            border: 2px dashed #e74c3c;
            letter-spacing: 4px;
            font-family: 'Courier New', monospace;
        }}
        
        .code-note {{
            font-size: 12px;
            color: #999;
            margin-top: 10px;
        }}
        
        .btn {{
            display: inline-block;
            padding: 12px 30px;
            text-decoration: none;
            border-radius: 6px;
            font-weight: 600;
            text-align: center;
            transition: all 0.3s ease;
            font-size: 16px;
        }}
        
        .btn-primary {{
            background-color: #3498db;
            color: white;
        }}
        
        .btn-primary:hover {{
            background-color: #2980b9;
        }}
        
        .btn-warning {{
            background-color: #f39c12;
            color: white;
        }}
        
        .btn-warning:hover {{
            background-color: #d68910;
        }}
        
        .action-section {{
            text-align: center;
            margin: 30px 0;
        }}
        
        .or-divider {{
            margin: 20px 0;
            text-align: center;
            position: relative;
        }}
        
        .or-divider:before {{
            content: '';
            position: absolute;
            top: 50%;
            left: 0;
            right: 0;
            height: 1px;
            background-color: #ddd;
        }}
        
        .or-divider span {{
            background-color: white;
            padding: 0 15px;
            color: #999;
            font-size: 14px;
        }}
        
        .security-alert {{
            background-color: #fff3cd;
            border: 1px solid #ffeaa7;
            border-radius: 6px;
            padding: 20px;
            margin: 20px 0;
        }}
        
        .alert-icon {{
            font-size: 24px;
            margin-bottom: 10px;
        }}
        
        .welcome-section {{
            text-align: center;
        }}
        
        .welcome-icon {{
            font-size: 48px;
            margin-bottom: 20px;
        }}
        
        .feature-list {{
            text-align: left;
            max-width: 300px;
            margin: 20px auto;
        }}
        
        .feature-list li {{
            margin: 10px 0;
            padding-left: 10px;
        }}
        
        .footer {{
            background-color: #f8f9fa;
            padding: 30px;
            text-align: center;
            border-top: 1px solid #e9ecef;
        }}
        
        .footer-message {{
            font-size: 14px;
            color: #666;
            margin-bottom: 20px;
        }}
        
        .company-info {{
            font-size: 12px;
            color: #999;
        }}
        
        .company-info a {{
            color: #3498db;
            text-decoration: none;
        }}
        
        .social-links {{
            margin: 15px 0;
        }}
        
        .social-links a {{
            display: inline-block;
            margin: 0 10px;
            text-decoration: none;
            color: #666;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            <h1>{{{{TITLE}}}}</h1>
            <div class='subtitle'>LearnQuest Learning Platform</div>
        </div>
        
        <div class='content'>
            <div class='greeting'>Hello, {{{{FULL_NAME}}}}!</div>
            <div class='message'>{{{{MESSAGE}}}}</div>
            {{{{CONTENT}}}}
        </div>
        
        <div class='footer'>
            <div class='footer-message'>{{{{FOOTER_MESSAGE}}}}</div>
            <div class='company-info'>
                <strong>LearnQuest Team</strong><br>
                <a href='mailto:{_emailSettings.SupportEmail}'>{_emailSettings.SupportEmail}</a><br>
                <a href='{_emailSettings.FrontendUrl}'>Visit our website</a>
            </div>
            <div class='social-links'>
                <a href='#'>📧 Email</a>
                <a href='#'>🐦 Twitter</a>
                <a href='#'>📘 Facebook</a>
            </div>
        </div>
    </div>
</body>
</html>";
        }
    }
}