using LearnQuestV1.Api.DTOs.Track;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class TrackService : ITrackService
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TrackService(
            IUnitOfWork uow,
            IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor)
        {
            _uow = uow;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Creates a new track under the current instructor’s account.
        /// Returns the newly created TrackId.
        /// </summary>
        public async Task<int> CreateTrackAsync(CreateTrackRequestDto dto)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var instructorId = user?.GetCurrentUserId();
            if (instructorId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            // Verify user is an instructor
            var isInstructor = await _uow.Users.Query()
                .AnyAsync(u =>
                    u.UserId == instructorId.Value
                    && u.Role == UserRole.Instructor
                    && !u.IsDeleted);

            if (!isInstructor)
                throw new KeyNotFoundException($"Instructor with ID {instructorId.Value} not found or deleted.");

            // Create the track
            var track = new CourseTrack
            {
                TrackName = dto.TrackName.Trim(),
                TrackDescription = dto.TrackDescription?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _uow.CourseTracks.AddAsync(track);
            await _uow.SaveAsync();

            // Attach any initial courses (only if they belong to this instructor)
            if (dto.CourseIds != null && dto.CourseIds.Any())
            {
                foreach (var courseId in dto.CourseIds)
                {
                    var course = await _uow.Courses.Query()
                        .FirstOrDefaultAsync(c =>
                            c.CourseId == courseId &&
                            c.InstructorId == instructorId.Value &&
                            !c.IsDeleted);

                    if (course != null)
                    {
                        var join = new CourseTrackCourse
                        {
                            TrackId = track.TrackId,
                            CourseId = course.CourseId
                        };
                        await _uow.CourseTrackCourses.AddAsync(join);
                    }
                }
                await _uow.SaveAsync();
            }

            return track.TrackId;
        }

        /// <summary>
        /// Uploads a new image for the specified track.
        /// Stores under wwwroot/uploads/TrackImages.
        /// </summary>
        public async Task UploadTrackImageAsync(int trackId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("No file provided.");

            var user = _httpContextAccessor.HttpContext?.User;
            var instructorId = user?.GetCurrentUserId();
            if (instructorId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            // Verify track belongs to this instructor
            var track = await _uow.CourseTracks.Query()
                .FirstOrDefaultAsync(t => t.TrackId == trackId);

            if (track == null)
                throw new KeyNotFoundException($"Track {trackId} not found.");

            // (Optional) If you want to ensure only the instructor who created it can update:
            // var owns = await _uow.Courses.Query()
            //     .AnyAsync(c => c.InstructorId == instructorId.Value 
            //                   && _uow.CourseTrackCourses.Query()
            //                      .Any(ctc => ctc.TrackId == trackId && ctc.CourseId == c.CourseId));
            // if (!owns) throw new UnauthorizedAccessException("Not allowed to modify this track.");

            // Save under wwwroot/uploads/TrackImages
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "TrackImages");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"track_{trackId}_{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            track.TrackImage = $"/uploads/TrackImages/{fileName}";
            _uow.CourseTracks.Update(track);
            await _uow.SaveAsync();
        }

        /// <summary>
        /// Updates name/description of an existing track.
        /// </summary>
        public async Task UpdateTrackAsync(UpdateTrackRequestDto dto)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var instructorId = user?.GetCurrentUserId();
            if (instructorId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            var track = await _uow.CourseTracks.Query()
                .FirstOrDefaultAsync(t => t.TrackId == dto.TrackId);

            if (track == null)
                throw new KeyNotFoundException($"Track {dto.TrackId} not found.");

            // (Optional) Verify instructor’s ownership logic here, if needed.

            if (!string.IsNullOrWhiteSpace(dto.TrackName))
                track.TrackName = dto.TrackName.Trim();

            if (!string.IsNullOrWhiteSpace(dto.TrackDescription))
                track.TrackDescription = dto.TrackDescription.Trim();

            _uow.CourseTracks.Update(track);
            await _uow.SaveAsync();
        }

        /// <summary>
        /// Deletes a track and all its associations.
        /// </summary>
        public async Task DeleteTrackAsync(int trackId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var instructorId = user?.GetCurrentUserId();
            if (instructorId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            var track = await _uow.CourseTracks.Query()
                .Include(t => t.CourseTrackCourses)
                .FirstOrDefaultAsync(t => t.TrackId == trackId);

            if (track == null)
                throw new KeyNotFoundException($"Track {trackId} not found.");

            // Remove each join‐row one by one
            if (track.CourseTrackCourses != null)
            {
                foreach (var join in track.CourseTrackCourses)
                {
                    _uow.CourseTrackCourses.Remove(join);
                }
            }

            // Finally remove the track itself
            _uow.CourseTracks.Remove(track);
            await _uow.SaveAsync();
        }


        /// <summary>
        /// Adds a single course to an existing track.
        /// Only if the course belongs to the current instructor.
        /// </summary>
        public async Task AddCourseToTrackAsync(AddCourseToTrackRequestDto dto)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var instructorId = user?.GetCurrentUserId();
            if (instructorId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            // Verify course belongs to this instructor and is not deleted
            var course = await _uow.Courses.Query()
                .FirstOrDefaultAsync(c =>
                    c.CourseId == dto.CourseId
                    && c.InstructorId == instructorId.Value
                    && !c.IsDeleted);

            if (course == null)
                throw new KeyNotFoundException($"Course {dto.CourseId} not found or not owned by you.");

            // Check existing join
            var exists = await _uow.CourseTrackCourses.Query()
                .AnyAsync(ctc => ctc.TrackId == dto.TrackId && ctc.CourseId == dto.CourseId);

            if (exists)
                throw new InvalidOperationException("Course already in this track.");

            var join = new CourseTrackCourse
            {
                TrackId = dto.TrackId,
                CourseId = dto.CourseId
            };
            await _uow.CourseTrackCourses.AddAsync(join);
            await _uow.SaveAsync();
        }

        /// <summary>
        /// Removes the specified (trackId, courseId) join entry.
        /// </summary>
        public async Task RemoveCourseFromTrackAsync(int trackId, int courseId)
        {
            var entry = await _uow.CourseTrackCourses.Query()
                .FirstOrDefaultAsync(ctc =>
                    ctc.TrackId == trackId &&
                    ctc.CourseId == courseId);

            if (entry == null)
                throw new KeyNotFoundException("Course not found in this track.");

            _uow.CourseTrackCourses.Remove(entry);
            await _uow.SaveAsync();
        }

        /// <summary>
        /// Returns all tracks that include at least one course belonging to the current instructor.
        /// </summary>
        public async Task<IEnumerable<TrackDto>> GetAllTracksAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var instructorId = user?.GetCurrentUserId();
            if (instructorId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            var tracks = await _uow.CourseTracks.Query()
                .Include(t => t.CourseTrackCourses)
                    .ThenInclude(ctc => ctc.Course)
                .ToListAsync();

            // Filter to only those tracks that contain at least one course taught by this instructor
            var relevant = tracks
                .Where(t =>
                    t.CourseTrackCourses
                     .Any(ctc => ctc.Course.InstructorId == instructorId.Value && !ctc.Course.IsDeleted))
                .ToList();

            return relevant
                .Select(t => new TrackDto
                {
                    TrackId = t.TrackId,
                    TrackName = t.TrackName,
                    TrackDescription = t.TrackDescription ?? string.Empty,
                    TrackImage = t.TrackImage,
                    CreatedAt = t.CreatedAt,
                    CourseCount = t.CourseTrackCourses
                        .Count(ctc => !ctc.Course.IsDeleted && ctc.Course.InstructorId == instructorId.Value)
                })
                .ToList();
        }

        /// <summary>
        /// Returns detailed info for a single track, including only the courses that belong to this instructor.
        /// </summary>
        public async Task<TrackDetailsDto> GetTrackDetailsAsync(int trackId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var instructorId = user?.GetCurrentUserId();
            if (instructorId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            var track = await _uow.CourseTracks.Query()
                .Include(t => t.CourseTrackCourses)
                    .ThenInclude(ctc => ctc.Course)
                .FirstOrDefaultAsync(t => t.TrackId == trackId);

            if (track == null)
                throw new KeyNotFoundException($"Track {trackId} not found.");

            var dto = new TrackDetailsDto
            {
                TrackId = track.TrackId,
                TrackName = track.TrackName,
                TrackDescription = track.TrackDescription ?? string.Empty,
                TrackImage = track.TrackImage,
                CreatedAt = track.CreatedAt
            };

            foreach (var ctc in track.CourseTrackCourses)
            {
                var course = ctc.Course;
                // Only include if instructor owns that course
                if (course.InstructorId == instructorId.Value && !course.IsDeleted)
                {
                    dto.Courses.Add(new CourseInTrackDto
                    {
                        CourseId = course.CourseId,
                        CourseName = course.CourseName,
                        CourseImage = course.CourseImage
                    });
                }
            }

            return dto;
        }
    }
}
