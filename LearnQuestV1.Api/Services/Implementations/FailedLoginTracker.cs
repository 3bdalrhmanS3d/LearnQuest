using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using System.Collections.Concurrent;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class FailedLoginTracker : IFailedLoginTracker
    {
        private readonly ConcurrentDictionary<string, (int Attempts, DateTime LockoutEnd)> _failedLoginAttempts = new();
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FailedLoginTracker> _logger;

        public FailedLoginTracker(IServiceProvider serviceProvider, ILogger<FailedLoginTracker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public void RecordFailedAttempt(string email)
        {
            var normalizedEmail = email.ToLower().Trim();

            _failedLoginAttempts.AddOrUpdate(
                normalizedEmail,
                (1, DateTime.UtcNow), // New entry
                (key, existingValue) => (existingValue.Attempts + 1, DateTime.UtcNow) // Update existing
            );

            _logger.LogWarning("Failed login attempt recorded for {Email}. Total attempts: {Attempts}",
                normalizedEmail, _failedLoginAttempts[normalizedEmail].Attempts);
        }

        public void LockUser(string email)
        {
            var normalizedEmail = email.ToLower().Trim();
            var lockoutEnd = DateTime.UtcNow.AddMinutes(15);

            _failedLoginAttempts[normalizedEmail] = (5, lockoutEnd);

            _logger.LogWarning("User account locked: {Email} until {LockoutEnd}", normalizedEmail, lockoutEnd);

            // Send account locked notification asynchronously
            _ = Task.Run(async () => await SendAccountLockedNotificationAsync(normalizedEmail, lockoutEnd));
        }

        public void ResetFailedAttempts(string email)
        {
            var normalizedEmail = email.ToLower().Trim();

            if (_failedLoginAttempts.TryRemove(normalizedEmail, out var removedValue))
            {
                _logger.LogInformation("Failed login attempts reset for {Email}. Previous attempts: {Attempts}",
                    normalizedEmail, removedValue.Attempts);
            }
        }

        public Dictionary<string, (int Attempts, DateTime LockoutEnd)> GetFailedAttempts()
        {
            // Clean up expired lockouts
            CleanupExpiredLockouts();

            // Return a copy to prevent external modifications
            return new Dictionary<string, (int Attempts, DateTime LockoutEnd)>(_failedLoginAttempts);
        }

        public bool IsAccountLocked(string email)
        {
            var normalizedEmail = email.ToLower().Trim();

            if (_failedLoginAttempts.TryGetValue(normalizedEmail, out var attemptData))
            {
                return attemptData.LockoutEnd > DateTime.UtcNow;
            }

            return false;
        }

        public TimeSpan? GetRemainingLockoutTime(string email)
        {
            var normalizedEmail = email.ToLower().Trim();

            if (_failedLoginAttempts.TryGetValue(normalizedEmail, out var attemptData))
            {
                var remaining = attemptData.LockoutEnd - DateTime.UtcNow;
                return remaining > TimeSpan.Zero ? remaining : null;
            }

            return null;
        }

        private async Task SendAccountLockedNotificationAsync(string email, DateTime unlockTime)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var emailQueueService = scope.ServiceProvider.GetRequiredService<IEmailQueueService>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var user = await GetUserByEmailAsync(email, unitOfWork);
                if (user != null)
                {
                    emailQueueService.QueueAccountLockedEmail(user.EmailAddress, user.FullName, unlockTime);
                    _logger.LogInformation("Account locked notification queued for {Email}", email);
                }
                else
                {
                    _logger.LogWarning("Could not find user {Email} to send account locked notification", email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send account locked notification for {Email}", email);
            }
        }

        private async Task<User?> GetUserByEmailAsync(string email, IUnitOfWork unitOfWork)
        {
            try
            {
                var users = await unitOfWork.Users.FindAsync(u => u.EmailAddress == email);
                return users.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email {Email}", email);
                return null;
            }
        }

        private void CleanupExpiredLockouts()
        {
            var now = DateTime.UtcNow;
            var expiredEntries = _failedLoginAttempts
                .Where(kvp => kvp.Value.LockoutEnd <= now && kvp.Value.Attempts >= 5)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var expiredEntry in expiredEntries)
            {
                _failedLoginAttempts.TryRemove(expiredEntry, out _);
                _logger.LogInformation("Expired lockout entry removed for {Email}", expiredEntry);
            }
        }

        // Cleanup method that can be called periodically
        public void PerformMaintenance()
        {
            CleanupExpiredLockouts();

            // Remove old failed attempts (older than 1 hour)
            var cutoffTime = DateTime.UtcNow.AddHours(-1);
            var oldEntries = _failedLoginAttempts
                .Where(kvp => kvp.Value.LockoutEnd <= cutoffTime && kvp.Value.Attempts < 5)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var oldEntry in oldEntries)
            {
                _failedLoginAttempts.TryRemove(oldEntry, out _);
            }

            if (oldEntries.Count > 0)
            {
                _logger.LogInformation("Cleaned up {Count} old failed login attempt entries", oldEntries.Count);
            }
        }
    }
}