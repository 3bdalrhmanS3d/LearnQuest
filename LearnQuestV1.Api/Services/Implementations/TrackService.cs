using LearnQuestV1.Api.DTOs.Track;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models.CourseOrganization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class TrackService : ITrackService
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TrackService> _logger;

        public TrackService(
            IUnitOfWork uow,
            IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor,
            ILogger<TrackService> logger)
        {
            _uow = uow;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        #region Helper Methods

        private ClaimsPrincipal GetCurrentUser()
        {
            return _httpContextAccessor.HttpContext?.User
                   ?? throw new InvalidOperationException("Unable to determine current user.");
        }

        private async Task ValidateAdminAccessAsync()
        {
            var user = GetCurrentUser();
            var userId = user.GetCurrentUserId();

            if (userId == null)
                throw new InvalidOperationException("Unable to determine current user ID.");

            // Verify user is an Admin
            var isAdmin = await _uow.Users.Query()
                .AnyAsync(u => u.UserId == userId.Value &&
                              u.Role == UserRole.Admin &&
                              !u.IsDeleted);

            if (!isAdmin)
                throw new UnauthorizedAccessException("Only administrators can manage tracks.");
        }

        private async Task<bool> ValidateCourseExistsAsync(int courseId)
        {
            return await _uow.Courses.Query()
                .AnyAsync(c => c.CourseId == courseId && !c.IsDeleted);
        }

        private async Task<bool> ValidateTrackExistsAsync(int trackId)
        {
            return await _uow.CourseTracks.Query()
                .AnyAsync(t => t.TrackId == trackId);
        }

        #endregion

        /// <summary>
        /// Creates a new track. Only Admin users can create tracks.
        /// Returns the newly created TrackId.
        /// </summary>
        public async Task<int> CreateTrackAsync(CreateTrackRequestDto dto)
        {
            await ValidateAdminAccessAsync();

            _logger.LogInformation("Creating new track: {TrackName}", dto.TrackName);

            // Create the track without image first
            var track = new CourseTrack
            {
                TrackName = dto.TrackName.Trim(),
                TrackDescription = dto.TrackDescription?.Trim(),
                CreatedAt = DateTime.UtcNow,
                TrackImage = "/uploads/trackImages/default.jpg" // Default image
            };

            await _uow.CourseTracks.AddAsync(track);
            await _uow.SaveAsync(); // Save to get the TrackId

            _logger.LogInformation("Track created with ID: {TrackId}", track.TrackId);

            // Now upload image if provided
            if (dto.TrackImage != null && dto.TrackImage.Length > 0)
            {
                try
                {
                    var imageUrl = await UploadTrackImageAsync(track.TrackId, dto.TrackImage);
                    track.TrackImage = imageUrl;
                    _uow.CourseTracks.Update(track);
                    await _uow.SaveAsync();

                    _logger.LogInformation("Track image uploaded successfully for track {TrackId}", track.TrackId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to upload track image for track {TrackId}", track.TrackId);
                    // Continue with default image - don't fail the track creation
                }
            }

            // Attach any initial courses (validate they exist)
            if (dto.CourseIds != null && dto.CourseIds.Any())
            {
                var validCourseIds = new List<int>();

                foreach (var courseId in dto.CourseIds)
                {
                    if (await ValidateCourseExistsAsync(courseId))
                    {
                        validCourseIds.Add(courseId);
                    }
                    else
                    {
                        _logger.LogWarning("Course {CourseId} not found during track creation", courseId);
                    }
                }

                // Add valid courses to track
                foreach (var courseId in validCourseIds)
                {
                    // Check if course is already in track
                    var exists = await _uow.CourseTrackCourses.Query()
                        .AnyAsync(ctc => ctc.TrackId == track.TrackId && ctc.CourseId == courseId);

                    if (!exists)
                    {
                        var join = new CourseTrackCourse
                        {
                            TrackId = track.TrackId,
                            CourseId = courseId
                        };
                        await _uow.CourseTrackCourses.AddAsync(join);
                    }
                }

                await _uow.SaveAsync();
                _logger.LogInformation("Added {Count} courses to track {TrackId}", validCourseIds.Count, track.TrackId);
            }

            return track.TrackId;
        }

        /// <summary>
        /// Uploads a new image for the specified track. Only Admin users can upload track images.
        /// Stores under wwwroot/uploads/trackImages.
        /// </summary>
        public async Task<string> UploadTrackImageAsync(int trackId, IFormFile file)
        {
            await ValidateAdminAccessAsync();

            if (file == null || file.Length == 0)
                throw new InvalidDataException("No file uploaded.");

            var track = await _uow.CourseTracks.Query()
                .FirstOrDefaultAsync(t => t.TrackId == trackId)
                ?? throw new KeyNotFoundException($"Track {trackId} not found.");

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
                throw new InvalidDataException("Invalid file type. Only JPG, JPEG, PNG, GIF, and WebP are allowed.");

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                throw new InvalidDataException("File size cannot exceed 5MB.");

            // Create uploads directory
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "trackImages");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Generate unique filename
            var fileName = $"track{trackId}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Delete old image if exists and it's not the default image
            if (!string.IsNullOrWhiteSpace(track.TrackImage) &&
                track.TrackImage != "/uploads/trackImages/default.jpg")
            {
                var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                    track.TrackImage.TrimStart('/'));

                if (File.Exists(oldImagePath))
                {
                    try
                    {
                        File.Delete(oldImagePath);
                        _logger.LogInformation("Deleted old track image: {ImagePath}", oldImagePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete old track image: {ImagePath}", oldImagePath);
                    }
                }
            }

            // Save new image
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Update track image URL
            var relativeUrl = $"/uploads/trackImages/{fileName}";
            track.TrackImage = relativeUrl;
            _uow.CourseTracks.Update(track);
            await _uow.SaveAsync();

            _logger.LogInformation("Track image uploaded successfully: {TrackId} -> {ImageUrl}", trackId, relativeUrl);

            return relativeUrl;
        }

        /// <summary>
        /// Updates name/description of an existing track. Only Admin users can update tracks.
        /// </summary>
        public async Task UpdateTrackAsync(UpdateTrackRequestDto dto)
        {
            await ValidateAdminAccessAsync();

            var track = await _uow.CourseTracks.Query()
                .FirstOrDefaultAsync(t => t.TrackId == dto.TrackId)
                ?? throw new KeyNotFoundException($"Track {dto.TrackId} not found.");

            _logger.LogInformation("Updating track {TrackId}", dto.TrackId);

            var hasChanges = false;

            if (!string.IsNullOrWhiteSpace(dto.TrackName) &&
                dto.TrackName.Trim() != track.TrackName)
            {
                track.TrackName = dto.TrackName.Trim();
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(dto.TrackDescription) &&
                dto.TrackDescription.Trim() != track.TrackDescription)
            {
                track.TrackDescription = dto.TrackDescription.Trim();
                hasChanges = true;
            }

            if (hasChanges)
            {
                _uow.CourseTracks.Update(track);
                await _uow.SaveAsync();
                _logger.LogInformation("Track {TrackId} updated successfully", dto.TrackId);
            }
            else
            {
                _logger.LogInformation("No changes detected for track {TrackId}", dto.TrackId);
            }
        }

        /// <summary>
        /// Deletes a track and all its associations. Only Admin users can delete tracks.
        /// </summary>
        public async Task DeleteTrackAsync(int trackId)
        {
            await ValidateAdminAccessAsync();

            var track = await _uow.CourseTracks.Query()
                .Include(t => t.CourseTrackCourses)
                .FirstOrDefaultAsync(t => t.TrackId == trackId)
                ?? throw new KeyNotFoundException($"Track {trackId} not found.");

            _logger.LogInformation("Deleting track {TrackId}", trackId);

            // Remove all course associations first
            if (track.CourseTrackCourses != null && track.CourseTrackCourses.Any())
            {
                var associationCount = track.CourseTrackCourses.Count;
                foreach (var join in track.CourseTrackCourses.ToList())
                {
                    _uow.CourseTrackCourses.Remove(join);
                }
                _logger.LogInformation("Removed {Count} course associations from track {TrackId}",
                    associationCount, trackId);
            }

            // Delete track image if it exists and it's not the default image
            if (!string.IsNullOrWhiteSpace(track.TrackImage) &&
                track.TrackImage != "/uploads/trackImages/default.jpg")
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                    track.TrackImage.TrimStart('/'));

                if (File.Exists(imagePath))
                {
                    try
                    {
                        File.Delete(imagePath);
                        _logger.LogInformation("Deleted track image: {ImagePath}", imagePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete track image: {ImagePath}", imagePath);
                    }
                }
            }

            // Finally remove the track itself
            _uow.CourseTracks.Remove(track);
            await _uow.SaveAsync();

            _logger.LogInformation("Track {TrackId} deleted successfully", trackId);
        }

        /// <summary>
        /// Adds a single course to an existing track. Only Admin users can manage track courses.
        /// </summary>
        public async Task AddCourseToTrackAsync(AddCourseToTrackRequestDto dto)
        {
            await ValidateAdminAccessAsync();

            // Validate track exists
            if (!await ValidateTrackExistsAsync(dto.TrackId))
                throw new KeyNotFoundException($"Track {dto.TrackId} not found.");

            // Validate course exists and is not deleted
            if (!await ValidateCourseExistsAsync(dto.CourseId))
                throw new KeyNotFoundException($"Course {dto.CourseId} not found or is deleted.");

            // Check if course is already in track
            var exists = await _uow.CourseTrackCourses.Query()
                .AnyAsync(ctc => ctc.TrackId == dto.TrackId && ctc.CourseId == dto.CourseId);

            if (exists)
                throw new InvalidOperationException("Course is already in this track.");

            var join = new CourseTrackCourse
            {
                TrackId = dto.TrackId,
                CourseId = dto.CourseId
            };

            await _uow.CourseTrackCourses.AddAsync(join);
            await _uow.SaveAsync();

            _logger.LogInformation("Added course {CourseId} to track {TrackId}", dto.CourseId, dto.TrackId);
        }

        /// <summary>
        /// Removes the specified course from track. Only Admin users can manage track courses.
        /// </summary>
        public async Task RemoveCourseFromTrackAsync(int trackId, int courseId)
        {
            await ValidateAdminAccessAsync();

            var entry = await _uow.CourseTrackCourses.Query()
                .FirstOrDefaultAsync(ctc => ctc.TrackId == trackId && ctc.CourseId == courseId);

            if (entry == null)
                throw new KeyNotFoundException("Course not found in this track.");

            _uow.CourseTrackCourses.Remove(entry);
            await _uow.SaveAsync();

            _logger.LogInformation("Removed course {CourseId} from track {TrackId}", courseId, trackId);
        }

        /// <summary>
        /// Returns all tracks in the system. Admin users can see all tracks.
        /// </summary>
        public async Task<IEnumerable<TrackDto>> GetAllTracksAsync()
        {
            await ValidateAdminAccessAsync();

            _logger.LogInformation("Retrieving all tracks for admin");

            var tracks = await _uow.CourseTracks.Query()
                .Include(t => t.CourseTrackCourses)
                    .ThenInclude(ctc => ctc.Course)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var trackDtos = tracks.Select(t => new TrackDto
            {
                TrackId = t.TrackId,
                TrackName = t.TrackName,
                TrackDescription = t.TrackDescription ?? string.Empty,
                TrackImage = t.TrackImage,
                CreatedAt = t.CreatedAt,
                CourseCount = t.CourseTrackCourses.Count(ctc => !ctc.Course.IsDeleted)
            }).ToList();

            _logger.LogInformation("Retrieved {Count} tracks", trackDtos.Count);

            return trackDtos;
        }

        /// <summary>
        /// Returns detailed info for a single track, including all courses. Admin users can see all details.
        /// </summary>
        public async Task<TrackDetailsDto> GetTrackDetailsAsync(int trackId)
        {
            await ValidateAdminAccessAsync();

            var track = await _uow.CourseTracks.Query()
                .Include(t => t.CourseTrackCourses)
                    .ThenInclude(ctc => ctc.Course)
                        .ThenInclude(c => c.Instructor)
                .FirstOrDefaultAsync(t => t.TrackId == trackId)
                ?? throw new KeyNotFoundException($"Track {trackId} not found.");

            _logger.LogInformation("Retrieving details for track {TrackId}", trackId);

            var dto = new TrackDetailsDto
            {
                TrackId = track.TrackId,
                TrackName = track.TrackName,
                TrackDescription = track.TrackDescription ?? string.Empty,
                TrackImage = track.TrackImage,
                CreatedAt = track.CreatedAt,
                Courses = new List<CourseInTrackDto>()
            };

            // Add all courses (admin can see all courses in track)
            foreach (var ctc in track.CourseTrackCourses.Where(ctc => !ctc.Course.IsDeleted))
            {
                var course = ctc.Course;
                dto.Courses.Add(new CourseInTrackDto
                {
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    CourseImage = course.CourseImage,
                    InstructorName = course.Instructor?.FullName ?? "Unknown Instructor",
                    InstructorId = course.InstructorId,
                    IsActive = course.IsActive,
                    CreatedAt = course.CreatedAt
                });
            }

            _logger.LogInformation("Retrieved track {TrackId} with {CourseCount} courses",
                trackId, dto.Courses.Count);

            return dto;
        }
    }
}