using LearnQuestV1.Api.DTOs.Progress;

using LearnQuestV1.Api.DTOs.Courses;
namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseCDto>> GetAllCoursesForInstructorAsync();
        Task<CourseDetailsDto> GetCourseDetailsAsync(int courseId);
        Task<int> CreateCourseAsync(CreateCourseDto input);
        Task UpdateCourseAsync(int courseId, UpdateCourseDto input);
        Task DeleteCourseAsync(int courseId);
        Task ToggleCourseStatusAsync(int courseId);
        Task<string> UploadCourseImageAsync(int courseId, IFormFile file);
    }
}
