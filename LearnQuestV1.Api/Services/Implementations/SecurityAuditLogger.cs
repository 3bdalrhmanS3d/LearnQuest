using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Models.Administration;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Api.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace LearnQuestV1.Api.Services.Implementations
{
    /// <summary>
    /// Enhanced implementation of comprehensive security audit logging service
    /// </summary>
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

        // =====================================================
        // Main Security Audit Methods (Existing Interface)
        // =====================================================

        /// <summary>
        /// Logs authentication attempts (both successful and failed)
        /// </summary>
        public async Task LogAuthenticationAttemptAsync(string emailAttempted, string? ipAddress, bool success, string? failureReason = null, int? userId = null)
        {
            try
            {
                var logEntry = new SecurityAuditLog
                {
                    EmailAttempted = MaskEmail(emailAttempted),
                    UserId = success ? userId : null,
                    IpAddress = ipAddress ?? GetClientIpAddress(),
                    Success = success,
                    FailureReason = failureReason,
                    EventType = "AUTHENTICATION_ATTEMPT",
                    EventDetails = success
                        ? "Successful authentication attempt"
                        : $"Failed authentication attempt. Reason: {failureReason}",
                    Timestamp = DateTime.UtcNow,
                    UserAgent = GetUserAgent(),
                    SessionId = GetSessionId(),
                    RiskScore = CalculateAuthenticationRiskScore(ipAddress, success, failureReason),
                    GeoLocation = GetGeoLocation()
                };

                await SaveAuditLogAsync(logEntry);

                if (success)
                    _logger.LogInformation("Successful login for {Email} from {IpAddress}", MaskEmail(emailAttempted), logEntry.IpAddress);
                else
                    _logger.LogWarning("Failed login for {Email} from {IpAddress}. Reason: {Reason}", MaskEmail(emailAttempted), logEntry.IpAddress, failureReason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log authentication attempt for {Email}", MaskEmail(emailAttempted));
            }
        }

        /// <summary>
        /// Logs password change events
        /// </summary>
        public async Task LogPasswordChangeAsync(int userId, string ipAddress)
        {
            try
            {
                var logEntry = new SecurityAuditLog
                {
                    UserId = userId,
                    IpAddress = ipAddress ?? GetClientIpAddress(),
                    Success = true,
                    EventType = "PASSWORD_CHANGE",
                    EventDetails = "Password changed successfully",
                    Timestamp = DateTime.UtcNow,
                    UserAgent = GetUserAgent(),
                    SessionId = GetSessionId(),
                    RiskScore = 10,
                    GeoLocation = GetGeoLocation()
                };

                await SaveAuditLogAsync(logEntry);
                _logger.LogInformation("Password changed for user {UserId} from {IpAddress}", userId, logEntry.IpAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log password change for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Logs account lockout events
        /// </summary>
        public async Task LogAccountLockoutAsync(string email, string ipAddress)
        {
            try
            {
                var logEntry = new SecurityAuditLog
                {
                    EmailAttempted = MaskEmail(email),
                    IpAddress = ipAddress ?? GetClientIpAddress(),
                    Success = false,
                    EventType = "ACCOUNT_LOCKOUT",
                    EventDetails = "Account locked due to failed login attempts",
                    Timestamp = DateTime.UtcNow,
                    UserAgent = GetUserAgent(),
                    RiskScore = 75,
                    GeoLocation = GetGeoLocation()
                };

                await SaveAuditLogAsync(logEntry);
                _logger.LogWarning("Account lockout triggered for {Email} from {IpAddress}", MaskEmail(email), logEntry.IpAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log account lockout for {Email}", MaskEmail(email));
            }
        }

        /// <summary>
        /// Logs suspicious activity detection
        /// </summary>
        public async Task LogSuspiciousActivityAsync(string email, string activity, string ipAddress)
        {
            try
            {
                var logEntry = new SecurityAuditLog
                {
                    EmailAttempted = MaskEmail(email),
                    IpAddress = ipAddress ?? GetClientIpAddress(),
                    Success = false,
                    EventType = "SUSPICIOUS_ACTIVITY",
                    EventDetails = $"Suspicious activity detected: {activity}",
                    Timestamp = DateTime.UtcNow,
                    UserAgent = GetUserAgent(),
                    RiskScore = 85,
                    GeoLocation = GetGeoLocation()
                };

                await SaveAuditLogAsync(logEntry);
                _logger.LogWarning("Suspicious activity: {Activity} for {Email} from {IpAddress}", activity, MaskEmail(email), logEntry.IpAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log suspicious activity for {Email}", MaskEmail(email));
            }
        }

        /// <summary>
        /// Logs token refresh attempts
        /// </summary>
        public async Task LogTokenRefreshAsync(int userId, string ipAddress, bool success)
        {
            try
            {
                var logEntry = new SecurityAuditLog
                {
                    UserId = userId,
                    IpAddress = ipAddress ?? GetClientIpAddress(),
                    Success = success,
                    EventType = "TOKEN_REFRESH",
                    EventDetails = success ? "Token refreshed successfully" : "Failed token refresh attempt",
                    Timestamp = DateTime.UtcNow,
                    UserAgent = GetUserAgent(),
                    SessionId = GetSessionId(),
                    RiskScore = success ? 5 : 40,
                    GeoLocation = GetGeoLocation()
                };

                await SaveAuditLogAsync(logEntry);

                if (success)
                    _logger.LogInformation("Token refreshed for user {UserId} from {IpAddress}", userId, logEntry.IpAddress);
                else
                    _logger.LogWarning("Failed token refresh for user {UserId} from {IpAddress}", userId, logEntry.IpAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log token refresh for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Logs automatic login attempts (remember me functionality)
        /// </summary>
        public async Task LogAutoLoginAttemptAsync(string ipAddress, bool success, string? failureReason = null)
        {
            try
            {
                var logEntry = new SecurityAuditLog
                {
                    IpAddress = ipAddress ?? GetClientIpAddress(),
                    Success = success,
                    FailureReason = failureReason,
                    EventType = "AUTO_LOGIN_ATTEMPT",
                    EventDetails = success ? "Successful auto-login" : $"Failed auto-login: {failureReason}",
                    Timestamp = DateTime.UtcNow,
                    UserAgent = GetUserAgent(),
                    RiskScore = success ? 15 : 50,
                    GeoLocation = GetGeoLocation()
                };

                await SaveAuditLogAsync(logEntry);

                if (success)
                    _logger.LogInformation("Successful auto-login from {IpAddress}", logEntry.IpAddress);
                else
                    _logger.LogWarning("Failed auto-login from {IpAddress}. Reason: {Reason}", logEntry.IpAddress, failureReason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log auto-login attempt from {IpAddress}", ipAddress);
            }
        }

        /// <summary>
        /// Logs password reset requests
        /// </summary>
        public async Task LogPasswordResetRequestAsync(string email, string ipAddress)
        {
            try
            {
                var logEntry = new SecurityAuditLog
                {
                    EmailAttempted = MaskEmail(email),
                    IpAddress = ipAddress ?? GetClientIpAddress(),
                    Success = true,
                    EventType = "PASSWORD_RESET_REQUEST",
                    EventDetails = "Password reset request submitted",
                    Timestamp = DateTime.UtcNow,
                    UserAgent = GetUserAgent(),
                    RiskScore = 25,
                    GeoLocation = GetGeoLocation()
                };

                await SaveAuditLogAsync(logEntry);
                _logger.LogInformation("Password reset requested for {Email} from {IpAddress}", MaskEmail(email), logEntry.IpAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log password reset request for {Email}", MaskEmail(email));
            }
        }

        /// <summary>
        /// Logs email verification attempts
        /// </summary>
        public async Task LogEmailVerificationAsync(string email, string ipAddress, bool success)
        {
            try
            {
                var logEntry = new SecurityAuditLog
                {
                    EmailAttempted = MaskEmail(email),
                    IpAddress = ipAddress ?? GetClientIpAddress(),
                    Success = success,
                    EventType = "EMAIL_VERIFICATION",
                    EventDetails = success ? "Email verified successfully" : "Failed email verification attempt",
                    Timestamp = DateTime.UtcNow,
                    UserAgent = GetUserAgent(),
                    RiskScore = success ? 5 : 30,
                    GeoLocation = GetGeoLocation()
                };

                await SaveAuditLogAsync(logEntry);

                if (success)
                    _logger.LogInformation("Email verified for {Email} from {IpAddress}", MaskEmail(email), logEntry.IpAddress);
                else
                    _logger.LogWarning("Failed email verification for {Email} from {IpAddress}", MaskEmail(email), logEntry.IpAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log email verification for {Email}", MaskEmail(email));
            }
        }

        /// <summary>
        /// Logs general user actions for audit purposes
        /// </summary>
        public async Task LogUserActionAsync(int userId, string action, string description, HttpContext? httpContext = null)
        {
            try
            {
                var context = httpContext ?? _httpContextAccessor.HttpContext;
                var logEntry = new SecurityAuditLog
                {
                    UserId = userId,
                    IpAddress = GetClientIpAddress(context),
                    Success = true,
                    EventType = "USER_ACTION",
                    EventDetails = $"{action}: {description}",
                    Timestamp = DateTime.UtcNow,
                    UserAgent = GetUserAgent(context),
                    SessionId = GetSessionId(context),
                    RiskScore = 10,
                    GeoLocation = GetGeoLocation(context)
                };

                await SaveAuditLogAsync(logEntry);
                _logger.LogInformation("User action logged: {Action} for user {UserId}", action, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log user action {Action} for user {UserId}", action, userId);
            }
        }

        // =====================================================
        // Enhanced Security Audit Methods (Additional Features)
        // =====================================================

        /// <summary>
        /// Logs user permission and authorization events
        /// </summary>
        public async Task LogAuthorizationEventAsync(int userId, string action, string resource, bool success, string? denialReason = null)
        {
            try
            {
                var auditLog = CreateBaseAuditLog(_httpContextAccessor.HttpContext, "AUTHORIZATION_EVENT", $"{action} on {resource}", success);
                auditLog.UserId = userId;
                if (!success)
                {
                    auditLog.FailureReason = denialReason;
                    auditLog.RiskScore = CalculateRiskScore("UnauthorizedAccess", _httpContextAccessor.HttpContext);
                }

                await SaveAuditLogAsync(auditLog);
                _logger.LogInformation("Authorization event: {Action} on {Resource} by user {UserId} - Success: {Success}", action, resource, userId, success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log authorization event for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Logs resource access events (CRUD operations)
        /// </summary>
        public async Task LogResourceAccessAsync(int userId, string resourceType, int resourceId, string action, string? details = null, bool success = true)
        {
            try
            {
                var auditLog = CreateBaseAuditLog(_httpContextAccessor.HttpContext, "RESOURCE_ACCESS", $"{action} {resourceType}#{resourceId}" + (details != null ? $": {details}" : ""), success);
                auditLog.UserId = userId;
                auditLog.RiskScore = CalculateDataAccessRiskScore(action, resourceType);

                await SaveAuditLogAsync(auditLog);
                _logger.LogInformation("Resource access logged: {Action} on {ResourceType}#{ResourceId} by user {UserId}", action, resourceType, resourceId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log resource access for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Logs suspicious activities that may indicate security threats
        /// </summary>
        public async Task LogSuspiciousActivityAsync(int? userId, string activityType, string description, SecurityRiskLevel riskLevel, string? ipAddress = null, Dictionary<string, object>? additionalData = null)
        {
            try
            {
                var auditLog = CreateBaseAuditLog(_httpContextAccessor.HttpContext, "SUSPICIOUS_ACTIVITY", $"{activityType}: {description}", false);
                auditLog.UserId = userId;
                auditLog.RiskScore = (int)riskLevel;
                auditLog.IpAddress = ipAddress ?? auditLog.IpAddress;
                if (additionalData != null)
                {
                    // assume Metadata is a JSON column
                    auditLog.EventDetails += $" | Metadata: {System.Text.Json.JsonSerializer.Serialize(additionalData)}";
                }

                await SaveAuditLogAsync(auditLog);
                _logger.LogWarning("Suspicious activity logged: {ActivityType} for user {UserId}", activityType, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log suspicious activity pattern {ActivityType}", activityType);
            }
        }

        /// <summary>
        /// Logs data export/download events for compliance
        /// </summary>
        public async Task LogDataExportAsync(int userId, string dataType, int recordCount, string exportFormat, string? purpose = null)
        {
            try
            {
                var details = $"Exported {recordCount} records of {dataType} as {exportFormat}" + (purpose != null ? $" for {purpose}" : "");
                var auditLog = CreateBaseAuditLog(_httpContextAccessor.HttpContext, "DATA_EXPORT", details, true);
                auditLog.UserId = userId;
                auditLog.RiskScore = 30;

                await SaveAuditLogAsync(auditLog);
                _logger.LogInformation("Data export logged for user {UserId}: {Details}", userId, details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log data export for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Logs content creation events with detailed metadata
        /// </summary>
        public async Task LogContentCreationAsync(int userId, int contentId, string contentType, int sectionId, string title, Dictionary<string, object>? metadata = null)
        {
            try
            {
                var details = $"{contentType}#{contentId} '{title}' in Section#{sectionId}";
                var auditLog = CreateBaseAuditLog(_httpContextAccessor.HttpContext, "CONTENT_CREATION", details, true);
                auditLog.UserId = userId;
                if (metadata != null)
                    auditLog.EventDetails += $" | Meta: {System.Text.Json.JsonSerializer.Serialize(metadata)}";

                await SaveAuditLogAsync(auditLog);
                _logger.LogInformation("Content creation logged: {Details} by user {UserId}", details, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log content creation for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Logs content modification events
        /// </summary>
        public async Task LogContentModificationAsync(int userId, int contentId, string changes, Dictionary<string, object>? previousValues = null, Dictionary<string, object>? newValues = null)
        {
            try
            {
                var details = $"Modified Content#{contentId}: {changes}";
                if (previousValues != null)
                    details += $" | Prev: {System.Text.Json.JsonSerializer.Serialize(previousValues)}";
                if (newValues != null)
                    details += $" | New: {System.Text.Json.JsonSerializer.Serialize(newValues)}";

                var auditLog = CreateBaseAuditLog(_httpContextAccessor.HttpContext, "CONTENT_MODIFICATION", details, true);
                auditLog.UserId = userId;
                auditLog.RiskScore = 20;

                await SaveAuditLogAsync(auditLog);
                _logger.LogInformation("Content modification logged: {Details} by user {UserId}", details, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log content modification for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Logs content deletion/archival events
        /// </summary>
        public async Task LogContentDeletionAsync(int userId, int contentId, string title, string deletionType, string? reason = null)
        {
            try
            {
                var details = $"{deletionType} Content#{contentId} '{title}'" + (reason != null ? $" because {reason}" : "");
                var auditLog = CreateBaseAuditLog(_httpContextAccessor.HttpContext, "CONTENT_DELETION", details, true);
                auditLog.UserId = userId;
                auditLog.RiskScore = 40;

                await SaveAuditLogAsync(auditLog);
                _logger.LogWarning("Content deletion logged: {Details} by user {UserId}", details, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log content deletion for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Logs content visibility changes
        /// </summary>
        public async Task LogContentVisibilityChangeAsync(int userId, int contentId, bool previousVisibility, bool newVisibility, string? reason = null)
        {
            try
            {
                var details = $"Content#{contentId} visibility: {previousVisibility} → {newVisibility}" + (reason != null ? $" because {reason}" : "");
                var auditLog = CreateBaseAuditLog(_httpContextAccessor.HttpContext, "CONTENT_VISIBILITY_CHANGE", details, true);
                auditLog.UserId = userId;
                auditLog.RiskScore = 15;

                await SaveAuditLogAsync(auditLog);
                _logger.LogInformation("Visibility change logged: {Details} by user {UserId}", details, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log content visibility change for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Logs bulk content operations
        /// </summary>
        public async Task LogBulkContentOperationAsync(int userId, string operation, IEnumerable<int> contentIds, int successCount, int failureCount, string? details = null)
        {
            try
            {
                var ids = string.Join(",", contentIds);
                var msg = $"{operation} on [{ids}]: {successCount} succeeded, {failureCount} failed" + (details != null ? $" ({details})" : "");
                var auditLog = CreateBaseAuditLog(_httpContextAccessor.HttpContext, "BULK_CONTENT_OPERATION", msg, failureCount == 0);
                auditLog.UserId = userId;
                auditLog.RiskScore = failureCount > 0 ? 50 : 10;

                await SaveAuditLogAsync(auditLog);
                _logger.LogInformation("Bulk content operation logged: {Msg} by user {UserId}", msg, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log bulk content operation for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Logs file upload events
        /// </summary>
        public async Task LogFileUploadAsync(int userId, string fileName, long fileSize, string fileType, string uploadPath, bool success = true, string? errorMessage = null)
        {
            try
            {
                var details = $"Uploaded '{fileName}' ({fileSize} bytes, {fileType}) to {uploadPath}" + (errorMessage != null ? $" Error: {errorMessage}" : "");
                var auditLog = CreateBaseAuditLog(_httpContextAccessor.HttpContext, "FILE_UPLOAD", details, success);
                auditLog.UserId = userId;
                auditLog.RiskScore = success ? 20 : 60;

                await SaveAuditLogAsync(auditLog);
                if (success)
                    _logger.LogInformation("File upload logged: {Details} by user {UserId}", details, userId);
                else
                    _logger.LogWarning("File upload failure logged: {Details} by user {UserId}", details, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log file upload for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Logs file deletion events
        /// </summary>
        public async Task LogFileDeletionAsync(int userId, string filePath, string fileName, string? reason = null, bool success = true)
        {
            try
            {
                var details = $"Deleted '{fileName}' at {filePath}" + (reason != null ? $" because {reason}" : "");
                var auditLog = CreateBaseAuditLog(_httpContextAccessor.HttpContext, "FILE_DELETION", details, success);
                auditLog.UserId = userId;
                auditLog.RiskScore = success ? 25 : 55;

                await SaveAuditLogAsync(auditLog);
                _logger.LogInformation("File deletion logged: {Details} by user {UserId}", details, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log file deletion for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Logs file access/download events
        /// </summary>
        public async Task LogFileAccessAsync(int userId, string filePath, string fileName, string accessType, int? contentId = null)
        {
            try
            {
                var details = $"{accessType} '{fileName}' at {filePath}" + (contentId.HasValue ? $" (Content#{contentId})" : "");
                var auditLog = CreateBaseAuditLog(_httpContextAccessor.HttpContext, "FILE_ACCESS", details, true);
                auditLog.UserId = userId;
                auditLog.RiskScore = 10;

                await SaveAuditLogAsync(auditLog);
                _logger.LogInformation("File access logged: {Details} by user {UserId}", details, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log file access for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Logs administrative actions performed by admins
        /// </summary>
        public async Task LogAdministrativeActionAsync(int adminId, string action, string targetType, int? targetId, string description, AdminActionImpactLevel impactLevel)
        {
            try
            {
                var details = $"{action} on {targetType}" + (targetId.HasValue ? $"#{targetId}" : "") + $": {description}";
                var auditLog = CreateBaseAuditLog(_httpContextAccessor.HttpContext, "ADMIN_ACTION", details, true);
                auditLog.UserId = adminId;
                auditLog.RiskScore = (int)impactLevel * 10;

                await SaveAuditLogAsync(auditLog);
                _logger.LogInformation("Admin action logged: {Details} by admin {AdminId}", details, adminId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log administrative action for admin {AdminId}", adminId);
            }
        }

        /// <summary>
        /// Logs system configuration changes
        /// </summary>
        public async Task LogSystemConfigurationChangeAsync(int adminId, string configurationKey, string? previousValue, string newValue, string? changeReason = null)
        {
            try
            {
                var details = $"Changed '{configurationKey}': '{previousValue}' → '{newValue}'" + (changeReason != null ? $" because {changeReason}" : "");
                var auditLog = CreateBaseAuditLog(_httpContextAccessor.HttpContext, "CONFIGURATION_CHANGE", details, true);
                auditLog.UserId = adminId;
                auditLog.RiskScore = 30;

                await SaveAuditLogAsync(auditLog);
                _logger.LogInformation("Configuration change logged: {Details} by admin {AdminId}", details, adminId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log system configuration change for admin {AdminId}", adminId);
            }
        }

        /// <summary>
        /// Logs user management actions (create, modify, disable, etc.)
        /// </summary>
        public async Task LogUserManagementActionAsync(int adminId, string action, int targetUserId, string targetUserEmail, string changes, string? reason = null)
        {
            try
            {
                var details = $"{action} User#{targetUserId} ({MaskEmail(targetUserEmail)})" + $": {changes}" + (reason != null ? $" because {reason}" : "");
                var auditLog = CreateBaseAuditLog(_httpContextAccessor.HttpContext, "USER_MANAGEMENT", details, true);
                auditLog.UserId = adminId;
                auditLog.RiskScore = 35;

                await SaveAuditLogAsync(auditLog);
                _logger.LogInformation("User management action logged: {Details} by admin {AdminId}", details, adminId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log user management action for admin {AdminId}", adminId);
            }
        }

        /// <summary>
        /// Logs data access for compliance (GDPR, CCPA, etc.)
        /// </summary>
        public async Task LogPersonalDataAccessAsync(int userId, int accessedBy, string dataType, string purpose, string? legalBasis = null)
        {
            try
            {
                var details = $"Data '{dataType}' for User#{userId} accessed by User#{accessedBy} for {purpose}" + (legalBasis != null ? $" under {legalBasis}" : "");
                var auditLog = CreateBaseAuditLog(_httpContextAccessor.HttpContext, "PERSONAL_DATA_ACCESS", details, true);
                auditLog.UserId = accessedBy;
                auditLog.RiskScore = 50;

                await SaveAuditLogAsync(auditLog);
                _logger.LogInformation("Personal data access logged: {Details}", details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log personal data access for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Logs privacy-related events (consent, data portability, etc.)
        /// </summary>
        public async Task LogPrivacyEventAsync(int userId, string eventType, string description, bool? consentGiven = null, Dictionary<string, object>? metadata = null)
        {
            try
            {
                var details = $"{eventType}: {description}" + (consentGiven.HasValue ? $", consent: {consentGiven}" : "");
                if (metadata != null)
                    details += $" | Meta: {System.Text.Json.JsonSerializer.Serialize(metadata)}";

                var auditLog = CreateBaseAuditLog(_httpContextAccessor.HttpContext, "PRIVACY_EVENT", details, true);
                auditLog.UserId = userId;
                auditLog.RiskScore = 20;

                await SaveAuditLogAsync(auditLog);
                _logger.LogInformation("Privacy event logged: {Details} for user {UserId}", details, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log privacy event for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Gets audit logs for a specific user (for data subject access requests)
        /// </summary>
        public async Task<IEnumerable<AuditLogEntry>> GetUserAuditLogsAsync(int userId, DateTime? startDate = null, DateTime? endDate = null, bool includePersonalData = false)
        {
            // Assumes IUnitOfWork.SecurityAuditLogs.Query() returns IQueryable<SecurityAuditLog>
            var query = _uow.SecurityAuditLogs.Query()
                .Where(l => l.UserId == userId
                            && (!startDate.HasValue || l.Timestamp >= startDate.Value)
                            && (!endDate.HasValue || l.Timestamp <= endDate.Value));

            var logs = await query.ToListAsync();
            return logs.Select(l => new AuditLogEntry
            {
                LogId = l.Id,
                UserId = l.UserId,
                EventType = l.EventType,
                Action = l.EventType,
                Resource = null,
                Details = includePersonalData || !string.IsNullOrEmpty(l.EmailAttempted)
                            ? $"{l.EventDetails} | Email: {l.EmailAttempted}"
                            : l.EventDetails ?? string.Empty,
                Timestamp = l.Timestamp,
                IpAddress = l.IpAddress,
                UserAgent = l.UserAgent,
                Success = l.Success,
                FailureReason = l.FailureReason,
                RiskLevel = (SecurityRiskLevel?)l.RiskScore,
                Metadata = null
            }).ToList();
        }

        /// <summary>
        /// Gets security statistics for a given time period
        /// </summary>
        public async Task<SecurityStatistics> GetSecurityStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _uow.SecurityAuditLogs.Query()
                .Where(l => (!startDate.HasValue || l.Timestamp >= startDate.Value)
                         && (!endDate.HasValue || l.Timestamp <= endDate.Value));

            var all = await query.ToListAsync();
            var total = all.Count;
            var success = all.Count(l => l.EventType == "AUTHENTICATION_ATTEMPT" && l.Success);
            var failed = all.Count(l => l.EventType == "AUTHENTICATION_ATTEMPT" && !l.Success);
            var suspicious = all.Count(l => l.RiskScore >= 80);

            return new SecurityStatistics
            {
                GeneratedAt = DateTime.UtcNow,
                StartDate = startDate,
                EndDate = endDate,
                TotalAuthenticationAttempts = total,
                SuccessfulAuthentications = success,
                FailedAuthentications = failed,
                AuthenticationSuccessRate = total > 0 ? (decimal)success / total * 100m : 0m,
                SuspiciousActivitiesDetected = suspicious,
                ResourceAccessAttempts = all.Count(l => l.EventType == "RESOURCE_ACCESS"),
                UnauthorizedAccessAttempts = all.Count(l => l.EventType == "AUTHORIZATION_EVENT" && !l.Success),
                DataExportEvents = all.Count(l => l.EventType == "DATA_EXPORT"),
                AdminActions = all.Count(l => l.EventType == "ADMIN_ACTION"),
                HighRiskEvents = all.Count(l => l.RiskScore >= (int)SecurityRiskLevel.High),
                CriticalRiskEvents = all.Count(l => l.RiskScore >= (int)SecurityRiskLevel.Critical),
                TopFailureReasons = all.Where(l => !l.Success && l.FailureReason != null)
                                                 .GroupBy(l => l.FailureReason)
                                                 .OrderByDescending(g => g.Count())
                                                 .Take(5)
                                                 .Select(g => new TopFailureReason
                                                 {
                                                     Reason = g.Key!,
                                                     Count = g.Count(),
                                                     Percentage = total > 0 ? (decimal)g.Count() / total * 100m : 0m
                                                 }),
                TopSuspiciousActivities = all.Where(l => l.RiskScore >= (int)SecurityRiskLevel.Medium)
                                                 .GroupBy(l => l.EventType)
                                                 .OrderByDescending(g => g.Count())
                                                 .Take(5)
                                                 .Select(g => new TopSuspiciousActivity
                                                 {
                                                     ActivityType = g.Key,
                                                     Count = g.Count(),
                                                     AverageRiskLevel = (SecurityRiskLevel)g.Average(l => l.RiskScore),
                                                     Percentage = total > 0 ? (decimal)g.Count() / total * 100m : 0m
                                                 })
            };
        }

        /// <summary>
        /// Gets failed authentication attempts for analysis
        /// </summary>
        public async Task<IEnumerable<FailedAuthenticationAnalysis>> GetFailedAuthenticationAnalysisAsync(int timeWindow = 24, int threshold = 3)
        {
            var cutoff = DateTime.UtcNow.AddHours(-timeWindow);
            var logs = await _uow.SecurityAuditLogs.Query()
                .Where(l => l.EventType == "AUTHENTICATION_ATTEMPT" && !l.Success && l.Timestamp >= cutoff)
                .ToListAsync();

            return logs.GroupBy(l => l.EmailAttempted)
                .Select(g => new FailedAuthenticationAnalysis
                {
                    EmailAttempted = g.Key ?? string.Empty,
                    IpAddress = g.Select(l => l.IpAddress).FirstOrDefault(),
                    FailureCount = g.Count(),
                    FirstFailure = g.Min(l => l.Timestamp),
                    LastFailure = g.Max(l => l.Timestamp),
                    FailureReasons = g.Select(l => l.FailureReason).Where(r => r != null!).Distinct().Cast<string>(),
                    RiskLevel = g.Max(l => (SecurityRiskLevel)l.RiskScore)
                })
                .Where(a => a.FailureCount >= threshold);
        }

        /// <summary>
        /// Gets suspicious activity patterns
        /// </summary>
        public async Task<IEnumerable<SuspiciousActivityPattern>> GetSuspiciousActivityPatternsAsync(SecurityRiskLevel riskLevel = SecurityRiskLevel.Medium, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _uow.SecurityAuditLogs.Query()
                .Where(l => l.RiskScore >= (int)riskLevel
                         && (!startDate.HasValue || l.Timestamp >= startDate.Value)
                         && (!endDate.HasValue || l.Timestamp <= endDate.Value));

            var logs = await query.ToListAsync();
            var total = logs.Count;

            return logs.GroupBy(l => l.EventType)
                .Select(g => new SuspiciousActivityPattern
                {
                    ActivityType = g.Key,
                    OccurrenceCount = g.Count(),
                    RiskLevel = riskLevel,
                    FirstOccurrence = g.Min(l => l.Timestamp),
                    LastOccurrence = g.Max(l => l.Timestamp),
                    AffectedUserIds = g.Select(l => l.UserId ?? 0).Distinct(),
                    AffectedIpAddresses = g.Select(l => l.IpAddress ?? string.Empty).Distinct(),
                    Description = g.Select(l => l.EventDetails).FirstOrDefault() ?? string.Empty
                });
        }

        /// <summary>
        /// Logs performance metrics for monitoring
        /// </summary>
        public async Task LogPerformanceMetricAsync(string operation, TimeSpan duration, bool success = true, Dictionary<string, object>? metadata = null)
        {
            try
            {
                var details = $"{operation} took {duration.TotalMilliseconds:F1}ms";
                if (metadata != null)
                    details += $" | Meta: {System.Text.Json.JsonSerializer.Serialize(metadata)}";

                var auditLog = CreateBaseAuditLog(_httpContextAccessor.HttpContext, "PERFORMANCE_METRIC", details, success);
                auditLog.RiskScore = success ? 5 : 50;

                await SaveAuditLogAsync(auditLog);
                _logger.LogInformation("Performance metric logged: {Details}", details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log performance metric for operation {Operation}", operation);
            }
        }

        /// <summary>
        /// Logs system health events
        /// </summary>
        public async Task LogSystemHealthEventAsync(string component, SystemHealthStatus healthStatus, string message, Dictionary<string, object>? metadata = null)
        {
            try
            {
                var details = $"{component} status: {healthStatus}. Message: {message}";
                if (metadata != null)
                    details += $" | Meta: {System.Text.Json.JsonSerializer.Serialize(metadata)}";

                var auditLog = CreateBaseAuditLog(_httpContextAccessor.HttpContext, "SYSTEM_HEALTH", details, healthStatus == SystemHealthStatus.Healthy);
                auditLog.RiskScore = healthStatus == SystemHealthStatus.Healthy ? 0 : (int)healthStatus * 20;

                await SaveAuditLogAsync(auditLog);
                _logger.LogInformation("System health event logged: {Details}", details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log system health event for component {Component}", component);
            }
        }

        /// <summary>
        /// Logs general security events
        /// </summary>
        public async Task LogSecurityEventAsync(
            string eventType,
            string eventDetails,
            HttpContext httpContext,
            bool success,
            int? userId = null,
            string? emailAttempted = null)
        {
            try
            {
                // build base log entry
                var auditLog = CreateBaseAuditLog(httpContext, eventType, eventDetails, success);
                auditLog.UserId = userId;
                if (emailAttempted != null)
                    auditLog.EmailAttempted = MaskEmail(emailAttempted);

                if (!success)
                {
                    auditLog.FailureReason = eventDetails;
                    auditLog.RiskScore = CalculateRiskScore(eventType, httpContext);
                }

                await SaveAuditLogAsync(auditLog);

                var level = success ? LogLevel.Information : LogLevel.Warning;
                _logger.Log(level, "Security event logged: {EventType} (Success: {Success})", eventType, success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log security event: {EventType}", eventType);
            }
        }

        /// <summary>
        /// Logs data access events for sensitive operations
        /// </summary>
        public async Task LogDataAccessAsync(
            int userId,
            string dataType,
            string operation,
            string? recordIds,
            HttpContext? httpContext = null)
        {
            try
            {
                var ctx = httpContext ?? _httpContextAccessor.HttpContext!;
                var details = $"Data access: {operation} on {dataType}"
                            + (recordIds != null ? $" (IDs: {recordIds})" : "");
                var auditLog = CreateBaseAuditLog(ctx, "DATA_ACCESS", details, true);
                auditLog.UserId = userId;
                auditLog.RiskScore = CalculateDataAccessRiskScore(operation, dataType);

                await SaveAuditLogAsync(auditLog);
                _logger.LogInformation("Data access logged: {Operation} on {DataType} by user {UserId}", operation, dataType, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log data access for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Logs API rate limit violations
        /// </summary>
        public async Task LogRateLimitViolationAsync(
            string ipAddress,
            string endpoint,
            int requestCount,
            TimeSpan timeWindow)
        {
            try
            {
                var details = $"Rate limit exceeded: {requestCount} calls to {endpoint} in {timeWindow.TotalMinutes:F1} minutes";
                var auditLog = CreateBaseAuditLog(_httpContextAccessor.HttpContext!, "RATE_LIMIT_VIOLATION", details, false);
                auditLog.IpAddress = ipAddress;
                auditLog.RiskScore = Math.Min(100, 20 + (requestCount / 10));

                await SaveAuditLogAsync(auditLog);
                _logger.LogWarning("Rate limit violation: {Count} calls to {Endpoint} from {Ip}", requestCount, endpoint, ipAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log rate limit violation for IP {Ip}", ipAddress);
            }
        }


        // =====================================================
        // Private Helper Methods
        // =====================================================

        /// <summary>
        /// Saves audit log entry to database with error handling
        /// </summary>
        private async Task SaveAuditLogAsync(SecurityAuditLog logEntry)
        {
            try
            {
                await _uow.SecurityAuditLogs.AddAsync(logEntry);
                await _uow.SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save security audit log: {EventType}", logEntry.EventType);
            }
        }

        /// <summary>
        /// Creates a base audit log entry with common fields
        /// </summary>
        private SecurityAuditLog CreateBaseAuditLog(HttpContext? httpContext, string eventType, string eventDetails, bool success)
        {
            var context = httpContext ?? _httpContextAccessor.HttpContext;
            return new SecurityAuditLog
            {
                EventType = eventType,
                EventDetails = eventDetails,
                Success = success,
                IpAddress = GetClientIpAddress(context),
                UserAgent = GetUserAgent(context),
                SessionId = GetSessionId(context),
                GeoLocation = GetGeoLocation(context),
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Calculates risk score for authentication attempts
        /// </summary>
        private static int CalculateAuthenticationRiskScore(string? ipAddress, bool success, string? failureReason)
        {
            int score = 0;
            if (!success) score += 30;
            if (!string.IsNullOrEmpty(failureReason))
            {
                if (failureReason.Contains("locked", StringComparison.OrdinalIgnoreCase)) score += 40;
                else if (failureReason.Contains("invalid", StringComparison.OrdinalIgnoreCase)) score += 20;
                else if (failureReason.Contains("expired", StringComparison.OrdinalIgnoreCase)) score += 15;
                else if (failureReason.Contains("brute", StringComparison.OrdinalIgnoreCase)) score += 50;
            }
            if (!string.IsNullOrEmpty(ipAddress) && IsSuspiciousIp(ipAddress)) score += 25;
            return Math.Min(100, score);
        }

        /// <summary>
        /// Calculates risk score based on event type and context
        /// </summary>
        private int CalculateRiskScore(string eventType, HttpContext? httpContext)
        {
            var baseScore = eventType switch
            {
                "InvalidTokenAccess" => 50,
                "UnauthorizedAccess" => 70,
                "SuspiciousActivity" => 80,
                "DataBreach" => 100,
                "MultipleFailedAttempts" => 60,
                "UnusualLoginLocation" => 40,
                _ => 10
            };

            var ip = GetClientIpAddress(httpContext);
            var ua = GetUserAgent(httpContext);
            if (string.IsNullOrEmpty(ua) || ua.Length < 10) baseScore += 20;
            if (IsPrivateIpAddress(ip)) baseScore -= 10;
            return Math.Min(baseScore, 100);
        }

        /// <summary>
        /// Calculates risk score for data access operations
        /// </summary>
        private static int CalculateDataAccessRiskScore(string operation, string dataType)
        {
            var baseScore = operation.ToLower() switch
            {
                "read" => 5,
                "create" => 10,
                "update" => 15,
                "delete" => 25,
                "export" => 30,
                "bulk_export" => 50,
                _ => 10
            };

            var sensitive = new[] { "user", "payment", "personal", "financial", "medical" };
            if (sensitive.Any(s => dataType.Contains(s, StringComparison.OrdinalIgnoreCase)))
                baseScore += 15;

            return Math.Min(100, baseScore);
        }

        /// <summary>
        /// Extracts client IP address from HTTP context with multiple fallbacks
        /// </summary>
        private string GetClientIpAddress(HttpContext? httpContext = null)
        {
            try
            {
                var ctx = httpContext ?? _httpContextAccessor.HttpContext;
                if (ctx == null) return "Unknown";

                var forwarded = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwarded))
                    return forwarded.Split(',')[0].Trim();

                var real = ctx.Request.Headers["X-Real-IP"].FirstOrDefault();
                if (!string.IsNullOrEmpty(real)) return real;

                var cf = ctx.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
                if (!string.IsNullOrEmpty(cf)) return cf;

                return ctx.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            }
            catch { return "Unknown"; }
        }

        /// <summary>
        /// Extracts user agent from HTTP context
        /// </summary>
        private string? GetUserAgent(HttpContext? httpContext = null)
        {
            try
            {
                var ctx = httpContext ?? _httpContextAccessor.HttpContext;
                return ctx?.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
            }
            catch { return "Unknown"; }
        }

        /// <summary>
        /// Extracts session ID from HTTP context
        /// </summary>
        private string? GetSessionId(HttpContext? httpContext = null)
        {
            try
            {
                var ctx = httpContext ?? _httpContextAccessor.HttpContext;
                return ctx?.Session?.Id;
            }
            catch { return null; }
        }

        /// <summary>
        /// Gets geographical location information (enhanced implementation)
        /// </summary>
        private string? GetGeoLocation(HttpContext? httpContext = null)
        {
            try
            {
                var ctx = httpContext ?? _httpContextAccessor.HttpContext;
                var ip = GetClientIpAddress(ctx);
                if (IsPrivateIpAddress(ip)) return "Internal Network";

                var country = ctx?.Request.Headers["CF-IPCountry"].FirstOrDefault();
                if (!string.IsNullOrEmpty(country) && country != "XX")
                    return country;

                return "Location lookup not implemented";
            }
            catch { return null; }
        }

        /// <summary>
        /// Checks if an IP address is in a private range
        /// </summary>
        private static bool IsPrivateIpAddress(string? ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress) || ipAddress == "Unknown")
                return false;
            try
            {
                if (System.Net.IPAddress.TryParse(ipAddress, out var ip))
                {
                    var b = ip.GetAddressBytes();
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return (b[0] == 10)
                            || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)
                            || (b[0] == 192 && b[1] == 168)
                            || (b[0] == 127);
                    }
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        return ip.Equals(System.Net.IPAddress.IPv6Loopback)
                            || ip.ToString().StartsWith("fe80:", StringComparison.OrdinalIgnoreCase);
                    }
                }
                return false;
            }
            catch { return false; }
        }

        /// <summary>
        /// Checks for suspicious IP patterns
        /// </summary>
        private static bool IsSuspiciousIp(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return false;
            var patterns = new[] { "0.0.0.0", "255.255.255.255", "169.254.", "224.", "240." };
            return patterns.Any(p => ipAddress.StartsWith(p));
        }

        /// <summary>
        /// Masks email address for privacy while keeping it useful for analysis
        /// </summary>
        private static string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains('@'))
                return "***@unknown.com";
            try
            {
                var parts = email.Split('@');
                if (parts.Length != 2) return "***@unknown.com";
                var local = parts[0];
                var domain = parts[1];
                if (local.Length <= 2) return $"***@{domain}";
                if (local.Length <= 4) return $"{local[0]}***@{domain}";
                return $"{local[0]}***{local[^1]}@{domain}";
            }
            catch { return "***@unknown.com"; }
        }

        public Task LogResourceAccessAsync(int? userId, string resourceType, int resourceId, string action, string? details = null, bool success = true)
        {
            throw new NotImplementedException();
        }
    }
}
