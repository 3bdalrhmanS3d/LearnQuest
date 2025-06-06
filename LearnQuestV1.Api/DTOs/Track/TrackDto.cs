namespace LearnQuestV1.Api.DTOs.Track
{
    public class TrackDto
    {
        public int TrackId { get; set; }
        public string TrackName { get; set; } = string.Empty;
        public string TrackDescription { get; set; } = string.Empty;
        public string? TrackImage { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CourseCount { get; set; }
    }
}
