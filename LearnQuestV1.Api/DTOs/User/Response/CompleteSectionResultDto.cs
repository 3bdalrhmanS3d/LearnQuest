namespace LearnQuestV1.Api.DTOs.User.Response
{
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
}
