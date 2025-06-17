namespace LearnQuestV1.Api.DTOs.Track
{
    public class CreateTrackRequestDto
    {
        public string TrackName { get; set; } = string.Empty;
        public string? TrackDescription { get; set; }
        /// <summary>
        /// Optional initial set of Course IDs to assign to this track.
        /// </summary>
        public List<int>? CourseIds { get; set; }

        public IFormFile? TrackImage { get; set; } // Optional image upload
    }
}
