using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Users.Response
{
    /// <summary>
    /// Auto login response DTO
    /// </summary>
    public class AutoLoginResponseDto
    {
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public string Role { get; set; } = null!;
    }

    /// <summary>
    /// Complete section result DTO
    /// </summary>
    public class CompleteSectionResultDto
    {
        public string Message { get; set; } = string.Empty;
        public int? NextSectionId { get; set; }
        public string? NextSectionName { get; set; }
        public int? NextLevelId { get; set; }
        public string? NextLevelName { get; set; }
        public bool IsCourseCompleted { get; set; }
        public int PointsAwarded { get; set; }
    }

    /// <summary>
    /// Course completion DTO
    /// </summary>
    public class CourseCompletionDto
    {
        public int TotalSections { get; set; }
        public int CompletedSections { get; set; }
        public bool IsCompleted { get; set; }
    }

    /// <summary>
    /// Course progress DTO
    /// </summary>
    public class CourseProgressDto
    {
        public int CourseId { get; set; }
        public int ProgressPercentage { get; set; }
    }

    /// <summary>
    /// Next section DTO
    /// </summary>
    public class NextSectionDto
    {
        public bool HasNextSection { get; set; }
        public int? SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int? LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public int ContentCount { get; set; }
        public int EstimatedDuration { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// Notification DTO
    /// </summary>
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Helper properties
        /// </summary>
        public string CreatedAtFormatted => CreatedAt.ToString("MMM dd, yyyy HH:mm");
        public string TimeAgo => GetTimeAgo();
        public string Status => IsRead ? "Read" : "Unread";
        public string StatusClass => IsRead ? "notification-read" : "notification-unread";

        private string GetTimeAgo()
        {
            var timeSpan = DateTime.UtcNow - CreatedAt;

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
    }

    /// <summary>
    /// Refresh token response DTO
    /// </summary>
    public class RefreshTokenResponseDto
    {
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public string RefreshToken { get; set; } = null!;
        public int? UserId { get; internal set; }
    }

    /// <summary>
    /// Sign in response DTO
    /// </summary>
    public class SigninResponseDto
    {
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public string Role { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public int UserId { get; set; }
        public string? AutoLoginToken { get; set; }
    }

    /// <summary>
    /// Student statistics DTO
    /// </summary>
    public class StudentStatsDto
    {
        public int SharedCourses { get; set; }
        public int CompletedSections { get; set; }
        public IEnumerable<CourseProgressDto> Progress { get; set; } = Enumerable.Empty<CourseProgressDto>();
    }
}
