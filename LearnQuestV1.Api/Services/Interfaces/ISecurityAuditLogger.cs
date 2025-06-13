// ISecurityAuditLogger.cs (Updated Interface)
namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface ISecurityAuditLogger
    {
        Task LogAuthenticationAttemptAsync(string emailAttempted, string? ipAddress, bool success, string? failureReason = null, int? userId = null);
        Task LogPasswordChangeAsync(int userId, string ipAddress);
        Task LogAccountLockoutAsync(string email, string ipAddress);
        Task LogSuspiciousActivityAsync(string email, string activity, string ipAddress);
        Task LogTokenRefreshAsync(int userId, string ipAddress, bool success);
        Task LogAutoLoginAttemptAsync(string ipAddress, bool success, string? failureReason = null);
        Task LogPasswordResetRequestAsync(string email, string ipAddress);
        Task LogEmailVerificationAsync(string email, string ipAddress, bool success);
    }
}