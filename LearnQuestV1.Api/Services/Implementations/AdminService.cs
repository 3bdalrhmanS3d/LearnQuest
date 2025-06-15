using LearnQuestV1.Api.DTOs.Admin;
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
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            IUnitOfWork uow,
            IEmailQueueService emailQueueService,
            IActionLogService logService,
            ILogger<AdminService> logger)
        {
            _uow = uow;
            _emailQueueService = emailQueueService;
            _logService = logService;
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

                user.Role = UserRole.Instructor;
                _uow.Users.Update(user);
                await _uow.SaveAsync();

                await _logService.LogAsync(adminId, targetUserId, "MakeInstructor",
                    $"User {user.EmailAddress} promoted to Instructor");
                await _uow.SaveAsync();

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

                user.Role = UserRole.Admin;
                _uow.Users.Update(user);
                await _uow.SaveAsync();

                await _logService.LogAsync(adminId, targetUserId, "MakeAdmin",
                    $"User {user.EmailAddress} promoted to Admin");
                await _uow.SaveAsync();

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

                user.Role = UserRole.RegularUser;
                _uow.Users.Update(user);
                await _uow.SaveAsync();

                await _logService.LogAsync(adminId, targetUserId, "MakeRegularUser",
                    $"User {user.EmailAddress} demoted to RegularUser");
                await _uow.SaveAsync();

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

                await _logService.LogAsync(adminId, targetUserId, "SoftDeleteUser",
                    $"User {user.EmailAddress} marked as deleted");
                await _uow.SaveAsync();

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

                await _logService.LogAsync(adminId, targetUserId, "RecoverUser",
                    $"User {user.EmailAddress} recovered");
                await _uow.SaveAsync();

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

                user.IsActive = !user.IsActive;
                _uow.Users.Update(user);
                await _uow.SaveAsync();

                await _logService.LogAsync(adminId, targetUserId, "ToggleUserActivation",
                    $"User {user.EmailAddress} activation toggled to {(user.IsActive ? "enabled" : "disabled")}");
                await _uow.SaveAsync();

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

                _emailQueueService.QueueEmail(user.EmailAddress, user.FullName, subject, bodyMessage);

                var notif = new Notification
                {
                    UserId = user.UserId,
                    Message = $"{subject} - {bodyMessage}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _uow.Notifications.AddAsync(notif);
                await _uow.SaveAsync();

                await _logService.LogAsync(adminId, user.UserId, "SendNotification",
                    $"Notification sent to {user.EmailAddress}: {subject}");
                await _uow.SaveAsync();

                _logger.LogInformation("Notification sent to user {UserId} by admin {AdminId}", input.UserId, adminId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", input.UserId);
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

                return new
                {
                    Period = new { StartDate = startDate, EndDate = endDate },
                    UserCreationTrend = userCreations,
                    CourseCreationTrend = courseCreations,
                    EnrollmentTrend = enrollmentTrend,
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
                        .Where(log => log.ActionDate >= cutoffDate)           // استخدم Timestamp أو الخاصية الصحيحة
                        .GroupBy(log => log.ActionType)                       // أو ActionType بحسب ما لديك
                        .Select(g => new { ActionType = g.Key, Count = g.Count() })
                        .ToListAsync(),

                    UserGrowth = await _uow.Users.Query()
                        .CountAsync(u => u.CreatedAt >= cutoffDate && !u.IsDeleted),

                    ActiveUsers = await _uow.Users.Query()
                        .Where(u => u.IsActive && !u.IsDeleted)
                        .Where(u => u.VisitHistories
                                        .Any(vh => vh.LastVisit >= cutoffDate))
                        .CountAsync()
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
                            .CountAsync(e => e.EnrolledAt >= todayStart)
                    },
                    ThisWeek = new
                    {
                        NewUsers = await _uow.Users.Query()
                            .CountAsync(u => u.CreatedAt >= weekStart && !u.IsDeleted),
                        NewEnrollments = await _uow.CourseEnrollments.Query()
                            .CountAsync(e => e.EnrolledAt >= weekStart)
                    },
                    ThisMonth = new
                    {
                        NewUsers = await _uow.Users.Query()
                            .CountAsync(u => u.CreatedAt >= monthStart && !u.IsDeleted),
                        NewEnrollments = await _uow.CourseEnrollments.Query()
                            .CountAsync(e => e.EnrolledAt >= monthStart)
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

        public async Task<dynamic> GetUsersWithFilteringAsync(
         string? role = null,
         bool? isVerified = null,
         string? searchTerm = null,
         int pageNumber = 1,
         int pageSize = 20)
        {
            try
            {
                // Note: explicitly IQueryable<User> here
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

        #region Private Helper Methods

        private async Task<User> FindUserByIdAsync(int userId)
        {
            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with id {userId} not found.");

            return user;
        }

        private (string subject, string bodyMessage) GetTemplateMessage(NotificationTemplateType template, string fullName)
        {
            return template switch
            {
                NotificationTemplateType.AccountActivated => (
                    "✅ Your Account has been Activated",
                    $"Hello {fullName},\n\nYour account has been successfully activated. You can now login and enjoy our services."
                ),
                NotificationTemplateType.AccountDeactivated => (
                    "⚠️ Account Deactivated",
                    $"Hello {fullName},\n\nYour account has been temporarily deactivated. Please contact support for further information."
                ),
                NotificationTemplateType.AccountDeleted => (
                    "🗑️ Account Deleted",
                    $"Hello {fullName},\n\nYour account has been deleted from our platform. If this was a mistake, please contact support immediately."
                ),
                NotificationTemplateType.AccountRestored => (
                    "♻️ Account Restored",
                    $"Hello {fullName},\n\nGood news! Your account has been restored. Welcome back!"
                ),
                NotificationTemplateType.GeneralAnnouncement => (
                    "📢 Important Announcement",
                    $"Hello {fullName},\n\nWe have an important update for you. Please check your dashboard for more information."
                ),
                _ => (
                    "📬 Notification",
                    $"Hello {fullName},\n\nThis is a notification from the administration."
                )
            };
        }

        #endregion
    }
}