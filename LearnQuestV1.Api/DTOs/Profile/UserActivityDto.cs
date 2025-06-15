namespace LearnQuestV1.Api.DTOs.Profile
{
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

