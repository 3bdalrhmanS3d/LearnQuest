namespace LearnQuestV1.Api.DTOs.Track
{
    public class TrackDetailsDto
    {
        public int TrackId { get; set; }
        public string TrackName { get; set; } = string.Empty;
        public string TrackDescription { get; set; } = string.Empty;
        public string? TrackImage { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CourseInTrackDto> Courses { get; set; } = new();
    }
}
