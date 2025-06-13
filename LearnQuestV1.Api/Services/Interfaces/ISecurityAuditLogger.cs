public interface ISecurityAuditLogger
{
    Task LogAuthenticationAttemptAsync(string emailAttempted, string? ipAddress, bool success, string? failureReason = null, int? userId = null);
    Task LogPasswordChangeAsync(int userId, string ipAddress);
    Task LogAccountLockoutAsync(string email, string ipAddress);
    Task LogSuspiciousActivityAsync(string email, string activity, string ipAddress);
}
