using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Profile
{
    public class UserProgressDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int CurrentLevelId { get; set; }
        public int CurrentSectionId { get; set; }
        public DateTime LastUpdated { get; set; }
    }
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string ProfilePhoto { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsProfileComplete { get; set; }
        public Dictionary<string, string>? RequiredFields { get; set; }

        public DateTime? BirthDate { get; set; }
        public string? Edu { get; set; }
        public string? National { get; set; }

        public IEnumerable<UserProgressDto> Progress { get; set; } = Enumerable.Empty<UserProgressDto>();

    }

    public class UserActivityDto
    {
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        /// <summary>
        /// Helper properties
        /// </summary>
        public string TimeAgo => GetTimeAgo();
        public string ActivityIcon => GetActivityIcon();
        public string FormattedTimestamp => Timestamp.ToString("MMM dd, yyyy HH:mm");

        private string GetTimeAgo()
        {
            var timeSpan = DateTime.UtcNow - Timestamp;

            return timeSpan.TotalDays switch
            {
                >= 365 => $"{(int)(timeSpan.TotalDays / 365)} year(s) ago",
                >= 30 => $"{(int)(timeSpan.TotalDays / 30)} month(s) ago",
                >= 1 => $"{(int)timeSpan.TotalDays} day(s) ago",
                _ => timeSpan.TotalHours switch
                {
                    >= 1 => $"{(int)timeSpan.TotalHours} hour(s) ago",
                    _ => timeSpan.TotalMinutes switch
                    {
                        >= 1 => $"{(int)timeSpan.TotalMinutes} minute(s) ago",
                        _ => "Just now"
                    }
                }
            };
        }

        public class ChangePasswordDto
        {
            [Required(ErrorMessage = "Current password is required")]
            [DataType(DataType.Password)]
            public string CurrentPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "New password is required")]
            [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password confirmation is required")]
            [Compare("NewPassword", ErrorMessage = "Password and confirmation password do not match")]
            [DataType(DataType.Password)]
            public string ConfirmPassword { get; set; } = string.Empty;

            [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
            public string? ChangeReason { get; set; }
        }

        public class ChangeUserNameDto
        {
            [Required(ErrorMessage = "New full name is required")]
            [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
            [RegularExpression(@"^[a-zA-Z\u0600-\u06FF\s]+$", ErrorMessage = "Full name can only contain letters and spaces")]
            public string NewFullName { get; set; } = string.Empty;

            [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
            public string? ChangeReason { get; set; }
        }

        public class ChangeUserNameResultDto
        {
            public bool Success { get; set; }
            public string NewFullName { get; set; } = string.Empty;
            public bool RequiresTokenRefresh { get; set; }
            public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        }

        private string GetActivityIcon()
        {
            return ActivityType.ToLower() switch
            {
                var type when type.Contains("login") => "log-in",
                var type when type.Contains("password") => "lock",
                var type when type.Contains("profile") => "user",
                var type when type.Contains("payment") => "credit-card",
                var type when type.Contains("course") => "book",
                var type when type.Contains("photo") => "image",
                var type when type.Contains("favorite") => "heart",
                _ => "activity"
            };
        }
    }
}
