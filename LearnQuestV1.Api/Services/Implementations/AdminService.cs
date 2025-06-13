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

        public AdminService(IUnitOfWork uow, IEmailQueueService emailQueueService, IActionLogService logService)
        {
            _uow = uow;
            _emailQueueService = emailQueueService;
            _logService = logService;
        }

        /// <summary>
        /// Returns two lists: verified users and unverified users.
        /// </summary>
        public async Task<(IEnumerable<AdminUserDto> Activated, IEnumerable<AdminUserDto> NotActivated)>
            GetUsersGroupedByVerificationAsync()
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
                IsVerified = u.AccountVerifications.Any(av => av.CheckedOK)
            });

            var activated = mapped.Where(u => u.IsVerified);
            var notActivated = mapped.Where(u => !u.IsVerified);

            return (activated, notActivated);
        }

        /// <summary>
        /// For backward compatibility with your original /all-users endpoint: returns both counts and lists.
        /// </summary>
        public async Task<IEnumerable<AdminUserDto>> GetAllUsersAsync()
        {
            var (activated, notActivated) = await GetUsersGroupedByVerificationAsync();

            // Combine into one list if the caller wants them all
            return activated.Concat(notActivated);
        }

        /// <summary>
        /// Return the “basic info” for a single user, including details if present.
        /// </summary>
        public async Task<BasicUserInfoDto> GetBasicUserInfoAsync(int userId)
        {
            var user = await _uow.Users.Query()
                .Where(u => !u.IsDeleted && u.UserId == userId)
                .Include(u => u.UserDetail)
                .FirstOrDefaultAsync();

            if (user == null)
                throw new KeyNotFoundException($"User with id {userId} not found.");

            var dto = new BasicUserInfoDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                EmailAddress = user.EmailAddress,
                Role = user.Role.ToString(),
                Details = user.UserDetail == null
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

        private async Task<User> FindUserByIdAsync(int userId)
        {
            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with id {userId} not found.");

            return user;
        }

        //private async Task LogAdminActionAsync(int adminId, int targetUserId, string actionType, string details)
        //{
        //    var log = new AdminActionLog
        //    {
        //        AdminId = adminId,
        //        TargetUserId = targetUserId,
        //        ActionType = actionType,
        //        ActionDetails = details,
        //        ActionDate = DateTime.UtcNow
        //    };

        //    await _uow.AdminActionLogs.AddAsync(log);
        //    await _uow.SaveAsync();
        //}

        public async Task PromoteToInstructorAsync(int adminId, int targetUserId)
        {
            var user = await FindUserByIdAsync(targetUserId);

            if (user.IsSystemProtected)
                throw new InvalidOperationException("This user is system-protected and cannot be modified.");

            user.Role = UserRole.Instructor;
            _uow.Users.Update(user);
            await _uow.SaveAsync();

            await _logService.LogAsync(adminId, targetUserId, "MakeInstructor", $"User {user.EmailAddress} promoted to Instructor");
            await _uow.SaveAsync();
        }

        public async Task PromoteToAdminAsync(int adminId, int targetUserId)
        {
            var user = await FindUserByIdAsync(targetUserId);
            user.Role = UserRole.Admin;
            _uow.Users.Update(user);
            await _uow.SaveAsync();

            await _logService.LogAsync(adminId, targetUserId, "MakeAdmin", $"User {user.EmailAddress} promoted to Admin");
            await _uow.SaveAsync();
        }

        public async Task DemoteToRegularUserAsync(int adminId, int targetUserId)
        {
            var user = await FindUserByIdAsync(targetUserId);
            if (user.IsSystemProtected)
                throw new InvalidOperationException("This user is system-protected and cannot be modified.");

            user.Role = UserRole.RegularUser;
            _uow.Users.Update(user);
            await _uow.SaveAsync();

            await _logService.LogAsync(adminId, targetUserId, "MakeRegularUser", $"User {user.EmailAddress} demoted to RegularUser");
            await _uow.SaveAsync();
        }

        public async Task DeleteUserAsync(int adminId, int targetUserId)
        {
            if (adminId == targetUserId)
                throw new InvalidOperationException("You cannot delete your own account.");

            var user = await FindUserByIdAsync(targetUserId);
            if (user.IsSystemProtected)
                throw new InvalidOperationException("Cannot delete system-protected user.");

            user.IsDeleted = true;
            _uow.Users.Update(user);
            await _uow.SaveAsync();

            await _logService.LogAsync(adminId, targetUserId, "SoftDeleteUser", $"User {user.EmailAddress} marked as deleted");
            await _uow.SaveAsync();
        }

        public async Task RecoverUserAsync(int adminId, int targetUserId)
        {
            var user = await FindUserByIdAsync(targetUserId);
            if (!user.IsDeleted)
                throw new InvalidOperationException("User is not deleted.");

            user.IsDeleted = false;
            _uow.Users.Update(user);
            await _uow.SaveAsync();

            await _logService.LogAsync(adminId, targetUserId, "RecoverUser", $"User {user.EmailAddress} recovered");
            await _uow.SaveAsync();
        }

        public async Task ToggleUserActivationAsync(int adminId, int targetUserId)
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
        }

        public async Task<IEnumerable<AdminActionLogDto>> GetAllAdminActionsAsync()
        {
            // Use the AdminActionLogs repository directly instead of trying to reach into Notifications.Query().Context
            var logs = await _uow.AdminActionLogs.Query()
                .Include(l => l.Admin)
                .Include(l => l.TargetUser)
                .OrderByDescending(l => l.ActionDate)
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
                ActionDate = l.ActionDate
            });
        }

        public async Task<IEnumerable<UserVisitHistory>> GetUserVisitHistoryAsync(int userId)
        {
            return await _uow.UserVisitHistories.FindAsync(v => v.UserId == userId);
        }

        public async Task<SystemStatsDto> GetSystemStatisticsAsync()
        {
            var totalUsers = await _uow.Users.Query().CountAsync( u => !u.IsDeleted );

            var activatedUsers = await _uow.Users.Query()
                .Include(u => u.AccountVerifications)
                .CountAsync(u => u.AccountVerifications.Any(av => av.CheckedOK));

            var totalCoursesActive = await _uow.Courses.Query().CountAsync(c => c.IsActive);

            var totalRegularUsers = await _uow.Users.Query()
                .CountAsync(u => u.Role == UserRole.RegularUser && !u.IsDeleted);
            var totalInstructors = await _uow.Users.Query().CountAsync(u => u.Role == UserRole.Instructor && !u.IsDeleted);
            var totalAdmins = await _uow.Users.Query().CountAsync(u => u.Role == UserRole.Admin && !u.IsDeleted);

            return new SystemStatsDto
            {
                TotalUsers = totalUsers,
                ActivatedUsers = activatedUsers,
                NotActivatedUsers = Math.Abs(totalUsers - activatedUsers),
                TotalRegularUsers = totalRegularUsers,
                TotalAdmins = totalAdmins,
                TotalInstructors = totalInstructors,
                TotalCourses = totalCoursesActive
            };
        }

        public async Task SendNotificationAsync(int adminId, AdminSendNotificationInput input)
        {
            var user = await _uow.Users.GetByIdAsync(input.UserId)
                       ?? throw new KeyNotFoundException("User not found.");

            string subject;
            string bodyMessage;

            if (input.TemplateType.HasValue)
            {
                // pick from templates
                (subject, bodyMessage) = GetTemplateMessage(input.TemplateType.Value, user.FullName);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(input.Subject) || string.IsNullOrWhiteSpace(input.Message))
                    throw new InvalidOperationException("Custom subject and message are required if no template is selected.");

                subject = input.Subject!;
                bodyMessage = input.Message!;
            }

            // queue email
            await _emailQueueService.SendCustomEmailAsync(user.EmailAddress, user.FullName, subject, bodyMessage);

            // create notification record
            var notif = new Notification
            {
                UserId = user.UserId,
                Message = $"{subject} - {bodyMessage}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            await _uow.Notifications.AddAsync(notif);
            await _uow.SaveAsync();

            await _logService.LogAsync(adminId, user.UserId, "SendNotification", $"Notification sent to {user.EmailAddress}");
            await _uow.SaveAsync();
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
    }
}
