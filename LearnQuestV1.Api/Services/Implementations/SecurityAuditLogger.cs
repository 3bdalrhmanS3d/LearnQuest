using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models.Administration;
using LearnQuestV1.Core.Models.UserManagement;

public class SecurityAuditLogger : ISecurityAuditLogger
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<SecurityAuditLogger> _logger;

    public SecurityAuditLogger(IUnitOfWork uow, ILogger<SecurityAuditLogger> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task LogAuthenticationAttemptAsync(string emailAttempted, string? ipAddress, bool success, string? failureReason = null, int? userId = null)
    {
        var logEntry = new SecurityAuditLog
        {
            EmailAttempted = emailAttempted,
            UserId = userId,
            IpAddress = ipAddress,
            Success = success,
            FailureReason = failureReason,
            EventType = "AUTHENTICATION_ATTEMPT",
            EventDetails = success
                ? "Successful authentication attempt"
                : $"Failed authentication attempt. Reason: {failureReason}",
            Timestamp = DateTime.UtcNow
        };

        await _uow.SecurityAuditLogs.AddAsync(logEntry);
        await _uow.SaveChangesAsync();

        if (success)
            _logger.LogInformation("Successful login for {Email} from {IpAddress}", emailAttempted, ipAddress);
        else
            _logger.LogWarning("Failed login for {Email} from {IpAddress}. Reason: {Reason}", emailAttempted, ipAddress, failureReason);
    }


    public async Task LogPasswordChangeAsync(int userId, string ipAddress)
    {
        var logEntry = new SecurityAuditLog
        {
            UserId = userId,
            IpAddress = ipAddress,
            Success = true, // لأنه حدث إداري ناجح
            EventType = "PASSWORD_CHANGE",
            EventDetails = "Password changed successfully",
            Timestamp = DateTime.UtcNow
        };

        await _uow.SecurityAuditLogs.AddAsync(logEntry);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Password changed for user {UserId} from {IpAddress}", userId, ipAddress);
    }


    public async Task LogAccountLockoutAsync(string email, string ipAddress)
    {
        var logEntry = new SecurityAuditLog
        {
            EmailAttempted = email,
            IpAddress = ipAddress,
            Success = false, // لأنه فشل أدى للقفل
            EventType = "ACCOUNT_LOCKOUT",
            EventDetails = $"Account locked due to failed login attempts: {MaskEmail(email)}",
            Timestamp = DateTime.UtcNow
        };

        await _uow.SecurityAuditLogs.AddAsync(logEntry);
        await _uow.SaveChangesAsync();

        _logger.LogWarning("Account lockout triggered for {Email} from {IpAddress}",
            MaskEmail(email), ipAddress);
    }

    public async Task LogSuspiciousActivityAsync(string email, string activity, string ipAddress)
    {
        var logEntry = new SecurityAuditLog
        {
            EmailAttempted = email,
            IpAddress = ipAddress,
            Success = false,  // لأنه حدث مريب = مشكلة 
            EventType = "SUSPICIOUS_ACTIVITY",
            EventDetails = $"Suspicious activity detected: {activity} for {MaskEmail(email)}",
            Timestamp = DateTime.UtcNow
        };

        await _uow.SecurityAuditLogs.AddAsync(logEntry);
        await _uow.SaveChangesAsync();

        _logger.LogWarning("Suspicious activity: {Activity} for {Email} from {IpAddress}",
            activity, MaskEmail(email), ipAddress);
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
