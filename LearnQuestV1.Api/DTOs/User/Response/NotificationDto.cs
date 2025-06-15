namespace LearnQuestV1.Api.DTOs.User.Response
{
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
}
