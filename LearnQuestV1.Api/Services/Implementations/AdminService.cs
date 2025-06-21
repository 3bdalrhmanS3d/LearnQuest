using LearnQuestV1.Api.DTOs.Admin;
using LearnQuestV1.Api.DTOs.Notifications;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Models.Communication;
using LearnQuestV1.Core.Models.UserManagement;
using Microsoft.EntityFrameworkCore;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _uow;
        private readonly IActionLogService _logService;
        private readonly IEmailQueueService _emailQueueService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            IUnitOfWork uow,
            IEmailQueueService emailQueueService,
            IActionLogService logService,
            INotificationService notificationService,
            ILogger<AdminService> logger)
        {
            _uow = uow;
            _emailQueueService = emailQueueService;
            _logService = logService;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Returns two lists: verified users and unverified users.
        /// </summary>
        public async Task<(IEnumerable<AdminUserDto> Activated, IEnumerable<AdminUserDto> NotActivated)>
            GetUsersGroupedByVerificationAsync()
        {
            try
            {
                var allUsers = await _uow.Users.Query()
                    .Where(u => !u.IsDeleted)
                    .Include(u => u.AccountVerifications)
                    .ToListAsync();

                var mapped = allUsers.Select(u => new AdminUserDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    EmailAddress = u.EmailAddress,
                    Role = u.Role.ToString(),
                    IsVerified = u.AccountVerifications.Any(av => av.CheckedOK),
                    CreatedAt = u.CreatedAt,
                    IsActive = u.IsActive
                });

                var activated = mapped.Where(u => u.IsVerified);
                var notActivated = mapped.Where(u => !u.IsVerified);

                return (activated, notActivated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users grouped by verification");
                throw;
            }
        }

        /// <summary>
        /// For backward compatibility with your original /all-users endpoint: returns both counts and lists.
        /// </summary>
        public async Task<IEnumerable<AdminUserDto>> GetAllUsersAsync()
        {
            var (activated, notActivated) = await GetUsersGroupedByVerificationAsync();
            return activated.Concat(notActivated);
        }

        /// <summary>
        /// Return the "basic info" for a single user, including details if present.
        /// </summary>
        public async Task<BasicUserInfoDto> GetBasicUserInfoAsync(int userId)
        {
            try
            {
                var user = await _uow.Users.Query()
                    .Where(u => !u.IsDeleted && u.UserId == userId)
                    .Include(u => u.UserDetail)
                    .Include(u => u.VisitHistories)
                    .FirstOrDefaultAsync();

                if (user == null)
                    throw new KeyNotFoundException($"User with id {userId} not found.");

                // pick the latest LastVisit, or null if none
                var lastVisit = user.VisitHistories
                    .OrderByDescending(vh => vh.LastVisit)
                    .FirstOrDefault()
                    ?.LastVisit;

                var dto = new BasicUserInfoDto
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    EmailAddress = user.EmailAddress,
                    Role = user.Role.ToString(),
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = lastVisit,   // correctly assigned
                    Details = user.UserDetail is null
                        ? null
                        : new BasicUserInfoDto.UserDetailDto
                        {
                            BirthDate = user.UserDetail.BirthDate,
                            EducationLevel = user.UserDetail.EducationLevel,
                            Nationality = user.UserDetail.Nationality,
                            CreatedAt = user.UserDetail.CreatedAt
                        }
                };

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting basic user info for user {UserId}", userId);
                throw;
            }
        }

        public async Task PromoteToInstructorAsync(int adminId, int targetUserId)
        {
            try
            {
                var user = await FindUserByIdAsync(targetUserId);

                if (user.IsSystemProtected)
                    throw new InvalidOperationException("This user is system-protected and cannot be modified.");

                if (user.Role == UserRole.Instructor)
                    throw new InvalidOperationException("User is already an Instructor.");

                var previousRole = user.Role.ToString();
                user.Role = UserRole.Instructor;
                _uow.Users.Update(user);
                await _uow.SaveAsync();

                // Log admin action
                await _logService.LogAsync(adminId, targetUserId, "MakeInstructor",
                    $"User {user.EmailAddress} promoted to Instructor");
                await _uow.SaveAsync();

                // Send new notification system notification
                await SendUserRoleChangeNotificationAsync(targetUserId, "Instructor", previousRole, isPromotion: true);

                _logger.LogInformation("User {UserId} promoted to Instructor by admin {AdminId}", targetUserId, adminId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error promoting user {UserId} to Instructor", targetUserId);
                throw;
            }
        }

        public async Task PromoteToAdminAsync(int adminId, int targetUserId)
        {
            try
            {
                var user = await FindUserByIdAsync(targetUserId);

                if (user.Role == UserRole.Admin)
                    throw new InvalidOperationException("User is already an Admin.");

                var previousRole = user.Role.ToString();
                user.Role = UserRole.Admin;
                _uow.Users.Update(user);
                await _uow.SaveAsync();

                // Log admin action
                await _logService.LogAsync(adminId, targetUserId, "MakeAdmin",
                    $"User {user.EmailAddress} promoted to Admin");
                await _uow.SaveAsync();

                // Send new notification system notification
                await SendUserRoleChangeNotificationAsync(targetUserId, "Admin", previousRole, isPromotion: true);

                _logger.LogInformation("User {UserId} promoted to Admin by admin {AdminId}", targetUserId, adminId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error promoting user {UserId} to Admin", targetUserId);
                throw;
            }
        }

        public async Task DemoteToRegularUserAsync(int adminId, int targetUserId)
        {
            try
            {
                var user = await FindUserByIdAsync(targetUserId);

                if (user.IsSystemProtected)
                    throw new InvalidOperationException("This user is system-protected and cannot be modified.");

                if (user.Role == UserRole.RegularUser)
                    throw new InvalidOperationException("User is already a Regular User.");

                var previousRole = user.Role.ToString();
                user.Role = UserRole.RegularUser;
                _uow.Users.Update(user);
                await _uow.SaveAsync();

                // Log admin action
                await _logService.LogAsync(adminId, targetUserId, "MakeRegularUser",
                    $"User {user.EmailAddress} demoted to RegularUser");
                await _uow.SaveAsync();

                // Send new notification system notification
                await SendUserRoleChangeNotificationAsync(targetUserId, "Regular User", previousRole, isPromotion: false);

                _logger.LogInformation("User {UserId} demoted to Regular User by admin {AdminId}", targetUserId, adminId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error demoting user {UserId} to Regular User", targetUserId);
                throw;
            }
        }

        public async Task DeleteUserAsync(int adminId, int targetUserId)
        {
            try
            {
                if (adminId == targetUserId)
                    throw new InvalidOperationException("You cannot delete your own account.");

                var user = await FindUserByIdAsync(targetUserId);

                if (user.IsSystemProtected)
                    throw new InvalidOperationException("Cannot delete system-protected user.");

                if (user.IsDeleted)
                    throw new InvalidOperationException("User is already deleted.");

                user.IsDeleted = true;
                _uow.Users.Update(user);
                await _uow.SaveAsync();

                // Log admin action
                await _logService.LogAsync(adminId, targetUserId, "SoftDeleteUser",
                    $"User {user.EmailAddress} marked as deleted");
                await _uow.SaveAsync();

                // Send account deletion notification
                await SendAccountStatusNotificationAsync(targetUserId, "AccountDeleted");

                _logger.LogInformation("User {UserId} soft-deleted by admin {AdminId}", targetUserId, adminId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", targetUserId);
                throw;
            }
        }

        public async Task RecoverUserAsync(int adminId, int targetUserId)
        {
            try
            {
                var user = await FindUserByIdAsync(targetUserId);

                if (!user.IsDeleted)
                    throw new InvalidOperationException("User is not deleted.");

                user.IsDeleted = false;
                _uow.Users.Update(user);
                await _uow.SaveAsync();

                // Log admin action
                await _logService.LogAsync(adminId, targetUserId, "RecoverUser",
                    $"User {user.EmailAddress} recovered");
                await _uow.SaveAsync();

                // Send account recovery notification
                await SendAccountStatusNotificationAsync(targetUserId, "AccountRestored");

                _logger.LogInformation("User {UserId} recovered by admin {AdminId}", targetUserId, adminId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recovering user {UserId}", targetUserId);
                throw;
            }
        }

        public async Task ToggleUserActivationAsync(int adminId, int targetUserId)
        {
            try
            {
                var user = await FindUserByIdAsync(targetUserId);

                if (user.IsSystemProtected)
                    throw new InvalidOperationException("Cannot modify system-protected user.");

                var wasActive = user.IsActive;
                user.IsActive = !user.IsActive;
                _uow.Users.Update(user);
                await _uow.SaveAsync();

                // Log admin action
                await _logService.LogAsync(adminId, targetUserId, "ToggleUserActivation",
                    $"User {user.EmailAddress} activation toggled to {(user.IsActive ? "enabled" : "disabled")}");
                await _uow.SaveAsync();

                // Send activation status notification
                var statusType = user.IsActive ? "AccountActivated" : "AccountDeactivated";
                await SendAccountStatusNotificationAsync(targetUserId, statusType);

                _logger.LogInformation("User {UserId} activation toggled to {Status} by admin {AdminId}",
                    targetUserId, user.IsActive ? "enabled" : "disabled", adminId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling activation for user {UserId}", targetUserId);
                throw;
            }
        }

        public async Task<IEnumerable<AdminActionLogDto>> GetAllAdminActionsAsync()
        {
            try
            {
                var logs = await _uow.AdminActionLogs.Query()
                    .Include(l => l.Admin)
                    .Include(l => l.TargetUser)
                    .OrderByDescending(l => l.ActionDate)
                    .Take(100) // Limit to recent 100 actions
                    .ToListAsync();

                return logs.Select(l => new AdminActionLogDto
                {
                    LogId = l.LogId,
                    AdminName = l.Admin.FullName,
                    AdminEmail = l.Admin.EmailAddress,
                    TargetUserName = l.TargetUser?.FullName,
                    TargetUserEmail = l.TargetUser?.EmailAddress,
                    ActionType = l.ActionType,
                    ActionDetails = l.ActionDetails,
                    ActionDate = l.ActionDate,
                    IpAddress = l.IpAddress
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin action logs");
                throw;
            }
        }

        public async Task<IEnumerable<UserVisitHistory>> GetUserVisitHistoryAsync(int userId)
        {
            try
            {
                return await _uow.UserVisitHistories.FindAsync(v => v.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting visit history for user {UserId}", userId);
                throw;
            }
        }

        public async Task<SystemStatsDto> GetSystemStatisticsAsync()
        {
            try
            {
                var totalUsers = await _uow.Users.Query().CountAsync(u => !u.IsDeleted);

                var activatedUsers = await _uow.Users.Query()
                    .Include(u => u.AccountVerifications)
                    .CountAsync(u => u.AccountVerifications.Any(av => av.CheckedOK) && !u.IsDeleted);

                var totalCoursesActive = await _uow.Courses.Query().CountAsync(c => c.IsActive && !c.IsDeleted);

                var totalRegularUsers = await _uow.Users.Query()
                    .CountAsync(u => u.Role == UserRole.RegularUser && !u.IsDeleted);
                var totalInstructors = await _uow.Users.Query()
                    .CountAsync(u => u.Role == UserRole.Instructor && !u.IsDeleted);
                var totalAdmins = await _uow.Users.Query()
                    .CountAsync(u => u.Role == UserRole.Admin && !u.IsDeleted);

                var totalEnrollments = await _uow.CourseEnrollments.Query().CountAsync();
                var totalRevenue = await _uow.Payments.Query()
                    .Where(p => p.Status == PaymentStatus.Completed)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;

                return new SystemStatsDto
                {
                    TotalUsers = totalUsers,
                    ActivatedUsers = activatedUsers,
                    NotActivatedUsers = Math.Max(0, totalUsers - activatedUsers),
                    TotalRegularUsers = totalRegularUsers,
                    TotalAdmins = totalAdmins,
                    TotalInstructors = totalInstructors,
                    TotalCourses = totalCoursesActive,
                    TotalEnrollments = totalEnrollments,
                    TotalRevenue = totalRevenue,
                    LastUpdated = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system statistics");
                throw;
            }
        }

        /// <summary>
        /// Enhanced notification sending with new notification system integration
        /// Maintains backward compatibility with email system
        /// </summary>
        public async Task SendNotificationAsync(int adminId, AdminSendNotificationInput input)
        {
            try
            {
                var user = await _uow.Users.GetByIdAsync(input.UserId)
                           ?? throw new KeyNotFoundException("User not found.");

                string subject;
                string bodyMessage;

                if (input.TemplateType.HasValue)
                {
                    (subject, bodyMessage) = GetTemplateMessage(input.TemplateType.Value, user.FullName);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(input.Subject) || string.IsNullOrWhiteSpace(input.Message))
                        throw new InvalidOperationException("Custom subject and message are required if no template is selected.");

                    subject = input.Subject!;
                    bodyMessage = input.Message!;
                }

                // Send via email queue (existing functionality)
                _emailQueueService.QueueEmail(user.EmailAddress, user.FullName, subject, bodyMessage);

                // Send via new notification system
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = user.UserId,
                    Title = subject,
                    Message = bodyMessage,
                    Type = "System",
                    Priority = "High",
                    Icon = "Mail"
                });

                // Keep old notification for backward compatibility (if still needed)
                var legacyNotif = new Notification
                {
                    UserId = user.UserId,
                    Message = $"{subject} - {bodyMessage}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _uow.Notifications.AddAsync(legacyNotif);
                await _uow.SaveAsync();

                // Log admin action
                await _logService.LogAsync(adminId, user.UserId, "SendNotification",
                    $"Notification sent to {user.EmailAddress}: {subject}");
                await _uow.SaveAsync();

                _logger.LogInformation("Enhanced notification sent to user {UserId} by admin {AdminId}", input.UserId, adminId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending enhanced notification to user {UserId}", input.UserId);
                throw;
            }
        }

        /// <summary>
        /// Send bulk notifications to multiple users using new notification system
        /// </summary>
        public async Task SendBulkNotificationAsync(int adminId, List<int> userIds, string title, string message, string type = "System", string priority = "Normal")
        {
            try
            {
                // Validate users exist
                var validUsers = await _uow.Users.Query()
                    .Where(u => userIds.Contains(u.UserId) && !u.IsDeleted && u.IsActive)
                    .ToListAsync();

                if (!validUsers.Any())
                    throw new InvalidOperationException("No valid users found for bulk notification.");

                // Use new notification system for bulk sending
                var bulkDto = new BulkCreateNotificationDto
                {
                    UserIds = validUsers.Select(u => u.UserId).ToList(),
                    Title = title,
                    Message = message,
                    Type = type,
                    Priority = priority,
                    Icon = GetIconForNotificationType(type)
                };

                await _notificationService.CreateBulkNotificationAsync(bulkDto);

                // Log admin action
                await _logService.LogAsync(adminId, 0, "SendBulkNotification",
                    $"Bulk notification sent to {validUsers.Count} users: {title}");
                await _uow.SaveAsync();

                _logger.LogInformation("Bulk notification sent to {UserCount} users by admin {AdminId}", validUsers.Count, adminId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk notification by admin {AdminId}", adminId);
                throw;
            }
        }

        /// <summary>
        /// Send system-wide announcement to all active users
        /// </summary>
        public async Task SendSystemAnnouncementAsync(int adminId, string title, string message, string priority = "High")
        {
            try
            {
                // Get all active users
                var activeUserIds = await _uow.Users.Query()
                    .Where(u => !u.IsDeleted && u.IsActive)
                    .Select(u => u.UserId)
                    .ToListAsync();

                if (!activeUserIds.Any())
                {
                    _logger.LogWarning("No active users found for system announcement");
                    return;
                }

                // Send system-wide notification
                await SendBulkNotificationAsync(adminId, activeUserIds, title, message, "System", priority);

                // Also queue emails for important announcements if priority is High
                if (priority == "High")
                {
                    var activeUsers = await _uow.Users.Query()
                        .Where(u => !u.IsDeleted && u.IsActive)
                        .ToListAsync();

                    foreach (var user in activeUsers)
                    {
                        _emailQueueService.QueueEmail(user.EmailAddress, user.FullName, title, message);
                    }
                }

                _logger.LogInformation("System announcement sent to {UserCount} users by admin {AdminId}", activeUserIds.Count, adminId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending system announcement by admin {AdminId}", adminId);
                throw;
            }
        }

        public async Task<dynamic> GetAdminAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddDays(-30);
                endDate ??= DateTime.UtcNow;

                var userCreations = await _uow.Users.Query()
                    .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate && !u.IsDeleted)
                    .GroupBy(u => u.CreatedAt.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                var courseCreations = await _uow.Courses.Query()
                    .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate && !c.IsDeleted)
                    .GroupBy(c => c.CreatedAt.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                var enrollmentTrend = await _uow.CourseEnrollments.Query()
                    .Where(e => e.EnrolledAt >= startDate && e.EnrolledAt <= endDate)
                    .GroupBy(e => e.EnrolledAt.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                // Get notification analytics
                var notificationStats = await _notificationService.GetNotificationAnalyticsAsync(startDate.Value, endDate.Value);

                return new
                {
                    Period = new { StartDate = startDate, EndDate = endDate },
                    UserCreationTrend = userCreations,
                    CourseCreationTrend = courseCreations,
                    EnrollmentTrend = enrollmentTrend,
                    NotificationAnalytics = notificationStats,
                    Summary = new
                    {
                        NewUsers = userCreations.Sum(u => u.Count),
                        NewCourses = courseCreations.Sum(c => c.Count),
                        NewEnrollments = enrollmentTrend.Sum(e => e.Count)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin analytics");
                throw;
            }
        }

        public async Task<dynamic> GetUserManagementStatsAsync(int timeframe = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-timeframe);

                var stats = new
                {
                    RecentActions = await _uow.AdminActionLogs.Query()
                        .Where(log => log.ActionDate >= cutoffDate)
                        .GroupBy(log => log.ActionType)
                        .Select(g => new { ActionType = g.Key, Count = g.Count() })
                        .ToListAsync(),

                    UserGrowth = await _uow.Users.Query()
                        .CountAsync(u => u.CreatedAt >= cutoffDate && !u.IsDeleted),

                    ActiveUsers = await _uow.Users.Query()
                        .Where(u => u.IsActive && !u.IsDeleted)
                        .Where(u => u.VisitHistories
                                        .Any(vh => vh.LastVisit >= cutoffDate))
                        .CountAsync(),

                    NotificationStats = await _notificationService.GetNotificationAnalyticsAsync(cutoffDate, DateTime.UtcNow)
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user management stats");
                throw;
            }
        }

        public async Task<dynamic> GetSecurityAuditSummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddDays(-7);
                endDate ??= DateTime.UtcNow;

                // Simulate async behavior for placeholder data  
                return await Task.FromResult(new
                {
                    Period = new { StartDate = startDate, EndDate = endDate },
                    Note = "Security audit summary requires SecurityAuditLogs integration",
                    FailedLogins = 0,
                    SuspiciousActivities = 0,
                    SuccessfulLogins = 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting security audit summary");
                throw;
            }
        }

        public async Task<dynamic> GetPlatformActivityAsync()
        {
            try
            {
                var todayStart = DateTime.Today;
                var weekStart = todayStart.AddDays(-7);
                var monthStart = todayStart.AddDays(-30);

                var activity = new
                {
                    Today = new
                    {
                        NewUsers = await _uow.Users.Query()
                            .CountAsync(u => u.CreatedAt >= todayStart && !u.IsDeleted),
                        NewEnrollments = await _uow.CourseEnrollments.Query()
                            .CountAsync(e => e.EnrolledAt >= todayStart),
                        NotificationsSent = await _uow.UserNotifications.Query()
                            .CountAsync(n => n.CreatedAt >= todayStart)
                    },
                    ThisWeek = new
                    {
                        NewUsers = await _uow.Users.Query()
                            .CountAsync(u => u.CreatedAt >= weekStart && !u.IsDeleted),
                        NewEnrollments = await _uow.CourseEnrollments.Query()
                            .CountAsync(e => e.EnrolledAt >= weekStart),
                        NotificationsSent = await _uow.UserNotifications.Query()
                            .CountAsync(n => n.CreatedAt >= weekStart)
                    },
                    ThisMonth = new
                    {
                        NewUsers = await _uow.Users.Query()
                            .CountAsync(u => u.CreatedAt >= monthStart && !u.IsDeleted),
                        NewEnrollments = await _uow.CourseEnrollments.Query()
                            .CountAsync(e => e.EnrolledAt >= monthStart),
                        NotificationsSent = await _uow.UserNotifications.Query()
                            .CountAsync(n => n.CreatedAt >= monthStart)
                    }
                };

                return activity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting platform activity");
                throw;
            }
        }

        public async Task<int> GetActiveUserCountAsync()
            => await _uow.Users.Query().CountAsync(u => !u.IsDeleted && u.IsActive);

        public async Task<dynamic> GetUsersWithFilteringAsync(
         string? role = null,
         bool? isVerified = null,
         string? searchTerm = null,
         int pageNumber = 1,
         int pageSize = 20)
        {
            try
            {
                IQueryable<User> query = _uow.Users.Query()
                    .Where(u => !u.IsDeleted)
                    .Include(u => u.AccountVerifications);

                // Apply filters
                if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, out var userRole))
                {
                    query = query.Where(u => u.Role == userRole);
                }

                if (isVerified.HasValue)
                {
                    if (isVerified.Value)
                        query = query.Where(u => u.AccountVerifications.Any(av => av.CheckedOK));
                    else
                        query = query.Where(u => !u.AccountVerifications.Any(av => av.CheckedOK));
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    query = query.Where(u =>
                        u.FullName.ToLower().Contains(searchLower) ||
                        u.EmailAddress.ToLower().Contains(searchLower));
                }

                var totalCount = await query.CountAsync();

                var users = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new AdminUserDto
                    {
                        UserId = u.UserId,
                        FullName = u.FullName,
                        EmailAddress = u.EmailAddress,
                        Role = u.Role.ToString(),
                        IsVerified = u.AccountVerifications.Any(av => av.CheckedOK),
                        CreatedAt = u.CreatedAt,
                        IsActive = u.IsActive
                    })
                    .ToListAsync();

                return new
                {
                    Users = users,
                    Pagination = new
                    {
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    },
                    Filters = new
                    {
                        Role = role,
                        IsVerified = isVerified,
                        SearchTerm = searchTerm
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users with filtering");
                throw;
            }
        }

        #region Private Helper Methods - Enhanced with Notification System

        private async Task<User> FindUserByIdAsync(int userId)
        {
            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with id {userId} not found.");

            return user;
        }

        /// <summary>
        /// Send role change notification using new notification system
        /// </summary>
        private async Task SendUserRoleChangeNotificationAsync(int userId, string newRole, string previousRole, bool isPromotion)
        {
            try
            {
                var title = isPromotion
                    ? $"🎉 Congratulations! You've been promoted to {newRole}"
                    : $"📋 Your role has been changed to {newRole}";

                var message = isPromotion
                    ? $"Your account has been upgraded from {previousRole} to {newRole}. You now have access to additional features and capabilities!"
                    : $"Your account role has been changed from {previousRole} to {newRole}. Please review your new permissions and capabilities.";

                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = isPromotion ? "Achievement" : "System",
                    Priority = "High",
                    Icon = isPromotion ? "Trophy" : "Settings"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending role change notification to user {UserId}", userId);
            }
        }

        /// <summary>
        /// Send account status notification using new notification system
        /// </summary>
        private async Task SendAccountStatusNotificationAsync(int userId, string statusType)
        {
            try
            {
                var (title, message, icon) = statusType switch
                {
                    "AccountActivated" => (
                        "✅ Account Activated",
                        "Your account has been activated! You can now access all platform features.",
                        "CheckCircle"
                    ),
                    "AccountDeactivated" => (
                        "⚠️ Account Deactivated",
                        "Your account has been temporarily deactivated. Please contact support if you have questions.",
                        "AlertTriangle"
                    ),
                    "AccountDeleted" => (
                        "🗑️ Account Scheduled for Deletion",
                        "Your account has been marked for deletion. Contact support immediately if this was a mistake.",
                        "Trash2"
                    ),
                    "AccountRestored" => (
                        "♻️ Account Restored",
                        "Good news! Your account has been restored and you can access the platform again.",
                        "RotateCcw"
                    ),
                    _ => (
                        "📋 Account Status Update",
                        "Your account status has been updated by an administrator.",
                        "Settings"
                    )
                };

                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = "System",
                    Priority = "High",
                    Icon = icon
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending account status notification to user {UserId}", userId);
            }
        }

        /// <summary>
        /// Get appropriate icon for notification type
        /// </summary>
        private string GetIconForNotificationType(string type)
        {
            return type.ToLower() switch
            {
                "system" => "Settings",
                "announcement" => "Megaphone",
                "security" => "Shield",
                "achievement" => "Trophy",
                "reminder" => "Clock",
                "welcome" => "Hand",
                _ => "Bell"
            };
        }

        /// <summary>
        /// Legacy template message method - enhanced with emoji and better formatting
        /// </summary>
        private (string subject, string bodyMessage) GetTemplateMessage(NotificationTemplateType template, string fullName)
        {
            return template switch
            {
                NotificationTemplateType.AccountActivated => (
                    "✅ Your Account has been Activated",
                    $"Hello {fullName},\n\nGreat news! Your account has been successfully activated. You can now login and enjoy all our platform features.\n\nWelcome to the community!"
                ),
                NotificationTemplateType.AccountDeactivated => (
                    "⚠️ Account Deactivated",
                    $"Hello {fullName},\n\nYour account has been temporarily deactivated. Please contact our support team for further information and assistance.\n\nSupport Email: support@learnquest.com"
                ),
                NotificationTemplateType.AccountDeleted => (
                    "🗑️ Account Deletion Notice",
                    $"Hello {fullName},\n\nYour account has been scheduled for deletion from our platform. If this was a mistake or you wish to recover your account, please contact support immediately.\n\nSupport Email: support@learnquest.com"
                ),
                NotificationTemplateType.AccountRestored => (
                    "♻️ Account Restored",
                    $"Hello {fullName},\n\nExcellent news! Your account has been restored and you can access our platform again. Welcome back!\n\nWe're glad to have you with us again."
                ),
                NotificationTemplateType.GeneralAnnouncement => (
                    "📢 Important Announcement",
                    $"Hello {fullName},\n\nWe have an important update for you. Please check your dashboard for more information and latest updates.\n\nThank you for being part of our community!"
                ),
                _ => (
                    "📬 Notification from Administration",
                    $"Hello {fullName},\n\nThis is a notification from the administration team. Please check your account for more details."
                )
            };
        }

        #endregion
    }
}