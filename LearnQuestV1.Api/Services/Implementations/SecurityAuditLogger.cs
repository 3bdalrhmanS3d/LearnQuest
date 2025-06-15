using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models.Administration;
using LearnQuestV1.Api.Utilities;

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
                    UserId = success ? userId : null, // Only store userId on successful login
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
                    _logger.LogInformation("Successful login for {Email} from {IpAddress}", MaskEmail(emailAttempted), ipAddress);
                else
                    _logger.LogWarning("Failed login for {Email} from {IpAddress}. Reason: {Reason}", MaskEmail(emailAttempted), ipAddress, failureReason);
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
                    RiskScore = 10, // Low risk for authenticated password change
                    GeoLocation = GetGeoLocation()
                };

                await SaveAuditLogAsync(logEntry);
                _logger.LogInformation("Password changed for user {UserId} from {IpAddress}", userId, ipAddress);
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
                    RiskScore = 75, // High risk event
                    GeoLocation = GetGeoLocation()
                };

                await SaveAuditLogAsync(logEntry);
                _logger.LogWarning("Account lockout triggered for {Email} from {IpAddress}", MaskEmail(email), ipAddress);
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
                    RiskScore = 85, // Very high risk
                    GeoLocation = GetGeoLocation()
                };

                await SaveAuditLogAsync(logEntry);
                _logger.LogWarning("Suspicious activity: {Activity} for {Email} from {IpAddress}", activity, MaskEmail(email), ipAddress);
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
                    _logger.LogInformation("Token refreshed for user {UserId} from {IpAddress}", userId, ipAddress);
                else
                    _logger.LogWarning("Failed token refresh for user {UserId} from {IpAddress}", userId, ipAddress);
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
                    _logger.LogInformation("Successful auto-login from {IpAddress}", ipAddress);
                else
                    _logger.LogWarning("Failed auto-login from {IpAddress}. Reason: {Reason}", ipAddress, failureReason);
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
                    Success = true, // Request was processed (doesn't mean email exists)
                    EventType = "PASSWORD_RESET_REQUEST",
                    EventDetails = "Password reset request submitted",
                    Timestamp = DateTime.UtcNow,
                    UserAgent = GetUserAgent(),
                    RiskScore = 25,
                    GeoLocation = GetGeoLocation()
                };

                await SaveAuditLogAsync(logEntry);
                _logger.LogInformation("Password reset requested for {Email} from {IpAddress}", MaskEmail(email), ipAddress);
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
                    _logger.LogInformation("Email verified for {Email} from {IpAddress}", MaskEmail(email), ipAddress);
                else
                    _logger.LogWarning("Failed email verification for {Email} from {IpAddress}", MaskEmail(email), ipAddress);
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
                    RiskScore = 10, // Default low risk for user actions
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
        /// Logs general security events
        /// </summary>
        public async Task LogSecurityEventAsync(string eventType, string eventDetails, HttpContext httpContext,
            bool success, int? userId = null, string? emailAttempted = null)
        {
            try
            {
                var auditLog = CreateBaseAuditLog(httpContext, eventType, eventDetails, success);
                auditLog.UserId = userId;
                auditLog.EmailAttempted = emailAttempted != null ? MaskEmail(emailAttempted) : null;

                if (!success)
                {
                    auditLog.FailureReason = eventDetails;
                    auditLog.RiskScore = CalculateRiskScore(eventType, httpContext);
                }

                await SaveAuditLogAsync(auditLog);

                var logLevel = success ? LogLevel.Information : LogLevel.Warning;
                _logger.Log(logLevel, "Security event logged: {EventType} - Success: {Success}", eventType, success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log security event: {EventType}", eventType);
            }
        }

        /// <summary>
        /// Logs data access events for sensitive operations
        /// </summary>
        public async Task LogDataAccessAsync(int userId, string dataType, string operation, string? recordIds, HttpContext? httpContext = null)
        {
            try
            {
                var context = httpContext ?? _httpContextAccessor.HttpContext;
                var logEntry = new SecurityAuditLog
                {
                    UserId = userId,
                    IpAddress = GetClientIpAddress(context),
                    Success = true,
                    EventType = "DATA_ACCESS",
                    EventDetails = $"Data access: {operation} on {dataType}" + (recordIds != null ? $" (IDs: {recordIds})" : ""),
                    Timestamp = DateTime.UtcNow,
                    UserAgent = GetUserAgent(context),
                    SessionId = GetSessionId(context),
                    RiskScore = CalculateDataAccessRiskScore(operation, dataType),
                    GeoLocation = GetGeoLocation(context)
                };

                await SaveAuditLogAsync(logEntry);
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
        public async Task LogRateLimitViolationAsync(string ipAddress, string endpoint, int requestCount, TimeSpan timeWindow)
        {
            try
            {
                var logEntry = new SecurityAuditLog
                {
                    IpAddress = ipAddress ?? GetClientIpAddress(),
                    Success = false,
                    EventType = "RATE_LIMIT_VIOLATION",
                    EventDetails = $"Rate limit exceeded: {requestCount} requests to {endpoint} in {timeWindow.TotalMinutes:F1} minutes",
                    Timestamp = DateTime.UtcNow,
                    UserAgent = GetUserAgent(),
                    RiskScore = Math.Min(100, 20 + (requestCount / 10)), // Increase risk based on request count
                    GeoLocation = GetGeoLocation()
                };

                await SaveAuditLogAsync(logEntry);
                _logger.LogWarning("Rate limit violation: {RequestCount} requests to {Endpoint} from {IpAddress}",
                    requestCount, endpoint, ipAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log rate limit violation for {IpAddress}", ipAddress);
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
                // Log to application logger if database logging fails
                _logger.LogError(ex, "Failed to save security audit log: {EventType}", logEntry.EventType);
                // Consider implementing a backup logging mechanism here
            }
        }

        /// <summary>
        /// Creates a base audit log entry with common fields
        /// </summary>
        private SecurityAuditLog CreateBaseAuditLog(HttpContext? httpContext, string eventType,
            string eventDetails, bool success)
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
                else if (failureReason.Contains("brute", StringComparison.OrdinalIgnoreCase))
                    score += 50;
            }

            // Check for suspicious IP patterns
            if (!string.IsNullOrEmpty(ipAddress) && IsSuspiciousIp(ipAddress))
                score += 25;

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

            // Adjust score based on context
            var ipAddress = GetClientIpAddress(httpContext);
            var userAgent = GetUserAgent(httpContext);

            // Increase score for suspicious patterns
            if (string.IsNullOrEmpty(userAgent) || userAgent.Length < 10)
                baseScore += 20;

            if (IsPrivateIpAddress(ipAddress))
                baseScore -= 10; // Internal IPs are less risky

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

            // Increase score for sensitive data types
            var sensitiveData = new[] { "user", "payment", "personal", "financial", "medical" };
            if (sensitiveData.Any(sensitive => dataType.Contains(sensitive, StringComparison.OrdinalIgnoreCase)))
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
                var context = httpContext ?? _httpContextAccessor.HttpContext;
                if (context == null) return "Unknown";

                // Check for forwarded headers first (common in load balancer scenarios)
                var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (ips.Length > 0)
                        return ips[0].Trim();
                }

                var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
                if (!string.IsNullOrEmpty(realIp))
                    return realIp;

                // Check for CloudFlare header
                var cfConnectingIp = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
                if (!string.IsNullOrEmpty(cfConnectingIp))
                    return cfConnectingIp;

                // Fall back to connection remote IP
                return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Extracts user agent from HTTP context
        /// </summary>
        private string? GetUserAgent(HttpContext? httpContext = null)
        {
            try
            {
                var context = httpContext ?? _httpContextAccessor.HttpContext;
                return context?.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Extracts session ID from HTTP context
        /// </summary>
        private string? GetSessionId(HttpContext? httpContext = null)
        {
            try
            {
                var context = httpContext ?? _httpContextAccessor.HttpContext;
                return context?.Session?.Id;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets geographical location information (enhanced implementation)
        /// </summary>
        private string? GetGeoLocation(HttpContext? httpContext = null)
        {
            try
            {
                var context = httpContext ?? _httpContextAccessor.HttpContext;
                var ipAddress = GetClientIpAddress(context);

                if (IsPrivateIpAddress(ipAddress))
                    return "Internal Network";

                // Check for CloudFlare country header
                var cfCountry = context?.Request.Headers["CF-IPCountry"].FirstOrDefault();
                if (!string.IsNullOrEmpty(cfCountry) && cfCountry != "XX")
                    return cfCountry;

                // You could integrate with services like MaxMind GeoIP, IP2Location, etc.
                // For now, return a placeholder
                return "Location lookup not implemented";
            }
            catch
            {
                return null;
            }
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
                    var bytes = ip.GetAddressBytes();

                    // Check for IPv4 private ranges
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return
                            // 10.0.0.0/8
                            (bytes[0] == 10) ||
                            // 172.16.0.0/12
                            (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                            // 192.168.0.0/16
                            (bytes[0] == 192 && bytes[1] == 168) ||
                            // 127.0.0.0/8 (localhost)
                            (bytes[0] == 127);
                    }

                    // Check for IPv6 localhost
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        return ip.Equals(System.Net.IPAddress.IPv6Loopback) ||
                               ip.ToString().StartsWith("fe80:", StringComparison.OrdinalIgnoreCase);
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks for suspicious IP patterns
        /// </summary>
        private static bool IsSuspiciousIp(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return false;

            // Basic suspicious IP detection
            var suspiciousPatterns = new[]
            {
                "0.0.0.0",           // Invalid IP
                "255.255.255.255",   // Broadcast IP
                "169.254.",          // Link-local addresses
                "224.",              // Multicast
                "240."               // Reserved for future use
            };

            return suspiciousPatterns.Any(pattern => ipAddress.StartsWith(pattern));
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
                if (parts.Length != 2)
                    return "***@unknown.com";

                var localPart = parts[0];
                var domain = parts[1];

                if (localPart.Length <= 2)
                    return $"***@{domain}";

                if (localPart.Length <= 4)
                    return $"{localPart[0]}***@{domain}";

                return $"{localPart[0]}***{localPart[^1]}@{domain}";
            }
            catch
            {
                return "***@unknown.com";
            }
        }
    }
}
