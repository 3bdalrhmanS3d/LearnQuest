namespace LearnQuestV1.Core.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Get the start of the week (Monday)
        /// </summary>
        public static DateTime StartOfWeek(this DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// Get the end of the week (Sunday)
        /// </summary>
        public static DateTime EndOfWeek(this DateTime date)
        {
            return date.StartOfWeek().AddDays(6);
        }

        /// <summary>
        /// Check if date is today
        /// </summary>
        public static bool IsToday(this DateTime date)
        {
            return date.Date == DateTime.UtcNow.Date;
        }

        /// <summary>
        /// Check if date is this week
        /// </summary>
        public static bool IsThisWeek(this DateTime date)
        {
            var startOfWeek = DateTime.UtcNow.StartOfWeek();
            var endOfWeek = startOfWeek.AddDays(7);
            return date >= startOfWeek && date < endOfWeek;
        }

        /// <summary>
        /// Get friendly time ago string
        /// </summary>
        public static string ToFriendlyString(this DateTime date)
        {
            var timeSpan = DateTime.UtcNow - date;

            if (timeSpan.TotalMinutes < 1) return "Just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays}d ago";
            if (timeSpan.TotalDays < 30) return $"{(int)(timeSpan.TotalDays / 7)}w ago";
            if (timeSpan.TotalDays < 365) return $"{(int)(timeSpan.TotalDays / 30)}mo ago";

            return date.ToString("MMM yyyy");
        }
    }
}
