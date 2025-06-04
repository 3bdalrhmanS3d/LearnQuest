namespace LearnQuestV1.Api.DTOs.User.Response
{
    public class CourseCompletionDto
    {
        public int TotalSections { get; set; }
        public int CompletedSections { get; set; }
        public bool IsCompleted { get; set; }
    }
}
