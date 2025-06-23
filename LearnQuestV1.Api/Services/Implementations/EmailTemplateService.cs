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

        public string BuildEnhancedVerificationEmail(string fullName, string verificationCode, string verificationLink)
        {
            return BuildEmailTemplate("Email Verification Required", $@"
                <p>Hello, <strong>{fullName}</strong>!</p>
                <p>Thank you for signing up! Please verify your email address using one of the methods below:</p>
                
                <div style='margin: 30px 0; padding: 20px; background-color: #f8f9ff; border-radius: 8px; border: 1px solid #e0e6ff;'>
                    <h3 style='margin: 0 0 15px 0; color: #667eea; font-size: 18px;'>Option 1: Enter Verification Code</h3>
                    <p style='margin: 0 0 10px 0;'>Enter this code in the verification page:</p>
                    <div class='code'>{verificationCode}</div>
                </div>

                <div style='margin: 30px 0; padding: 20px; background-color: #f0f9ff; border-radius: 8px; border: 1px solid #bfdbfe;'>
                    <h3 style='margin: 0 0 15px 0; color: #3b82f6; font-size: 18px;'>Option 2: Click Verification Link</h3>
                    <p style='margin: 0 0 15px 0;'>Or simply click the button below to verify instantly:</p>
                    <p style='text-align: center; margin: 0;'>
                        <a href='{verificationLink}' class='btn' style='background-color: #3b82f6; padding: 15px 30px; font-size: 16px;'>
                            ✅ Verify Email Address
                        </a>
                    </p>
                </div>

                <div style='margin: 20px 0; padding: 15px; background-color: #fef2f2; border-radius: 6px; border-left: 4px solid #ef4444;'>
                    <p style='margin: 0; font-size: 14px; color: #dc2626;'>
                        <strong>⚠️ Important:</strong> This verification will expire in 30 minutes for security reasons.
                    </p>
                </div>

                <p style='font-size: 14px; color: #6b7280; margin-top: 20px;'>
                    If you did not create an account with us, please ignore this email.
                </p>
            ");
        }

        public string BuildPasswordResetEmail(string fullName, string resetLink, string verificationCode)
        {
            return BuildEmailTemplate("Password Reset Request", $@"
                <p>Hello, <strong>{fullName}</strong>!</p>
                <p>We received a request to reset your password. You can reset it using either method below:</p>
                
                <div style='margin: 30px 0; padding: 20px; background-color: #f8f9ff; border-radius: 8px; border: 1px solid #e0e6ff;'>
                    <h3 style='margin: 0 0 15px 0; color: #667eea; font-size: 18px;'>Option 1: Use Reset Code</h3>
                    <p style='margin: 0 0 10px 0;'>Enter this code in the password reset page:</p>
                    <div class='code'>{verificationCode}</div>
                </div>

                <div style='margin: 30px 0; padding: 20px; background-color: #f0f9ff; border-radius: 8px; border: 1px solid #bfdbfe;'>
                    <h3 style='margin: 0 0 15px 0; color: #3b82f6; font-size: 18px;'>Option 2: Click Reset Link</h3>
                    <p style='margin: 0 0 15px 0;'>Or click the button below to reset directly:</p>
                    <p style='text-align: center; margin: 0;'>
                        <a href='{resetLink}' class='btn' style='background-color: #dc3545; padding: 15px 30px; font-size: 16px;'>
                            🔒 Reset Password
                        </a>
                    </p>
                </div>

                <div style='margin: 20px 0; padding: 15px; background-color: #fef2f2; border-radius: 6px; border-left: 4px solid #ef4444;'>
                    <p style='margin: 0; font-size: 14px; color: #dc2626;'>
                        <strong>⚠️ Security Notice:</strong> This reset link will expire in 30 minutes.
                    </p>
                </div>

                <p style='font-size: 14px; color: #6b7280; margin-top: 20px;'>
                    If you did not request this password reset, please ignore this email and your password will remain unchanged.
                </p>
            ");
        }

        public string BuildWelcomeEmail(string fullName)
        {
            return BuildEmailTemplate("Welcome to LearnQuest!", $@"
                <div style='text-align: center; margin: 20px 0;'>
                    <h2 style='color: #667eea; font-size: 24px; margin: 0;'>🎉 Welcome Aboard!</h2>
                </div>
                
                <p>Hello, <strong>{fullName}</strong>!</p>
                <p>Congratulations! Your email has been verified successfully and your LearnQuest account is now active.</p>
                
                <div style='margin: 30px 0; padding: 25px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); border-radius: 10px; color: white; text-align: center;'>
                    <h3 style='margin: 0 0 15px 0; font-size: 20px;'>🚀 Ready to Start Learning?</h3>
                    <p style='margin: 0 0 20px 0; opacity: 0.9;'>Explore thousands of courses and unlock your potential!</p>
                    <a href='{_emailSettings.WebsiteUrl}' class='btn' style='background-color: white; color: #667eea; padding: 15px 30px; font-size: 16px; text-decoration: none; border-radius: 6px; font-weight: 600;'>
                        🎓 Start Learning Now
                    </a>
                </div>

                <div style='margin: 30px 0;'>
                    <h3 style='color: #374151; font-size: 18px; margin: 0 0 15px 0;'>What You Can Do Now:</h3>
                    <ul style='text-align: left; margin: 0; padding-left: 20px; color: #4b5563;'>
                        <li style='margin: 10px 0;'>📚 Browse and enroll in courses</li>
                        <li style='margin: 10px 0;'>📊 Track your learning progress</li>
                        <li style='margin: 10px 0;'>📝 Take quizzes and assessments</li>
                        <li style='margin: 10px 0;'>🏆 Earn certificates and badges</li>
                        <li style='margin: 10px 0;'>💬 Connect with other learners</li>
                    </ul>
                </div>

                <div style='margin: 25px 0; padding: 20px; background-color: #f0f9ff; border-radius: 8px; border-left: 4px solid #3b82f6;'>
                    <p style='margin: 0; font-size: 14px; color: #1e40af;'>
                        <strong>💡 Pro Tip:</strong> Complete your profile to get personalized course recommendations!
                    </p>
                </div>

                <p style='color: #4b5563;'>Happy learning!</p>
                <p style='color: #667eea; font-weight: 600;'>The LearnQuest Team</p>
            ");
        }

        public string BuildPasswordChangedEmail(string fullName)
        {
            return BuildEmailTemplate("Password Changed Successfully", $@"
                <div style='text-align: center; margin: 20px 0;'>
                    <div style='width: 80px; height: 80px; background-color: #10b981; border-radius: 50%; margin: 0 auto 20px; display: flex; align-items: center; justify-content: center; font-size: 40px;'>
                        ✅
                    </div>
                </div>

                <p>Hello, <strong>{fullName}</strong>!</p>
                <p>Your password has been changed successfully.</p>
                
                <div style='margin: 25px 0; padding: 20px; background-color: #f0fdf4; border-radius: 8px; border: 1px solid #bbf7d0;'>
                    <h3 style='margin: 0 0 10px 0; color: #166534; font-size: 16px;'>✅ Password Update Confirmed</h3>
                    <p style='margin: 0; color: #166534;'><strong>When:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>
                </div>

                <div style='margin: 25px 0; padding: 20px; background-color: #fef2f2; border-radius: 8px; border-left: 4px solid #ef4444;'>
                    <h3 style='margin: 0 0 10px 0; color: #dc2626; font-size: 16px;'>🚨 Didn't Make This Change?</h3>
                    <p style='margin: 0 0 15px 0; color: #dc2626;'>If you did not change your password, your account may have been compromised.</p>
                    <p style='text-align: center; margin: 0;'>
                        <a href='mailto:{_emailSettings.SupportEmail}' class='btn' style='background-color: #dc3545; padding: 12px 25px;'>
                            🆘 Contact Support Immediately
                        </a>
                    </p>
                </div>

                <div style='margin: 25px 0; padding: 15px; background-color: #f8fafc; border-radius: 6px; border: 1px solid #e2e8f0;'>
                    <h4 style='margin: 0 0 10px 0; color: #475569; font-size: 14px;'>🔐 Security Tips:</h4>
                    <ul style='margin: 0; padding-left: 20px; font-size: 14px; color: #64748b;'>
                        <li>Use a unique password for your LearnQuest account</li>
                        <li>Enable two-factor authentication if available</li>
                        <li>Never share your password with anyone</li>
                        <li>Update your password regularly</li>
                    </ul>
                </div>
            ");
        }

        public string BuildAccountLockedEmail(string fullName, DateTime unlockTime)
        {
            var timeZoneNote = unlockTime.ToString("yyyy-MM-dd HH:mm") + " UTC";
            var remainingTime = unlockTime - DateTime.UtcNow;
            var waitTime = remainingTime.TotalMinutes > 60
                ? $"{Math.Ceiling(remainingTime.TotalHours)} hour(s)"
                : $"{Math.Ceiling(remainingTime.TotalMinutes)} minute(s)";

            return BuildEmailTemplate("Account Temporarily Locked", $@"
                <div style='text-align: center; margin: 20px 0;'>
                    <div style='width: 80px; height: 80px; background-color: #f59e0b; border-radius: 50%; margin: 0 auto 20px; display: flex; align-items: center; justify-content: center; font-size: 40px;'>
                        🔒
                    </div>
                </div>

                <p>Hello, <strong>{fullName}</strong>!</p>
                <p>Your account has been temporarily locked due to multiple failed login attempts.</p>
                
                <div style='margin: 25px 0; padding: 20px; background-color: #fef3c7; border-radius: 8px; border: 1px solid #fcd34d;'>
                    <h3 style='margin: 0 0 15px 0; color: #92400e; font-size: 16px;'>⏰ Lockout Details</h3>
                    <p style='margin: 0 0 10px 0; color: #92400e;'><strong>Unlock Time:</strong> {timeZoneNote}</p>
                    <p style='margin: 0; color: #92400e;'><strong>Time Remaining:</strong> Approximately {waitTime}</p>
                </div>

                <div style='margin: 25px 0; padding: 20px; background-color: #f0f9ff; border-radius: 8px; border-left: 4px solid #3b82f6;'>
                    <h3 style='margin: 0 0 10px 0; color: #1e40af; font-size: 16px;'>🛡️ Security Information</h3>
                    <p style='margin: 0; color: #1e40af;'>This temporary lockout is a security measure to protect your account from unauthorized access attempts.</p>
                </div>

                <div style='margin: 25px 0; padding: 20px; background-color: #fef2f2; border-radius: 8px; border-left: 4px solid #ef4444;'>
                    <h3 style='margin: 0 0 10px 0; color: #dc2626; font-size: 16px;'>🚨 Suspicious Activity?</h3>
                    <p style='margin: 0 0 15px 0; color: #dc2626;'>If you believe these login attempts were not made by you, please contact our support team immediately.</p>
                    <p style='text-align: center; margin: 0;'>
                        <a href='mailto:{_emailSettings.SupportEmail}' class='btn' style='background-color: #dc3545; padding: 12px 25px;'>
                            📞 Contact Support
                        </a>
                    </p>
                </div>

                <div style='margin: 25px 0; padding: 15px; background-color: #f8fafc; border-radius: 6px; border: 1px solid #e2e8f0;'>
                    <h4 style='margin: 0 0 10px 0; color: #475569; font-size: 14px;'>💡 Next Steps:</h4>
                    <ol style='margin: 0; padding-left: 20px; font-size: 14px; color: #64748b;'>
                        <li>Wait for the lockout period to expire</li>
                        <li>Try logging in again with the correct credentials</li>
                        <li>Consider resetting your password if you've forgotten it</li>
                        <li>Contact support if you continue experiencing issues</li>
                    </ol>
                </div>
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