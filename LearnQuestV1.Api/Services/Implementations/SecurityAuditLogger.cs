using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models.Administration;
using Microsoft.Extensions.Logging;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class SecurityAuditLogger : ISecurityAuditLogger
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<SecurityAuditLogger> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SecurityAuditLogger(
            IUnitOfWork uow,
            ILogger<SecurityAuditLogger> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _uow = uow;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAuthenticationAttemptAsync(string emailAttempted, string? ipAddress, bool success, string? failureReason = null, int? userId = null)
        {
            var logEntry = new SecurityAuditLog
            {
                EmailAttempted = MaskEmail(emailAttempted),
                UserId = success ? userId : null, // Only store userId on successful login
                IpAddress = ipAddress,
                Success = success,
                FailureReason = failureReason,
                EventType = "AUTHENTICATION_ATTEMPT",
                EventDetails = success
                    ? "Successful authentication attempt"
                    : $"Failed authentication attempt. Reason: {failureReason}",
                Timestamp = DateTime.UtcNow,
                UserAgent = GetUserAgent(),
                SessionId = GetSessionId(),
                RiskScore = CalculateRiskScore(ipAddress, success, failureReason)
            };

            await SaveAuditLogAsync(logEntry);

            if (success)
                _logger.LogInformation("Successful login for {Email} from {IpAddress}", MaskEmail(emailAttempted), ipAddress);
            else
                _logger.LogWarning("Failed login for {Email} from {IpAddress}. Reason: {Reason}", MaskEmail(emailAttempted), ipAddress, failureReason);
        }

        public async Task LogPasswordChangeAsync(int userId, string ipAddress)
        {
            var logEntry = new SecurityAuditLog
            {
                UserId = userId,
                IpAddress = ipAddress,
                Success = true,
                EventType = "PASSWORD_CHANGE",
                EventDetails = "Password changed successfully",
                Timestamp = DateTime.UtcNow,
                UserAgent = GetUserAgent(),
                SessionId = GetSessionId(),
                RiskScore = 10 // Low risk for authenticated password change
            };

            await SaveAuditLogAsync(logEntry);
            _logger.LogInformation("Password changed for user {UserId} from {IpAddress}", userId, ipAddress);
        }

        public async Task LogAccountLockoutAsync(string email, string ipAddress)
        {
            var logEntry = new SecurityAuditLog
            {
                EmailAttempted = MaskEmail(email),
                IpAddress = ipAddress,
                Success = false,
                EventType = "ACCOUNT_LOCKOUT",
                EventDetails = $"Account locked due to failed login attempts",
                Timestamp = DateTime.UtcNow,
                UserAgent = GetUserAgent(),
                RiskScore = 75 // High risk event
            };

            await SaveAuditLogAsync(logEntry);
            _logger.LogWarning("Account lockout triggered for {Email} from {IpAddress}", MaskEmail(email), ipAddress);
        }

        public async Task LogSuspiciousActivityAsync(string email, string activity, string ipAddress)
        {
            var logEntry = new SecurityAuditLog
            {
                EmailAttempted = MaskEmail(email),
                IpAddress = ipAddress,
                Success = false,
                EventType = "SUSPICIOUS_ACTIVITY",
                EventDetails = $"Suspicious activity detected: {activity}",
                Timestamp = DateTime.UtcNow,
                UserAgent = GetUserAgent(),
                RiskScore = 85 // Very high risk
            };

            await SaveAuditLogAsync(logEntry);
            _logger.LogWarning("Suspicious activity: {Activity} for {Email} from {IpAddress}", activity, MaskEmail(email), ipAddress);
        }

        public async Task LogTokenRefreshAsync(int userId, string ipAddress, bool success)
        {
            var logEntry = new SecurityAuditLog
            {
                UserId = userId,
                IpAddress = ipAddress,
                Success = success,
                EventType = "TOKEN_REFRESH",
                EventDetails = success ? "Token refreshed successfully" : "Failed token refresh attempt",
                Timestamp = DateTime.UtcNow,
                UserAgent = GetUserAgent(),
                SessionId = GetSessionId(),
                RiskScore = success ? 5 : 40
            };

            await SaveAuditLogAsync(logEntry);

            if (success)
                _logger.LogInformation("Token refreshed for user {UserId} from {IpAddress}", userId, ipAddress);
            else
                _logger.LogWarning("Failed token refresh for user {UserId} from {IpAddress}", userId, ipAddress);
        }

        public async Task LogAutoLoginAttemptAsync(string ipAddress, bool success, string? failureReason = null)
        {
            var logEntry = new SecurityAuditLog
            {
                IpAddress = ipAddress,
                Success = success,
                FailureReason = failureReason,
                EventType = "AUTO_LOGIN_ATTEMPT",
                EventDetails = success ? "Successful auto-login" : $"Failed auto-login: {failureReason}",
                Timestamp = DateTime.UtcNow,
                UserAgent = GetUserAgent(),
                RiskScore = success ? 15 : 50
            };

            await SaveAuditLogAsync(logEntry);

            if (success)
                _logger.LogInformation("Successful auto-login from {IpAddress}", ipAddress);
            else
                _logger.LogWarning("Failed auto-login from {IpAddress}. Reason: {Reason}", ipAddress, failureReason);
        }

        public async Task LogPasswordResetRequestAsync(string email, string ipAddress)
        {
            var logEntry = new SecurityAuditLog
            {
                EmailAttempted = MaskEmail(email),
                IpAddress = ipAddress,
                Success = true, // Request was processed (doesn't mean email exists)
                EventType = "PASSWORD_RESET_REQUEST",
                EventDetails = "Password reset request submitted",
                Timestamp = DateTime.UtcNow,
                UserAgent = GetUserAgent(),
                RiskScore = 25
            };

            await SaveAuditLogAsync(logEntry);
            _logger.LogInformation("Password reset requested for {Email} from {IpAddress}", MaskEmail(email), ipAddress);
        }

        public async Task LogEmailVerificationAsync(string email, string ipAddress, bool success)
        {
            var logEntry = new SecurityAuditLog
            {
                EmailAttempted = MaskEmail(email),
                IpAddress = ipAddress,
                Success = success,
                EventType = "EMAIL_VERIFICATION",
                EventDetails = success ? "Email verified successfully" : "Failed email verification attempt",
                Timestamp = DateTime.UtcNow,
                UserAgent = GetUserAgent(),
                RiskScore = success ? 5 : 30
            };

            await SaveAuditLogAsync(logEntry);

            if (success)
                _logger.LogInformation("Email verified for {Email} from {IpAddress}", MaskEmail(email), ipAddress);
            else
                _logger.LogWarning("Failed email verification for {Email} from {IpAddress}", MaskEmail(email), ipAddress);
        }

        private async Task SaveAuditLogAsync(SecurityAuditLog logEntry)
        {
            try
            {
                await _uow.SecurityAuditLogs.AddAsync(logEntry);
                await _uow.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log to application logger if database logging fails
                _logger.LogError(ex, "Failed to save security audit log: {EventType}", logEntry.EventType);
            }
        }

        private string? GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
        }

        private string? GetSessionId()
        {
            return _httpContextAccessor.HttpContext?.Session?.Id;
        }

        private static int CalculateRiskScore(string? ipAddress, bool success, string? failureReason)
        {
            int score = 0;

            // Base score for failed attempts
            if (!success)
                score += 30;

            // Higher score for specific failure reasons
            if (!string.IsNullOrEmpty(failureReason))
            {
                if (failureReason.Contains("locked", StringComparison.OrdinalIgnoreCase))
                    score += 40;
                else if (failureReason.Contains("invalid", StringComparison.OrdinalIgnoreCase))
                    score += 20;
                else if (failureReason.Contains("expired", StringComparison.OrdinalIgnoreCase))
                    score += 15;
            }

            // Check for suspicious IP patterns (basic implementation)
            if (!string.IsNullOrEmpty(ipAddress) && IsSuspiciousIp(ipAddress))
                score += 25;

            return Math.Min(100, score);
        }

        private static bool IsSuspiciousIp(string ipAddress)
        {
            // Basic suspicious IP detection
            // In production, integrate with threat intelligence services
            var suspiciousPatterns = new[]
            {
                "127.0.0.1", // Localhost (might be suspicious in production)
                "0.0.0.0"    // Invalid IP
            };

            return suspiciousPatterns.Any(pattern => ipAddress.Contains(pattern));
        }

        private static string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains('@'))
                return "***";

            var parts = email.Split('@');
            var localPart = parts[0];
            var domain = parts[1];

            if (localPart.Length <= 2)
                return $"***@{domain}";

            return $"{localPart[0]}***{localPart[^1]}@{domain}";
        }
    }
}