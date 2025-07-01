using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace LearnQuestV1.Api.Services.Interfaces
{
    /// <summary>
    /// Enhanced comprehensive security audit logging service interface
    /// </summary>
    public interface ISecurityAuditLogger
    {
        // =====================================================
        // Core Security Audit Methods
        // =====================================================

        /// <summary>
        /// Logs authentication attempts (both successful and failed)
        /// </summary>
        /// <param name="emailAttempted">Email address used in login attempt</param>
        /// <param name="ipAddress">IP address of the request</param>
        /// <param name="success">Whether the authentication was successful</param>
        /// <param name="failureReason">Reason for failure if unsuccessful</param>
        /// <param name="userId">User ID if authentication was successful</param>
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

        /// <summary>
        /// Logs general security events
        /// </summary>
        /// <param name="eventType">Type of security event</param>
        /// <param name="eventDetails">Detailed description of the event</param>
        /// <param name="httpContext">Current HTTP context</param>
        /// <param name="success">Whether the security event was successful</param>
        /// <param name="userId">Optional user ID if known</param>
        /// <param name="emailAttempted">Optional email if this was a login attempt</param>
        Task LogSecurityEventAsync(string eventType, string eventDetails, HttpContext httpContext, bool success, int? userId = null, string? emailAttempted = null);

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

        // =====================================================
        // New Methods
        // (marked with “// +”)
        // =====================================================

        /// <summary>
        /// Logs user permission and authorization events
        /// </summary>
        /// <param name="userId">User attempting the action</param>
        /// <param name="action">Action being attempted</param>
        /// <param name="resource">Resource being accessed</param>
        /// <param name="success">Whether authorization was successful</param>
        /// <param name="denialReason">Reason for denial if unsuccessful</param>
        Task LogAuthorizationEventAsync(int userId, string action, string resource, bool success, string? denialReason = null); // +

        /// <summary>
        /// Logs resource access events (CRUD operations)
        /// </summary>
        /// <param name="userId">User accessing the resource</param>
        /// <param name="resourceType">Type of resource (e.g., "Content", "Course", "User")</param>
        /// <param name="resourceId">ID of the specific resource</param>
        /// <param name="action">Action performed (CREATE, READ, UPDATE, DELETE)</param>
        /// <param name="details">Additional details about the action</param>
        /// <param name="success">Whether the action was successful</param>
        Task LogResourceAccessAsync(int? userId, string resourceType, int resourceId, string action, string? details = null, bool success = true); // +

        /// <summary>
        /// Logs suspicious activities that may indicate security threats
        /// </summary>
        /// <param name="userId">User ID if known, null for anonymous</param>
        /// <param name="activityType">Type of suspicious activity</param>
        /// <param name="description">Description of the activity</param>
        /// <param name="riskLevel">Risk level assessment</param>
        /// <param name="ipAddress">IP address of the request</param>
        /// <param name="additionalData">Additional context data</param>
        Task LogSuspiciousActivityAsync(int? userId, string activityType, string description, SecurityRiskLevel riskLevel, string? ipAddress = null, Dictionary<string, object>? additionalData = null); // +

        /// <summary>
        /// Logs data export/download events for compliance
        /// </summary>
        /// <param name="userId">User performing the export</param>
        /// <param name="dataType">Type of data being exported</param>
        /// <param name="recordCount">Number of records exported</param>
        /// <param name="exportFormat">Format of the export (CSV, PDF, etc.)</param>
        /// <param name="purpose">Purpose of the export</param>
        Task LogDataExportAsync(int userId, string dataType, int recordCount, string exportFormat, string? purpose = null); // +

        /// <summary>
        /// Logs content creation events with detailed metadata
        /// </summary>
        /// <param name="userId">User creating the content</param>
        /// <param name="contentId">ID of the created content</param>
        /// <param name="contentType">Type of content (Video, Document, Text)</param>
        /// <param name="sectionId">Section the content belongs to</param>
        /// <param name="title">Title of the content</param>
        /// <param name="metadata">Additional content metadata</param>
        Task LogContentCreationAsync(int userId, int contentId, string contentType, int sectionId, string title, Dictionary<string, object>? metadata = null); // +

        /// <summary>
        /// Logs content modification events
        /// </summary>
        /// <param name="userId">User modifying the content</param>
        /// <param name="contentId">ID of the modified content</param>
        /// <param name="changes">Description of changes made</param>
        /// <param name="previousValues">Previous values for changed fields</param>
        /// <param name="newValues">New values for changed fields</param>
        Task LogContentModificationAsync(int userId, int contentId, string changes, Dictionary<string, object>? previousValues = null, Dictionary<string, object>? newValues = null); // +

        /// <summary>
        /// Logs content deletion/archival events
        /// </summary>
        /// <param name="userId">User deleting the content</param>
        /// <param name="contentId">ID of the deleted content</param>
        /// <param name="title">Title of the deleted content</param>
        /// <param name="deletionType">Type of deletion (soft delete, hard delete, archive)</param>
        /// <param name="reason">Reason for deletion</param>
        Task LogContentDeletionAsync(int userId, int contentId, string title, string deletionType, string? reason = null); // +

        /// <summary>
        /// Logs content visibility changes
        /// </summary>
        /// <param name="userId">User changing visibility</param>
        /// <param name="contentId">ID of the content</param>
        /// <param name="previousVisibility">Previous visibility state</param>
        /// <param name="newVisibility">New visibility state</param>
        /// <param name="reason">Reason for visibility change</param>
        Task LogContentVisibilityChangeAsync(int userId, int contentId, bool previousVisibility, bool newVisibility, string? reason = null); // +

        /// <summary>
        /// Logs bulk content operations
        /// </summary>
        /// <param name="userId">User performing bulk operation</param>
        /// <param name="operation">Type of bulk operation</param>
        /// <param name="contentIds">IDs of affected content</param>
        /// <param name="successCount">Number of successful operations</param>
        /// <param name="failureCount">Number of failed operations</param>
        /// <param name="details">Additional operation details</param>
        Task LogBulkContentOperationAsync(int userId, string operation, IEnumerable<int> contentIds, int successCount, int failureCount, string? details = null); // +

        /// <summary>
        /// Logs file upload events
        /// </summary>
        /// <param name="userId">User uploading the file</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="fileSize">File size in bytes</param>
        /// <param name="fileType">MIME type or extension</param>
        /// <param name="uploadPath">Path where file was stored</param>
        /// <param name="success">Whether upload was successful</param>
        /// <param name="errorMessage">Error message if upload failed</param>
        Task LogFileUploadAsync(int userId, string fileName, long fileSize, string fileType, string uploadPath, bool success = true, string? errorMessage = null); // +

        /// <summary>
        /// Logs file deletion events
        /// </summary>
        /// <param name="userId">User deleting the file</param>
        /// <param name="filePath">Path of the deleted file</param>
        /// <param name="fileName">Name of the deleted file</param>
        /// <param name="reason">Reason for deletion</param>
        /// <param name="success">Whether deletion was successful</param>
        Task LogFileDeletionAsync(int userId, string filePath, string fileName, string? reason = null, bool success = true); // +

        /// <summary>
        /// Logs file access/download events
        /// </summary>
        /// <param name="userId">User accessing the file</param>
        /// <param name="filePath">Path of the accessed file</param>
        /// <param name="fileName">Name of the accessed file</param>
        /// <param name="accessType">Type of access (view, download)</param>
        /// <param name="contentId">Associated content ID if applicable</param>
        Task LogFileAccessAsync(int userId, string filePath, string fileName, string accessType, int? contentId = null); // +

        /// <summary>
        /// Logs administrative actions performed by admins
        /// </summary>
        /// <param name="adminId">Admin user performing the action</param>
        /// <param name="action">Administrative action taken</param>
        /// <param name="targetType">Type of target (User, Course, System, etc.)</param>
        /// <param name="targetId">ID of the target if applicable</param>
        /// <param name="description">Description of the action</param>
        /// <param name="impactLevel">Level of impact of the action</param>
        Task LogAdministrativeActionAsync(int adminId, string action, string targetType, int? targetId, string description, AdminActionImpactLevel impactLevel); // +

        /// <summary>
        /// Logs system configuration changes
        /// </summary>
        /// <param name="adminId">Admin making the change</param>
        /// <param name="configurationKey">Configuration key that was changed</param>
        /// <param name="previousValue">Previous configuration value</param>
        /// <param name="newValue">New configuration value</param>
        /// <param name="changeReason">Reason for the change</param>
        Task LogSystemConfigurationChangeAsync(int adminId, string configurationKey, string? previousValue, string newValue, string? changeReason = null); // +

        /// <summary>
        /// Logs user management actions (create, modify, disable, etc.)
        /// </summary>
        /// <param name="adminId">Admin performing the action</param>
        /// <param name="action">Action performed on user</param>
        /// <param name="targetUserId">ID of the target user</param>
        /// <param name="targetUserEmail">Email of the target user</param>
        /// <param name="changes">Description of changes made</param>
        /// <param name="reason">Reason for the action</param>
        Task LogUserManagementActionAsync(int adminId, string action, int targetUserId, string targetUserEmail, string changes, string? reason = null); // +

        /// <summary>
        /// Logs data access for compliance (GDPR, CCPA, etc.)
        /// </summary>
        /// <param name="userId">User whose data was accessed</param>
        /// <param name="accessedBy">User who accessed the data</param>
        /// <param name="dataType">Type of personal data accessed</param>
        /// <param name="purpose">Purpose of data access</param>
        /// <param name="legalBasis">Legal basis for data processing</param>
        Task LogPersonalDataAccessAsync(int userId, int accessedBy, string dataType, string purpose, string? legalBasis = null); // +

        /// <summary>
        /// Logs privacy-related events (consent, data portability, etc.)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="eventType">Type of privacy event</param>
        /// <param name="description">Description of the event</param>
        /// <param name="consentGiven">Whether consent was given (if applicable)</param>
        /// <param name="metadata">Additional event metadata</param>
        Task LogPrivacyEventAsync(int userId, string eventType, string description, bool? consentGiven = null, Dictionary<string, object>? metadata = null); // +

        /// <summary>
        /// Gets audit logs for a specific user (for data subject access requests)
        /// </summary>
        /// <param name="userId">User ID to get audit logs for</param>
        /// <param name="startDate">Start date for log retrieval</param>
        /// <param name="endDate">End date for log retrieval</param>
        /// <param name="includePersonalData">Whether to include personal data in logs</param>
        /// <returns>Collection of audit log entries</returns>
        Task<IEnumerable<AuditLogEntry>> GetUserAuditLogsAsync(int userId, DateTime? startDate = null, DateTime? endDate = null, bool includePersonalData = false); // +

        /// <summary>
        /// Gets security statistics for a given time period
        /// </summary>
        /// <param name="startDate">Start date for statistics</param>
        /// <param name="endDate">End date for statistics</param>
        /// <returns>Security statistics summary</returns>
        Task<SecurityStatistics> GetSecurityStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null); // +

        /// <summary>
        /// Gets failed authentication attempts for analysis
        /// </summary>
        /// <param name="timeWindow">Time window to analyze (in hours)</param>
        /// <param name="threshold">Minimum number of failures to include</param>
        /// <returns>Failed authentication analysis</returns>
        Task<IEnumerable<FailedAuthenticationAnalysis>> GetFailedAuthenticationAnalysisAsync(int timeWindow = 24, int threshold = 3); // +

        /// <summary>
        /// Gets suspicious activity patterns
        /// </summary>
        /// <param name="riskLevel">Minimum risk level to include</param>
        /// <param name="startDate">Start date for analysis</param>
        /// <param name="endDate">End date for analysis</param>
        /// <returns>Suspicious activity patterns</returns>
        Task<IEnumerable<SuspiciousActivityPattern>> GetSuspiciousActivityPatternsAsync(SecurityRiskLevel riskLevel = SecurityRiskLevel.Medium, DateTime? startDate = null, DateTime? endDate = null); // +

        /// <summary>
        /// Logs performance metrics for monitoring
        /// </summary>
        /// <param name="operation">Operation being measured</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="success">Whether operation was successful</param>
        /// <param name="metadata">Additional performance metadata</param>
        /// <returns>Task for async operation</returns>
        Task LogPerformanceMetricAsync(string operation, TimeSpan duration, bool success = true, Dictionary<string, object>? metadata = null); // +

        /// <summary>
        /// Logs system health events
        /// </summary>
        /// <param name="component">System component</param>
        /// <param name="healthStatus">Health status</param>
        /// <param name="message">Health message</param>
        /// <param name="metadata">Additional health metadata</param>
        /// <returns>Task for async operation</returns>
        Task LogSystemHealthEventAsync(string component, SystemHealthStatus healthStatus, string message, Dictionary<string, object>? metadata = null); // +
    }

    /// <summary>
    /// Security risk levels for suspicious activity logging
    /// </summary>
    public enum SecurityRiskLevel
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    /// <summary>
    /// Administrative action impact levels
    /// </summary>
    public enum AdminActionImpactLevel
    {
        Low = 1,
        Medium = 2,
        High = 3,
        SystemWide = 4
    }

    /// <summary>
    /// System health status levels
    /// </summary>
    public enum SystemHealthStatus
    {
        Healthy = 1,
        Warning = 2,
        Error = 3,
        Critical = 4
    }

    /// <summary>
    /// Audit log entry for compliance and reporting
    /// </summary>
    public class AuditLogEntry
    {
        public int LogId { get; set; }
        public int? UserId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public bool Success { get; set; }
        public string? FailureReason { get; set; }
        public SecurityRiskLevel? RiskLevel { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Security statistics summary
    /// </summary>
    public class SecurityStatistics
    {
        public DateTime GeneratedAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TotalAuthenticationAttempts { get; set; }
        public int SuccessfulAuthentications { get; set; }
        public int FailedAuthentications { get; set; }
        public decimal AuthenticationSuccessRate { get; set; }
        public int SuspiciousActivitiesDetected { get; set; }
        public int ResourceAccessAttempts { get; set; }
        public int UnauthorizedAccessAttempts { get; set; }
        public int DataExportEvents { get; set; }
        public int AdminActions { get; set; }
        public int HighRiskEvents { get; set; }
        public int CriticalRiskEvents { get; set; }
        public IEnumerable<TopFailureReason> TopFailureReasons { get; set; } = new List<TopFailureReason>();
        public IEnumerable<TopSuspiciousActivity> TopSuspiciousActivities { get; set; } = new List<TopSuspiciousActivity>();
    }

    /// <summary>
    /// Failed authentication analysis result
    /// </summary>
    public class FailedAuthenticationAnalysis
    {
        public string EmailAttempted { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public int FailureCount { get; set; }
        public DateTime FirstFailure { get; set; }
        public DateTime LastFailure { get; set; }
        public IEnumerable<string> FailureReasons { get; set; } = new List<string>();
        public SecurityRiskLevel RiskLevel { get; set; }
    }

    /// <summary>
    /// Suspicious activity pattern
    /// </summary>
    public class SuspiciousActivityPattern
    {
        public string ActivityType { get; set; } = string.Empty;
        public int OccurrenceCount { get; set; }
        public SecurityRiskLevel RiskLevel { get; set; }
        public DateTime FirstOccurrence { get; set; }
        public DateTime LastOccurrence { get; set; }
        public IEnumerable<int> AffectedUserIds { get; set; } = new List<int>();
        public IEnumerable<string> AffectedIpAddresses { get; set; } = new List<string>();
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Top failure reason statistics
    /// </summary>
    public class TopFailureReason
    {
        public string Reason { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// Top suspicious activity statistics
    /// </summary>
    public class TopSuspiciousActivity
    {
        public string ActivityType { get; set; } = string.Empty;
        public int Count { get; set; }
        public SecurityRiskLevel AverageRiskLevel { get; set; }
        public decimal Percentage { get; set; }
    }
}
