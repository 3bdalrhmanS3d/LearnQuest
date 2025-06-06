using LearnQuestV1.Api.DTOs.Track;

namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface ITrackService
    {
        Task<int> CreateTrackAsync(CreateTrackRequestDto dto);
        Task UploadTrackImageAsync(int trackId, IFormFile file);
        Task UpdateTrackAsync(UpdateTrackRequestDto dto);
        Task DeleteTrackAsync(int trackId);
        Task AddCourseToTrackAsync(AddCourseToTrackRequestDto dto);
        Task RemoveCourseFromTrackAsync(int trackId, int courseId);
        Task<IEnumerable<TrackDto>> GetAllTracksAsync();
        Task<TrackDetailsDto> GetTrackDetailsAsync(int trackId);
    }
}
