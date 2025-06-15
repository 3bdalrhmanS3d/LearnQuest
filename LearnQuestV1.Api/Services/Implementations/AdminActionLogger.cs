using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Models.Administration;
using LearnQuestV1.EF.Application;
using Microsoft.EntityFrameworkCore;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class AdminActionLogger : IAdminActionLogger
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminActionLogger> _logger;

        public AdminActionLogger(ApplicationDbContext context, ILogger<AdminActionLogger> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogActionAsync(int adminId, int? targetUserId, string actionType, string actionDetails, string? ipAddress = null)
        {
            try
            {
                var adminActionLog = new AdminActionLog
                {
                    AdminId = adminId,
                    TargetUserId = targetUserId,
                    ActionType = actionType,
                    ActionDetails = actionDetails,
                    IpAddress = ipAddress ?? "Unknown",
                    ActionDate = DateTime.UtcNow
                };

                _context.AdminActionLogs.Add(adminActionLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Admin action logged: {ActionType} by admin {AdminId} on target {TargetUserId}",
                    actionType, adminId, targetUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log admin action: {ActionType} by admin {AdminId}", actionType, adminId);
                // Don't throw - logging failures shouldn't break the main operation
            }
        }

        public async Task<IEnumerable<dynamic>> GetAdminActionsAsync(
            int? adminId = null,
            string? actionType = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageSize = 50,
            int pageNumber = 1)
        {
            try
            {
                var query = _context.AdminActionLogs
                    .Include(log => log.Admin)
                    .Include(log => log.TargetUser)
                    .AsQueryable();

                // Apply filters
                if (adminId.HasValue)
                    query = query.Where(log => log.AdminId == adminId.Value);

                if (!string.IsNullOrEmpty(actionType))
                    query = query.Where(log => log.ActionType == actionType);

                if (startDate.HasValue)
                    query = query.Where(log => log.ActionDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(log => log.ActionDate <= endDate.Value);

                // Apply pagination and ordering
                var logs = await query
                    .OrderByDescending(log => log.ActionDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(log => new
                    {
                        log.LogId,
                        log.AdminId,
                        AdminName = log.Admin.FullName,
                        AdminEmail = log.Admin.EmailAddress,
                        log.TargetUserId,
                        TargetUserName = log.TargetUser != null ? log.TargetUser.FullName : null,
                        TargetUserEmail = log.TargetUser != null ? log.TargetUser.EmailAddress : null,
                        log.ActionType,
                        log.ActionDetails,
                        log.ActionDate,
                        log.IpAddress
                    })
                    .ToListAsync();

                _logger.LogDebug("Retrieved {LogCount} admin action logs", logs.Count);
                return logs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin actions");
                return Enumerable.Empty<dynamic>();
            }
        }

        public async Task<IEnumerable<dynamic>> GetUserActionHistoryAsync(int targetUserId, int pageSize = 20, int pageNumber = 1)
        {
            try
            {
                var history = await _context.AdminActionLogs
                    .Include(log => log.Admin)
                    .Where(log => log.TargetUserId == targetUserId)
                    .OrderByDescending(log => log.ActionDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(log => new
                    {
                        log.LogId,
                        log.AdminId,
                        AdminName = log.Admin.FullName,
                        AdminEmail = log.Admin.EmailAddress,
                        log.ActionType,
                        log.ActionDetails,
                        log.ActionDate,
                        log.IpAddress
                    })
                    .ToListAsync();

                _logger.LogDebug("Retrieved {HistoryCount} action history records for user {UserId}", history.Count, targetUserId);
                return history;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId} action history", targetUserId);
                return Enumerable.Empty<dynamic>();
            }
        }

        public async Task<dynamic> GetActionStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Default to last 30 days if no dates provided
                startDate ??= DateTime.UtcNow.AddDays(-30);
                endDate ??= DateTime.UtcNow;

                var query = _context.AdminActionLogs
                    .Where(log => log.ActionDate >= startDate && log.ActionDate <= endDate);

                var totalActions = await query.CountAsync();

                var actionsByType = await query
                    .GroupBy(log => log.ActionType)
                    .Select(g => new
                    {
                        ActionType = g.Key,
                        Count = g.Count(),
                        Percentage = Math.Round((double)g.Count() / totalActions * 100, 2)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();

                var actionsByAdmin = await query
                    .Include(log => log.Admin)
                    .GroupBy(log => new { log.AdminId, log.Admin.FullName })
                    .Select(g => new
                    {
                        AdminId = g.Key.AdminId,
                        AdminName = g.Key.FullName,
                        Count = g.Count(),
                        Percentage = Math.Round((double)g.Count() / totalActions * 100, 2)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();

                var dailyActions = await query
                    .GroupBy(log => log.ActionDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                var statistics = new
                {
                    Period = new
                    {
                        StartDate = startDate,
                        EndDate = endDate,
                        Days = (endDate - startDate)?.Days ?? 0
                    },
                    Overview = new
                    {
                        TotalActions = totalActions,
                        AveragePerDay = totalActions / Math.Max(1, (endDate - startDate)?.Days ?? 1),
                        UniqueAdmins = actionsByAdmin.Count(),
                        MostActiveAdmin = actionsByAdmin.FirstOrDefault()?.AdminName ?? "N/A"
                    },
                    ActionsByType = actionsByType,
                    ActionsByAdmin = actionsByAdmin,
                    DailyTrend = dailyActions,
                    GeneratedAt = DateTime.UtcNow
                };

                _logger.LogDebug("Generated action statistics for period {StartDate} to {EndDate}: {TotalActions} actions",
                    startDate, endDate, totalActions);

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating action statistics");
                return new
                {
                    Error = "Failed to generate statistics",
                    GeneratedAt = DateTime.UtcNow
                };
            }
        }
    }
}