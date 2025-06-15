using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LearnQuestV1.Api.Services.Interfaces
{
    /// <summary>
    /// Enhanced interface for comprehensive security audit logging
    /// </summary>
    public interface ISecurityAuditLogger
    {
        /// <summary>
        /// Logs authentication attempts (both successful and failed)
        /// </summary>
        /// <param name="emailAttempted">Email address used in the attempt</param>
        /// <param name="ipAddress">IP address of the attempt</param>
        /// <param name="success">Whether the authentication was successful</param>
        /// <param name="failureReason">Reason for failure (if applicable)</param>
        /// <param name="userId">User ID (for successful logins)</param>
        Task LogAuthenticationAttemptAsync(string emailAttempted, string? ipAddress, bool success, string? failureReason = null, int? userId = null);

        /// <summary>
        /// Logs password change events
        /// </summary>
        /// <param name="userId">User who changed password</param>
        /// <param name="ipAddress">IP address of the request</param>
        Task LogPasswordChangeAsync(int userId, string ipAddress);

        /// <summary>
        /// Logs account lockout events
        /// </summary>
        /// <param name="email">Email of the locked account</param>
        /// <param name="ipAddress">IP address that triggered lockout</param>
        Task LogAccountLockoutAsync(string email, string ipAddress);

        /// <summary>
        /// Logs suspicious activity detection
        /// </summary>
        /// <param name="email">Email associated with suspicious activity</param>
        /// <param name="activity">Description of suspicious activity</param>
        /// <param name="ipAddress">IP address of the activity</param>
        Task LogSuspiciousActivityAsync(string email, string activity, string ipAddress);

        /// <summary>
        /// Logs token refresh attempts
        /// </summary>
        /// <param name="userId">User attempting token refresh</param>
        /// <param name="ipAddress">IP address of the request</param>
        /// <param name="success">Whether refresh was successful</param>
        Task LogTokenRefreshAsync(int userId, string ipAddress, bool success);

        /// <summary>
        /// Logs automatic login attempts (remember me functionality)
        /// </summary>
        /// <param name="ipAddress">IP address of the attempt</param>
        /// <param name="success">Whether auto-login was successful</param>
        /// <param name="failureReason">Reason for failure (if applicable)</param>
        Task LogAutoLoginAttemptAsync(string ipAddress, bool success, string? failureReason = null);

        /// <summary>
        /// Logs password reset requests
        /// </summary>
        /// <param name="email">Email requesting password reset</param>
        /// <param name="ipAddress">IP address of the request</param>
        Task LogPasswordResetRequestAsync(string email, string ipAddress);

        /// <summary>
        /// Logs email verification attempts
        /// </summary>
        /// <param name="email">Email being verified</param>
        /// <param name="ipAddress">IP address of the verification attempt</param>
        /// <param name="success">Whether verification was successful</param>
        Task LogEmailVerificationAsync(string email, string ipAddress, bool success);

        /// <summary>
        /// Logs general user actions for audit purposes
        /// </summary>
        /// <param name="userId">User performing the action</param>
        /// <param name="action">Type of action</param>
        /// <param name="description">Detailed description of the action</param>
        /// <param name="httpContext">HTTP context for additional information</param>
        Task LogUserActionAsync(int userId, string action, string description, HttpContext? httpContext = null);

        // Additional enhanced methods for better security monitoring

        /// <summary>
        /// Logs general security events
        /// </summary>
        /// <param name="eventType">Type of security event</param>
        /// <param name="eventDetails">Detailed description of the event</param>
        /// <param name="httpContext">Current HTTP context</param>
        /// <param name="success">Whether the security event was successful</param>
        /// <param name="userId">Optional user ID if known</param>
        /// <param name="emailAttempted">Optional email if this was a login attempt</param>
        Task LogSecurityEventAsync(string eventType, string eventDetails, HttpContext httpContext,
            bool success, int? userId = null, string? emailAttempted = null);

        /// <summary>
        /// Logs data access events for sensitive operations
        /// </summary>
        /// <param name="userId">User accessing data</param>
        /// <param name="dataType">Type of data accessed</param>
        /// <param name="operation">Operation performed (read, write, delete)</param>
        /// <param name="recordIds">IDs of records accessed</param>
        /// <param name="httpContext">HTTP context</param>
        Task LogDataAccessAsync(int userId, string dataType, string operation, string? recordIds, HttpContext? httpContext = null);

        /// <summary>
        /// Logs API rate limit violations
        /// </summary>
        /// <param name="ipAddress">IP address that exceeded limits</param>
        /// <param name="endpoint">API endpoint accessed</param>
        /// <param name="requestCount">Number of requests made</param>
        /// <param name="timeWindow">Time window for the requests</param>
        Task LogRateLimitViolationAsync(string ipAddress, string endpoint, int requestCount, TimeSpan timeWindow);
    }
}