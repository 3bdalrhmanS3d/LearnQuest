namespace LearnQuestV1.Api.DTOs.User.Response
{
    public class NextSectionDto
    {
        public bool HasNextSection { get; set; } // ✅ تم استخدامه في الدالة
        public int? SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;

        public int? LevelId { get; set; } // ✅ من nextSection.LevelId
        public string LevelName { get; set; } = string.Empty; // ✅ من nextSection.Level.LevelName

        public int ContentCount { get; set; } // ✅ من nextSection.Contents.Count
        public int EstimatedDuration { get; set; } // ✅ من Sum(c => c.DurationInMinutes)

        public string? Message { get; set; }
    }
}
