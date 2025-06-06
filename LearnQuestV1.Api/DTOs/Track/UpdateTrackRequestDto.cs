namespace LearnQuestV1.Api.DTOs.Track
{
    public class UpdateTrackRequestDto
    {
        public int TrackId { get; set; }
        public string? TrackName { get; set; }
        public string? TrackDescription { get; set; }
    }
}
