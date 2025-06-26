using LearnQuestV1.Api.DTOs.Payments;
using LearnQuestV1.Api.DTOs.Profile;
using LearnQuestV1.Api.DTOs.Progress;
using LearnQuestV1.Api.DTOs.Users.Request;
using LearnQuestV1.Api.DTOs.Users.Response;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models.CourseOrganization;
using LearnQuestV1.Core.Models.Financial;
using LearnQuestV1.Core.Models.LearningAndProgress;
using LearnQuestV1.Core.Models.UserManagement;
using Microsoft.EntityFrameworkCore;
using static LearnQuestV1.Api.DTOs.Profile.UserActivityDto;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<UserService> _logger;
        private readonly IPointsService _pointsService;

        public UserService(IUnitOfWork uow, IWebHostEnvironment env, ILogger<UserService> logger, IPointsService pointsService)
        {
            _uow = uow;
            _env = env;
            _logger = logger;
            _pointsService = pointsService;
        }

        // =====================================================
        // 1) User Profile: fetch and update
        // =====================================================

        /// <summary>
        /// Retrieves a user's profile, including personal details and progress.
        /// Throws KeyNotFoundException if the user does not exist.
        /// Throws InvalidOperationException if the user's details record is missing.
        /// </summary>
        public async Task<UserProfileDto> GetUserProfileAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Retrieving profile for user {UserId}", userId);

                // Include UserDetail and UserProgresses → Course for each progress
                var user = await _uow.Users.Query()
                    .Include(u => u.UserDetail)
                    .Include(u => u.UserProgresses)
                        .ThenInclude(up => up.Course)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", userId);
                    throw new KeyNotFoundException($"User with ID {userId} not found.");
                }

                bool hasDetail = user.UserDetail != null;
                // إذا لا توجد تفاصيل، لن ننشئ سجل في هذه المرحلة – فقط نُظهر العلم Required
                var required = hasDetail ? null : new Dictionary<string, string>
                {
                    { "BirthDate",     "YYYY-MM-DD required" },
                    { "EducationLevel","Required" },
                    { "Nationality",   "Required" }
                };

                var progressDtos = user.UserProgresses.Select(up => new UserProgressDto
                {
                    CourseId = up.CourseId,
                    CourseName = up.Course?.CourseName ?? string.Empty,
                    CurrentLevelId = up.CurrentLevelId,
                    CurrentSectionId = up.CurrentSectionId,
                    LastUpdated = up.LastUpdated
                }).ToList();

                var profile = new UserProfileDto
                {
                    FullName = user.FullName,
                    EmailAddress = user.EmailAddress,
                    Role = user.Role.ToString(),
                    ProfilePhoto = user.ProfilePhoto ?? "~\\profile-pictures\\default.png",
                    IsProfileComplete = hasDetail,
                    RequiredFields = required,
                    CreatedAt = user.CreatedAt,
                    BirthDate = user.UserDetail?.BirthDate ?? default,
                    Edu = user.UserDetail?.EducationLevel,
                    National = user.UserDetail?.Nationality,
                    Progress = progressDtos
                };

                _logger.LogInformation("Profile retrieved successfully for user {UserId}", userId);
                return profile;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException || ex is InvalidOperationException))
            {
                _logger.LogError(ex, "Error retrieving profile for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Creates or updates a user's personal details.
        /// Throws KeyNotFoundException if the user does not exist.
        /// </summary>
        public async Task UpdateUserProfileAsync(int userId, UserProfileUpdateDto dto)
        {
            try
            {
                _logger.LogInformation("Updating profile for user {UserId}", userId);

                var user = await _uow.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found for profile update", userId);
                    throw new KeyNotFoundException($"User with ID {userId} not found.");
                }

                var detail = (await _uow.UserDetails.FindAsync(d => d.UserId == userId))
                             .FirstOrDefault();

                if (detail == null)
                {
                    detail = new UserDetail
                    {
                        UserId = userId,
                        BirthDate = dto.BirthDate,
                        EducationLevel = dto.Edu,
                        Nationality = dto.National,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _uow.UserDetails.AddAsync(detail);
                    _logger.LogInformation("Created new UserDetail for user {UserId}", userId);
                }
                else
                {
                    detail.BirthDate = dto.BirthDate;
                    detail.EducationLevel = dto.Edu;
                    detail.Nationality = dto.National;
                    _uow.UserDetails.Update(detail);
                    _logger.LogInformation("Updated existing UserDetail for user {UserId}", userId);
                }

                await _uow.SaveAsync();
                _logger.LogInformation("Profile update completed for user {UserId}", userId);
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
                throw;
            }
        }

        // =====================================================
        // 2) Payments and Enrollment
        // =====================================================

        /// <summary>
        /// Records a pending payment for a course.
        /// Throws KeyNotFoundException if the course does not exist.
        /// Throws InvalidOperationException if the user has already completed payment for that course.
        /// </summary>
        public async Task RegisterPaymentAsync(int userId, PaymentRequestDto dto)
        {
            try
            {
                _logger.LogInformation("Registering payment for user {UserId}, course {CourseId}", userId, dto.CourseId);

                var course = await _uow.Courses.GetByIdAsync(dto.CourseId);
                if (course == null)
                {
                    _logger.LogWarning("Course with ID {CourseId} not found for payment", dto.CourseId);
                    throw new KeyNotFoundException($"Course with ID {dto.CourseId} not found.");
                }

                var existingPayment = (await _uow.Payments.FindAsync(p =>
                    p.UserId == userId &&
                    p.CourseId == dto.CourseId &&
                    p.Status == PaymentStatus.Completed))
                    .Any();

                if (existingPayment)
                {
                    _logger.LogWarning("User {UserId} already paid for course {CourseId}", userId, dto.CourseId);
                    throw new InvalidOperationException("You have already paid for this course.");
                }

                var payment = new Payment
                {
                    UserId = userId,
                    CourseId = dto.CourseId,
                    Amount = dto.Amount,
                    PaymentDate = DateTime.UtcNow,
                    Status = PaymentStatus.Pending,
                    TransactionId = dto.TransactionId
                };

                await _uow.Payments.AddAsync(payment);
                await _uow.SaveAsync();

                _logger.LogInformation("Payment registered successfully for user {UserId}, course {CourseId}", userId, dto.CourseId);
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException || ex is InvalidOperationException))
            {
                _logger.LogError(ex, "Error registering payment for user {UserId}, course {CourseId}", userId, dto.CourseId);
                throw;
            }
        }

        /// <summary>
        /// Confirms a pending payment and enrolls the user in the course.
        /// Throws KeyNotFoundException if the payment record does not exist.
        /// Throws InvalidOperationException if it's already completed.
        /// </summary>
        public async Task ConfirmPaymentAsync(int paymentId)
        {
            try
            {
                _logger.LogInformation("Confirming payment {PaymentId}", paymentId);

                var payment = await _uow.Payments.GetByIdAsync(paymentId);
                if (payment == null)
                {
                    _logger.LogWarning("Payment with ID {PaymentId} not found", paymentId);
                    throw new KeyNotFoundException($"Payment with ID {paymentId} not found.");
                }

                if (payment.Status == PaymentStatus.Completed)
                {
                    _logger.LogWarning("Payment {PaymentId} already completed", paymentId);
                    throw new InvalidOperationException("Payment already completed.");
                }

                payment.Status = PaymentStatus.Completed;
                _uow.Payments.Update(payment);

                // Check if enrollment already exists
                var existingEnrollment = await _uow.CourseEnrollments.Query()
                    .AnyAsync(e => e.UserId == payment.UserId && e.CourseId == payment.CourseId);

                if (!existingEnrollment)
                {
                    var enrollment = new CourseEnrollment
                    {
                        UserId = payment.UserId,
                        CourseId = payment.CourseId,
                        EnrolledAt = DateTime.UtcNow
                    };
                    await _uow.CourseEnrollments.AddAsync(enrollment);
                }

                await _uow.SaveAsync();
                _logger.LogInformation("Payment {PaymentId} confirmed and enrollment created", paymentId);
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException || ex is InvalidOperationException))
            {
                _logger.LogError(ex, "Error confirming payment {PaymentId}", paymentId);
                throw;
            }
        }

        // =====================================================
        // 3) User's Courses & Favorites
        // =====================================================

        /// <summary>
        /// Retrieves all courses the user has paid for and is enrolled in.
        /// Returns an empty list if none.
        /// </summary>
        public async Task<IEnumerable<MyCourseDto>> GetMyCoursesAsync(int userId, bool onlyCompleted = false)
        {
            var enrollments = await _uow.CourseEnrollments.Query()
                .Include(e => e.Course)
                .Where(e => e.UserId == userId)
                .ToListAsync();

            var result = new List<MyCourseDto>();

            foreach (var e in enrollments)
            {
                // تحقق من الدفع
                bool isPaid = await _uow.Payments.Query()
                    .AnyAsync(p =>
                        p.UserId == userId &&
                        p.CourseId == e.CourseId &&
                        p.Status == PaymentStatus.Completed);
                if (!isPaid) continue;

                // إجمالي عدد الـ Contents
                var cid = e.CourseId;
                var totalContents = await _uow.Contents.Query()
                    .Where(ct => ct.Section.Level.CourseId == cid && !ct.IsDeleted && ct.IsVisible)
                    .CountAsync();

                // عدد الـ Contents المنجزة
                var completedContents = await _uow.UserContentActivities.Query()
                    .Where(uca =>
                        uca.UserId == userId &&
                        uca.IsCompleted &&
                        uca.Content.Section.Level.CourseId == cid)
                    .Select(uca => uca.ContentId)
                    .Distinct()
                    .CountAsync();

                int progress = totalContents == 0
                    ? 0
                    : (int)(completedContents * 100.0 / totalContents);

                // فلترة الكورسات المكتملة إذا طُلب ذلك
                if (onlyCompleted && progress < 100)
                    continue;

                result.Add(new MyCourseDto
                {
                    CourseId = cid,
                    CourseName = e.Course.CourseName,
                    Description = e.Course.Description,
                    EnrolledAt = e.EnrolledAt,
                    ProgressPercentage = progress
                });
            }

            return result;
        }


        /// <summary>
        /// Retrieves all favorite courses for the user.
        /// Returns an empty list if none.
        /// </summary>
        public async Task<IEnumerable<CourseDto>> GetFavoriteCoursesAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Retrieving favorite courses for user {UserId}", userId);

                var favorites = await _uow.FavoriteCourses.Query()
                    .Include(f => f.Course)
                    .Where(f => f.UserId == userId)
                    .ToListAsync();

                var list = favorites
                    .Where(f => f.Course != null)
                    .Select(f => new CourseDto
                    {
                        CourseId = f.Course.CourseId,
                        CourseName = f.Course.CourseName,
                        Description = f.Course.Description,
                        CourseImage = f.Course.CourseImage ?? string.Empty,
                        CoursePrice = f.Course.CoursePrice
                    })
                    .ToList();

                _logger.LogInformation("Retrieved {FavoriteCount} favorite courses for user {UserId}", list.Count, userId);
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving favorite courses for user {UserId}", userId);
                throw;
            }
        }

        // =====================================================
        // 4) Upload / Delete Profile Photo
        // =====================================================

        /// <summary>
        /// Uploads a new profile photo for the user.
        /// Throws KeyNotFoundException if the user does not exist.
        /// Throws InvalidOperationException if no file is provided.
        /// </summary>
        public async Task UploadProfilePhotoAsync(int userId, IFormFile file)
        {
            try
            {
                _logger.LogInformation("Uploading profile photo for user {UserId}", userId);

                var user = await _uow.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found for photo upload", userId);
                    throw new KeyNotFoundException($"User with ID {userId} not found.");
                }

                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("No file provided for photo upload by user {UserId}", userId);
                    throw new InvalidOperationException("No file uploaded.");
                }

                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/profile-pictures");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                    _logger.LogInformation("Created uploads directory: {Directory}", uploadsFolder);
                }

                // Delete old photo if exists
                if (!string.IsNullOrEmpty(user.ProfilePhoto) && !user.ProfilePhoto.EndsWith("default.png"))
                {
                    var oldFilePath = Path.Combine(_env.WebRootPath, user.ProfilePhoto.TrimStart('/'));
                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                        _logger.LogInformation("Deleted old profile photo for user {UserId}", userId);
                    }
                }

                var ext = Path.GetExtension(file.FileName);
                var fileName = $"user_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                user.ProfilePhoto = $"/uploads/profile-pictures/{fileName}";
                _uow.Users.Update(user);
                await _uow.SaveAsync();

                _logger.LogInformation("Profile photo uploaded successfully for user {UserId}", userId);
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException || ex is InvalidOperationException))
            {
                _logger.LogError(ex, "Error uploading profile photo for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Deletes the user's custom profile photo and reverts to default.
        /// Throws KeyNotFoundException if the user does not exist.
        /// Throws InvalidOperationException if no custom photo to delete.
        /// </summary>
        public async Task DeleteProfilePhotoAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Deleting profile photo for user {UserId}", userId);

                var user = await _uow.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found for photo deletion", userId);
                    throw new KeyNotFoundException($"User with ID {userId} not found.");
                }

                if (string.IsNullOrEmpty(user.ProfilePhoto) || user.ProfilePhoto.EndsWith("default.png"))
                {
                    _logger.LogWarning("No custom profile photo to delete for user {UserId}", userId);
                    throw new InvalidOperationException("No custom profile photo to delete.");
                }

                var filePath = Path.Combine(_env.WebRootPath, user.ProfilePhoto.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Profile photo file deleted for user {UserId}", userId);
                }

                user.ProfilePhoto = "/uploads/profile-pictures/default.png";
                _uow.Users.Update(user);
                await _uow.SaveAsync();

                _logger.LogInformation("Profile photo deleted and reset to default for user {UserId}", userId);
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException || ex is InvalidOperationException))
            {
                _logger.LogError(ex, "Error deleting profile photo for user {UserId}", userId);
                throw;
            }
        }

        // =====================================================
        // 5) Course Tracks and Their Courses
        // =====================================================

        /// <summary>
        /// Retrieves all course tracks with the count of active, non-deleted courses in each.
        /// Returns an empty list if no tracks exist.
        /// </summary>
        public async Task<IEnumerable<TrackDto>> GetAllTracksAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all course tracks");

                var tracks = await _uow.CourseTracks.Query()
                    .Include(t => t.CourseTrackCourses)
                        .ThenInclude(ctc => ctc.Course)
                    .ToListAsync();

                if (!tracks.Any())
                {
                    _logger.LogInformation("No course tracks found");
                    return Array.Empty<TrackDto>();
                }

                var result = tracks.Select(t => new TrackDto
                {
                    TrackId = t.TrackId,
                    TrackName = t.TrackName,
                    TrackDescription = t.TrackDescription ?? string.Empty,
                    CourseCount = t.CourseTrackCourses.Count(ctc =>
                        !ctc.Course.IsDeleted && ctc.Course.IsActive)
                }).ToList();

                _logger.LogInformation("Retrieved {TrackCount} course tracks", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course tracks");
                throw;
            }
        }

        /// <summary>
        /// Retrieves all courses in a given track.
        /// Throws KeyNotFoundException if the track does not exist.
        /// </summary>
        public async Task<TrackCoursesDto> GetCoursesInTrackAsync(int trackId)
        {
            try
            {
                _logger.LogInformation("Retrieving courses for track {TrackId}", trackId);

                var track = await _uow.CourseTracks.Query()
                    .Include(t => t.CourseTrackCourses)
                        .ThenInclude(ctc => ctc.Course)
                            .ThenInclude(c => c.Instructor)
                    .Include(t => t.CourseTrackCourses)
                        .ThenInclude(ctc => ctc.Course)
                            .ThenInclude(c => c.Levels)
                    .FirstOrDefaultAsync(t => t.TrackId == trackId);

                if (track == null)
                {
                    _logger.LogWarning("Track with ID {TrackId} not found", trackId);
                    throw new KeyNotFoundException($"Track with ID {trackId} not found.");
                }

                var courses = track.CourseTrackCourses
                    .Where(ctc => !ctc.Course.IsDeleted && ctc.Course.IsActive)
                    .Select(ctc => new CourseInTrackDto
                    {
                        CourseId = ctc.Course.CourseId,
                        CourseName = ctc.Course.CourseName,
                        Description = ctc.Course.Description,
                        CourseImage = ctc.Course.CourseImage ?? string.Empty,
                        CoursePrice = ctc.Course.CoursePrice,
                        CreatedAt = ctc.Course.CreatedAt,
                        InstructorName = ctc.Course.Instructor.FullName,
                        LevelsCount = ctc.Course.Levels?.Count ?? 0
                    })
                    .ToList();

                var result = new TrackCoursesDto
                {
                    TrackId = track.TrackId,
                    TrackName = track.TrackName,
                    TrackDescription = track.TrackDescription ?? string.Empty,
                    TotalCourses = courses.Count,
                    Courses = courses
                };

                _logger.LogInformation("Retrieved {CourseCount} courses for track {TrackId}", courses.Count, trackId);
                return result;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error retrieving courses for track {TrackId}", trackId);
                throw;
            }
        }

        // =====================================================
        // 6) Course Search
        // =====================================================

        /// <summary>
        /// Searches active, non-deleted courses by name or description.
        /// Returns an empty list if no matches.
        /// </summary>
        public async Task<IEnumerable<CourseDto>> SearchCoursesAsync(string? search)
        {
            try
            {
                _logger.LogInformation("Searching courses with query: {SearchQuery}", search ?? "empty");

                var query = _uow.Courses.Query()
                    .Where(c => !c.IsDeleted && c.IsActive);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var lower = search.ToLower();
                    query = query.Where(c =>
                        c.CourseName.ToLower().Contains(lower) ||
                        c.Description.ToLower().Contains(lower));
                }

                var list = await query.ToListAsync();
                if (!list.Any())
                {
                    _logger.LogInformation("No courses found for search query: {SearchQuery}", search ?? "empty");
                    return Array.Empty<CourseDto>();
                }

                var result = list.Select(c => new CourseDto
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    Description = c.Description,
                    CourseImage = c.CourseImage ?? string.Empty,
                    CoursePrice = c.CoursePrice
                }).ToList();

                _logger.LogInformation("Found {CourseCount} courses for search query: {SearchQuery}", result.Count, search ?? "empty");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching courses with query: {SearchQuery}", search ?? "empty");
                throw;
            }
        }

        // =====================================================
        // 7) Levels, Sections, and Contents
        // =====================================================

        /// <summary>
        /// Returns all visible levels for a given course, if the user is enrolled.
        /// Throws InvalidOperationException if not enrolled.
        /// Throws KeyNotFoundException if the course does not exist.
        /// Returns an empty levels list if none visible.
        /// </summary>
        public async Task<LevelsResponseDto> GetCourseLevelsAsync(int userId, int courseId)
        {
            try
            {
                _logger.LogInformation("Retrieving levels for course {CourseId} for user {UserId}", courseId, userId);

                var enrolled = await _uow.CourseEnrollments.Query()
                    .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
                if (!enrolled)
                {
                    _logger.LogWarning("User {UserId} not enrolled in course {CourseId}", userId, courseId);
                    throw new InvalidOperationException("User is not enrolled in this course.");
                }

                var course = await _uow.Courses.Query()
                    .Include(c => c.Levels.Where(l => !l.IsDeleted && l.IsVisible)
                        .OrderBy(l => l.LevelOrder))
                    .FirstOrDefaultAsync(c => c.CourseId == courseId && !c.IsDeleted);

                if (course == null)
                {
                    _logger.LogWarning("Course with ID {CourseId} not found", courseId);
                    throw new KeyNotFoundException($"Course with ID {courseId} not found.");
                }

                var levels = course.Levels.Select(l => new LevelDto
                {
                    LevelId = l.LevelId,
                    LevelName = l.LevelName,
                    LevelDetails = l.LevelDetails,
                    LevelOrder = l.LevelOrder,
                    IsVisible = l.IsVisible
                }).ToList();

                var result = new LevelsResponseDto
                {
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    Description = course.Description,
                    CourseImage = course.CourseImage ?? string.Empty,
                    LevelsCount = levels.Count,
                    Levels = levels
                };

                _logger.LogInformation("Retrieved {LevelCount} levels for course {CourseId}", levels.Count, courseId);
                return result;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException || ex is InvalidOperationException))
            {
                _logger.LogError(ex, "Error retrieving levels for course {CourseId} for user {UserId}", courseId, userId);
                throw;
            }
        }

        /// <summary>
        /// Returns all visible sections for a given level, if the user is enrolled.
        /// Throws InvalidOperationException if not enrolled.
        /// Throws KeyNotFoundException if the level does not exist.
        /// Returns an empty sections list if none visible.
        /// </summary>
        public async Task<SectionsResponseDto> GetLevelSectionsAsync(int userId, int levelId)
        {
            try
            {
                _logger.LogInformation("Retrieving sections for level {LevelId} for user {UserId}", levelId, userId);

                var level = await _uow.Levels.Query()
                    .Include(l => l.Sections.OrderBy(s => s.SectionOrder))
                    .Include(l => l.Course)
                    .FirstOrDefaultAsync(l => l.LevelId == levelId && !l.IsDeleted);

                if (level == null)
                {
                    _logger.LogWarning("Level with ID {LevelId} not found", levelId);
                    throw new KeyNotFoundException($"Level with ID {levelId} not found.");
                }

                var enrolled = await _uow.CourseEnrollments.Query()
                    .AnyAsync(e => e.UserId == userId && e.CourseId == level.CourseId);
                if (!enrolled)
                {
                    _logger.LogWarning("User {UserId} not enrolled in course for level {LevelId}", userId, levelId);
                    throw new InvalidOperationException("User is not enrolled in this course.");
                }

                var userProgress = (await _uow.UserProgresses.FindAsync(p =>
                        p.UserId == userId && p.CourseId == level.CourseId))
                    .FirstOrDefault();

                var sections = level.Sections
                    .Where(s => !s.IsDeleted && s.IsVisible)
                    .Select(s => new SectionDto
                    {
                        SectionId = s.SectionId,
                        SectionName = s.SectionName,
                        SectionOrder = s.SectionOrder,
                        IsCurrent = userProgress != null && userProgress.CurrentSectionId == s.SectionId,
                        IsCompleted = userProgress != null &&
                                      s.SectionOrder <
                                      level.Sections.First(sec => sec.SectionId == userProgress.CurrentSectionId).SectionOrder
                    })
                    .ToList();

                var result = new SectionsResponseDto
                {
                    LevelId = level.LevelId,
                    LevelName = level.LevelName,
                    LevelDetails = level.LevelDetails,
                    Sections = sections
                };

                _logger.LogInformation("Retrieved {SectionCount} sections for level {LevelId}", sections.Count, levelId);
                return result;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException || ex is InvalidOperationException))
            {
                _logger.LogError(ex, "Error retrieving sections for level {LevelId} for user {UserId}", levelId, userId);
                throw;
            }
        }

        /// <summary>
        /// Returns all visible contents for a given section, if the user is enrolled.
        /// Throws InvalidOperationException if not enrolled.
        /// Throws KeyNotFoundException if the section does not exist.
        /// Returns an empty contents list if none visible.
        /// </summary>
        public async Task<ContentsResponseDto> GetSectionContentsAsync(int userId, int sectionId)
        {
            try
            {
                _logger.LogInformation("Retrieving contents for section {SectionId} for user {UserId}", sectionId, userId);

                var section = await _uow.Sections.Query()
                    .Include(s => s.Level)
                        .ThenInclude(l => l.Course)
                    .Include(s => s.Contents.Where(c => c.IsVisible).OrderBy(c => c.ContentOrder))
                    .FirstOrDefaultAsync(s => s.SectionId == sectionId && !s.IsDeleted && s.IsVisible);

                if (section == null)
                {
                    _logger.LogWarning("Section with ID {SectionId} not found", sectionId);
                    throw new KeyNotFoundException($"Section with ID {sectionId} not found.");
                }

                var enrolled = await _uow.CourseEnrollments.Query()
                    .AnyAsync(e => e.UserId == userId && e.CourseId == section.Level.CourseId);
                if (!enrolled)
                {
                    _logger.LogWarning("User {UserId} not enrolled in course for section {SectionId}", userId, sectionId);
                    throw new InvalidOperationException("User is not enrolled in this course.");
                }

                var contents = section.Contents.Select(c => new ContentDto
                {
                    ContentId = c.ContentId,
                    Title = c.Title,
                    ContentType = c.ContentType.ToString(),
                    ContentText = c.ContentText ?? string.Empty,
                    ContentDoc = c.ContentDoc ?? string.Empty,
                    ContentUrl = c.ContentUrl ?? string.Empty,
                    DurationInMinutes = c.DurationInMinutes,
                    ContentDescription = c.ContentDescription ?? string.Empty
                }).ToList();

                var result = new ContentsResponseDto
                {
                    SectionId = section.SectionId,
                    SectionName = section.SectionName,
                    Contents = contents
                };

                _logger.LogInformation("Retrieved {ContentCount} contents for section {SectionId}", contents.Count, sectionId);
                return result;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException || ex is InvalidOperationException))
            {
                _logger.LogError(ex, "Error retrieving contents for section {SectionId} for user {UserId}", sectionId, userId);
                throw;
            }
        }

        // =====================================================
        // 8) Track Content Time
        // =====================================================

        /// <summary>
        /// Marks the start time for a user's content activity.
        /// Throws InvalidOperationException if already started and not ended.
        /// </summary>
        public async Task StartContentAsync(int userId, int contentId)
        {
            try
            {
                _logger.LogInformation("Starting content activity for user {UserId}, content {ContentId}", userId, contentId);

                var exists = (await _uow.UserContentActivities.FindAsync(a =>
                    a.UserId == userId && a.ContentId == contentId && a.EndTime == null))
                    .FirstOrDefault();

                if (exists != null)
                {
                    _logger.LogWarning("Content session already active for user {UserId}, content {ContentId}", userId, contentId);
                    throw new InvalidOperationException("Content session already started and not yet ended.");
                }

                var activity = new UserContentActivity
                {
                    UserId = userId,
                    ContentId = contentId,
                    StartTime = DateTime.UtcNow
                };

                await _uow.UserContentActivities.AddAsync(activity);
                await _uow.SaveAsync();

                _logger.LogInformation("Content activity started for user {UserId}, content {ContentId}", userId, contentId);
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                _logger.LogError(ex, "Error starting content activity for user {UserId}, content {ContentId}", userId, contentId);
                throw;
            }
        }

        /// <summary>
        /// Marks the end time for a user's content activity.
        /// Throws KeyNotFoundException if no active session is found.
        /// </summary>
        public async Task EndContentAsync(int userId, int contentId)
        {
            try
            {
                _logger.LogInformation("Ending content activity for user {UserId}, content {ContentId}", userId, contentId);

                var activity = (await _uow.UserContentActivities.FindAsync(a =>
                    a.UserId == userId && a.ContentId == contentId && a.EndTime == null))
                    .FirstOrDefault();

                if (activity == null)
                {
                    _logger.LogWarning("No active content session found for user {UserId}, content {ContentId}", userId, contentId);
                    throw new KeyNotFoundException("No active content session found to end.");
                }

                activity.EndTime = DateTime.UtcNow;
                _uow.UserContentActivities.Update(activity);
                await _uow.SaveAsync();

                _logger.LogInformation("Content activity ended for user {UserId}, content {ContentId}", userId, contentId);
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error ending content activity for user {UserId}, content {ContentId}", userId, contentId);
                throw;
            }
        }

        // =====================================================
        // 9) Complete Section & Get Next Section
        // =====================================================

        /// <summary>
        /// Marks a section as complete and advances to the next section (if any) in the same level.
        /// If it was the last section in that level, it stays on current section.
        /// Throws KeyNotFoundException if the current section does not exist.
        /// </summary>
        public async Task<CompleteSectionResultDto> CompleteSectionAsync(int userId, int currentSectionId)
        {
            try
            {
                _logger.LogInformation("Completing section {SectionId} for user {UserId}", currentSectionId, userId);

                var current = await _uow.Sections.Query()
                    .Include(s => s.Level)
                    .FirstOrDefaultAsync(s => s.SectionId == currentSectionId);

                if (current == null)
                {
                    _logger.LogWarning("Section with ID {SectionId} not found", currentSectionId);
                    throw new KeyNotFoundException($"Section with ID {currentSectionId} not found.");
                }

                var allSections = await _uow.Sections.Query()
                    .Where(s => s.LevelId == current.LevelId)
                    .OrderBy(s => s.SectionOrder)
                    .ToListAsync();

                var idx = allSections.FindIndex(s => s.SectionId == currentSectionId);
                var next = idx + 1 < allSections.Count ? allSections[idx + 1] : null;

                var progress = (await _uow.UserProgresses.FindAsync(p =>
                    p.UserId == userId && p.CourseId == current.Level.CourseId))
                    .FirstOrDefault();

                if (progress == null)
                {
                    progress = new UserProgress
                    {
                        UserId = userId,
                        CourseId = current.Level.CourseId,
                        CurrentLevelId = current.LevelId,
                        CurrentSectionId = next?.SectionId ?? currentSectionId,
                        LastUpdated = DateTime.UtcNow
                    };
                    await _uow.UserProgresses.AddAsync(progress);
                    _logger.LogInformation("Created new progress record for user {UserId}", userId);
                }
                else
                {
                    progress.CurrentLevelId = current.LevelId;
                    progress.CurrentSectionId = next?.SectionId ?? currentSectionId;
                    progress.LastUpdated = DateTime.UtcNow;
                    _uow.UserProgresses.Update(progress);
                    _logger.LogInformation("Updated progress record for user {UserId}", userId);
                }

                await _uow.SaveAsync();

                var result = new CompleteSectionResultDto
                {
                    Message = next != null ? "Moved to next section." : "This was the last section in this level.",
                    NextSectionId = next?.SectionId
                };

                _logger.LogInformation("Section {SectionId} completed for user {UserId}. Next section: {NextSectionId}",
                    currentSectionId, userId, next?.SectionId);

                return result;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error completing section {SectionId} for user {UserId}", currentSectionId, userId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the next section in the same level or the first section of the next level.
        /// If the course is finished, returns a message indicating completion.
        /// Throws KeyNotFoundException if the current progress or section cannot be found.
        /// </summary>
        public async Task<NextSectionDto> GetNextSectionAsync(int userId, int courseId)
        {
            try
            {
                _logger.LogInformation("Getting next section for user {UserId}, course {CourseId}", userId, courseId);

                var progress = (await _uow.UserProgresses.FindAsync(p =>
                    p.UserId == userId && p.CourseId == courseId))
                    .FirstOrDefault();

                if (progress == null)
                {
                    _logger.LogWarning("No progress record found for user {UserId}, course {CourseId}", userId, courseId);
                    throw new KeyNotFoundException($"No progress record found for user {userId} in course {courseId}.");
                }

                var currentSection = await _uow.Sections.Query()
                    .Include(s => s.Level)
                    .FirstOrDefaultAsync(s => s.SectionId == progress.CurrentSectionId);

                if (currentSection == null)
                {
                    _logger.LogWarning("Current section {SectionId} not found", progress.CurrentSectionId);
                    throw new KeyNotFoundException($"Section with ID {progress.CurrentSectionId} not found.");
                }

                var next = await _uow.Sections.Query()
                    .Where(s => s.LevelId == currentSection.LevelId && s.SectionOrder > currentSection.SectionOrder)
                    .OrderBy(s => s.SectionOrder)
                    .FirstOrDefaultAsync();

                if (next != null)
                {
                    _logger.LogInformation("Found next section {SectionId} in same level", next.SectionId);
                    return new NextSectionDto { SectionId = next.SectionId, SectionName = next.SectionName };
                }

                var nextLevel = await _uow.Levels.Query()
                    .Where(l => l.CourseId == courseId && l.LevelOrder > currentSection.Level.LevelOrder)
                    .OrderBy(l => l.LevelOrder)
                    .FirstOrDefaultAsync();

                if (nextLevel != null)
                {
                    var firstSection = await _uow.Sections.Query()
                        .Where(s => s.LevelId == nextLevel.LevelId)
                        .OrderBy(s => s.SectionOrder)
                        .FirstOrDefaultAsync();

                    if (firstSection != null)
                    {
                        _logger.LogInformation("Found first section {SectionId} in next level", firstSection.SectionId);
                        return new NextSectionDto { SectionId = firstSection.SectionId, SectionName = firstSection.SectionName };
                    }
                }

                _logger.LogInformation("Course completed for user {UserId}, course {CourseId}", userId, courseId);
                return new NextSectionDto { Message = "You have completed the course 🎉" };
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error getting next section for user {UserId}, course {CourseId}", userId, courseId);
                throw;
            }
        }

        // =====================================================
        // 10) User Statistics
        // =====================================================

        /// <summary>
        /// Returns summary statistics for the user:
        /// - Number of enrolled courses
        /// - Number of completed sections
        /// - Progress percentage per course
        /// </summary>
        public async Task<StudentStatsDto> GetUserStatsAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Retrieving statistics for user {UserId}", userId);

                var enrolledCourseIds = (await _uow.CourseEnrollments.FindAsync(e => e.UserId == userId))
                    .Select(e => e.CourseId)
                    .ToList();

                var completedSectionIds = (await _uow.UserContentActivities.FindAsync(a =>
                        a.UserId == userId && a.EndTime != null))
                    .Select(a => a.Content.SectionId)
                    .Distinct()
                    .ToList();

                var allProgress = await _uow.UserProgresses.Query()
                    .Where(p => p.UserId == userId)
                    .ToListAsync();

                var progressDtos = new List<CourseProgressDto>();
                foreach (var p in allProgress)
                {
                    var totalSections = await _uow.Sections.Query()
                        .Where(s => s.Level.CourseId == p.CourseId && !s.IsDeleted && s.IsVisible)
                        .CountAsync();

                    var currentOrder = await _uow.Sections.Query()
                        .Where(s => s.SectionId == p.CurrentSectionId)
                        .Select(s => s.SectionOrder)
                        .FirstOrDefaultAsync();

                    var percentage = totalSections > 0
                        ? (int)((double)currentOrder / totalSections * 100)
                        : 0;

                    progressDtos.Add(new CourseProgressDto
                    {
                        CourseId = p.CourseId,
                        ProgressPercentage = percentage
                    });
                }

                var result = new StudentStatsDto
                {
                    SharedCourses = enrolledCourseIds.Count,
                    CompletedSections = completedSectionIds.Count,
                    Progress = progressDtos
                };

                _logger.LogInformation("Retrieved statistics for user {UserId}: {EnrolledCourses} courses, {CompletedSections} sections",
                    userId, result.SharedCourses, result.CompletedSections);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving statistics for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Checks whether the user has completed all sections in a course.
        /// Returns total sections, completed sections count, and a boolean indicator.
        /// </summary>
        public async Task<CourseCompletionDto> HasCompletedCourseAsync(int userId, int courseId)
        {
            try
            {
                _logger.LogInformation("Checking course completion for user {UserId}, course {CourseId}", userId, courseId);

                var totalSections = await _uow.Sections.Query()
                    .Where(s => s.Level.CourseId == courseId && !s.IsDeleted && s.IsVisible)
                    .CountAsync();

                var completedCount = (await _uow.UserContentActivities.FindAsync(a =>
                        a.UserId == userId && a.EndTime != null && a.Content.Section.Level.CourseId == courseId))
                    .Select(a => a.Content.SectionId)
                    .Distinct()
                    .Count();

                var result = new CourseCompletionDto
                {
                    TotalSections = totalSections,
                    CompletedSections = completedCount,
                    IsCompleted = completedCount == totalSections
                };

                _logger.LogInformation("Course completion for user {UserId}, course {CourseId}: {CompletedSections}/{TotalSections} = {IsCompleted}",
                    userId, courseId, completedCount, totalSections, result.IsCompleted);

                if (result.IsCompleted)
                {
                    try
                    {
                        await _pointsService.AwardCourseCompletionPointsAsync(userId, courseId);
                        _logger.LogInformation("Course completion points awarded to user {UserId} for course {CourseId}", userId, courseId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error awarding course completion points for user {UserId}, course {CourseId}", userId, courseId);
                        // Don't fail the completion check if points awarding fails
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking course completion for user {UserId}, course {CourseId}", userId, courseId);
                throw;
            }
        }

        // =====================================================
        // 11) Notifications
        // =====================================================

        /// <summary>
        /// Retrieves all notifications for the user, sorted by creation date descending.
        /// Returns an empty list if none.
        /// </summary>
        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Retrieving notifications for user {UserId}", userId);

                var notifications = (await _uow.Notifications.FindAsync(n => n.UserId == userId))
                    .OrderByDescending(n => n.CreatedAt)
                    .ToList();

                var result = notifications.Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                }).ToList();

                _logger.LogInformation("Retrieved {NotificationCount} notifications for user {UserId}", result.Count, userId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Returns the count of unread notifications for the user.
        /// </summary>
        public async Task<int> GetUnreadNotificationsCountAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Getting unread notifications count for user {UserId}", userId);

                var count = await _uow.Notifications.Query()
                    .CountAsync(n => n.UserId == userId && !n.IsRead);

                _logger.LogInformation("User {UserId} has {UnreadCount} unread notifications", userId, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notifications count for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Marks a single notification as read.
        /// Throws KeyNotFoundException if the notification does not exist.
        /// </summary>
        public async Task MarkNotificationAsReadAsync(int userId, int notificationId)
        {
            try
            {
                _logger.LogInformation("Marking notification {NotificationId} as read for user {UserId}", notificationId, userId);

                var notification = await _uow.Notifications.Query()
                    .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

                if (notification == null)
                {
                    _logger.LogWarning("Notification {NotificationId} not found for user {UserId}", notificationId, userId);
                    throw new KeyNotFoundException($"Notification with ID {notificationId} not found.");
                }

                if (!notification.IsRead)
                {
                    notification.IsRead = true;
                    _uow.Notifications.Update(notification);
                    await _uow.SaveAsync();
                    _logger.LogInformation("Notification {NotificationId} marked as read for user {UserId}", notificationId, userId);
                }
                else
                {
                    _logger.LogInformation("Notification {NotificationId} already read for user {UserId}", notificationId, userId);
                }
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read for user {UserId}", notificationId, userId);
                throw;
            }
        }

        /// <summary>
        /// Marks all unread notifications for the user as read.
        /// </summary>
        public async Task MarkAllNotificationsAsReadAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Marking all notifications as read for user {UserId}", userId);

                var notifs = (await _uow.Notifications.FindAsync(n => n.UserId == userId && !n.IsRead))
                    .ToList();

                if (!notifs.Any())
                {
                    _logger.LogInformation("No unread notifications found for user {UserId}", userId);
                    return;
                }

                foreach (var n in notifs)
                    n.IsRead = true;

                await _uow.SaveAsync();
                _logger.LogInformation("Marked {NotificationCount} notifications as read for user {UserId}", notifs.Count, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public async Task AddToFavoritesAsync(int userId, int courseId)
        {
            try
            {
                // Check if course exists
                var course = await _uow.Courses.GetByIdAsync(courseId);
                if (course == null || course.IsDeleted)
                {
                    _logger.LogWarning("Course {CourseId} not found or deleted", courseId);
                    throw new KeyNotFoundException("Course not found");
                }

                // Check if already exists in favorites
                bool alreadyFavorite = await _uow.FavoriteCourses.Query()
                    .AnyAsync(f => f.UserId == userId && f.CourseId == courseId);

                if (alreadyFavorite)
                {
                    _logger.LogWarning("Course {CourseId} already in favorites for user {UserId}", courseId, userId);
                    throw new InvalidOperationException("Course already added to favorites");
                }

                // Add new favorite
                var favorite = new FavoriteCourse
                {
                    UserId = userId,
                    CourseId = courseId,
                    AddedAt = DateTime.UtcNow
                };

                await _uow.FavoriteCourses.AddAsync(favorite);
                await _uow.SaveAsync();

                _logger.LogInformation("Course {CourseId} added to favorites for user {UserId}", courseId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding course {CourseId} to favorites for user {UserId}", courseId, userId);
                throw;
            }
        }

        public async Task RemoveFromFavoritesAsync(int userId, int courseId)
        {
            try
            {
                // Check if exists in favorites
                var favorite = await _uow.FavoriteCourses.Query()
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.CourseId == courseId);

                if (favorite == null)
                {
                    _logger.LogWarning("Favorite course {CourseId} not found for user {UserId}", courseId, userId);
                    throw new KeyNotFoundException("Course not found in favorites");
                }

                _uow.FavoriteCourses.Remove(favorite);
                await _uow.SaveAsync();

                _logger.LogInformation("Course {CourseId} removed from favorites for user {UserId}", courseId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing course {CourseId} from favorites for user {UserId}", courseId, userId);
                throw;
            }
        }

        public async Task<ChangeUserNameResultDto> ChangeUserNameAsync(int userId, ChangeUserNameDto dto)
        {
            try
            {
                var user = await _uow.Users.GetByIdAsync(userId);
                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("User {UserId} not found for name change", userId);
                    throw new KeyNotFoundException("User not found");
                }

                var oldName = user.FullName;
                user.FullName = dto.NewFullName.Trim();

                _uow.Users.Update(user);
                await _uow.SaveAsync();

                _logger.LogInformation("User {UserId} changed name from '{OldName}' to '{NewName}'",
                    userId, oldName, dto.NewFullName);

                // حسب نظامك الحالي نفترض إنه يحتاج Refresh للـ Token بعد الاسم الجديد:
                bool requiresTokenRefresh = true;

                return new ChangeUserNameResultDto
                {
                    Success = true,
                    NewFullName = dto.NewFullName,
                    RequiresTokenRefresh = requiresTokenRefresh,
                    ChangedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing name for user {UserId}", userId);
                throw;
            }
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            try
            {
                // 1️⃣ تأكد إن اليوزر موجود
                var user = await _uow.Users.GetByIdAsync(userId);
                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("User {UserId} not found for password change", userId);
                    throw new KeyNotFoundException("User not found");
                }

                bool isCurrentValid = AuthHelpers.VerifyPassword(dto.CurrentPassword, user.PasswordHash);
                if (!isCurrentValid)
                {
                    _logger.LogWarning("Invalid current password provided for user {UserId}", userId);
                    throw new UnauthorizedAccessException("Invalid current password");
                }

                // 3️⃣ عمل Hash للباسورد الجديد
                string newPasswordHash = AuthHelpers.HashPassword(dto.NewPassword);

                // 4️⃣ تحديث البيانات
                user.PasswordHash = newPasswordHash;
                
                _uow.Users.Update(user);
                await _uow.SaveAsync();

                _logger.LogInformation("Password changed successfully for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                throw;
            }
        }

        public async Task RevokeAllRefreshTokensAsync(int userId, string reason)
        {
            try
            {
                var tokens = await _uow.RefreshTokens.Query()
                    .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow)
                    .ToListAsync();

                if (!tokens.Any())
                {
                    _logger.LogInformation("No active refresh tokens to revoke for user {UserId}", userId);
                    return;
                }

                foreach (var token in tokens)
                {
                    token.IsRevoked = true;
                }

                await _uow.SaveAsync();

                _logger.LogInformation("Revoked {TokenCount} refresh tokens for user {UserId}. Reason: {Reason}",
                    tokens.Count, userId, reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh tokens for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> VerifyCurrentPasswordAsync(int userId, string currentPassword)
        {
            try
            {
                var user = await _uow.Users.GetByIdAsync(userId);
                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("User {UserId} not found for password verification", userId);
                    return false;
                }

                bool isValid = AuthHelpers.VerifyPassword(currentPassword, user.PasswordHash);
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password for user {UserId}", userId);
                return false;
            }
        }


        public async Task<bool> IsCourseInFavoritesAsync(int userId, int courseId)
        {
            try
            {
                bool exists = await _uow.FavoriteCourses.Query()
                    .AnyAsync(f => f.UserId == userId && f.CourseId == courseId);

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking favorite status for user {UserId} and course {CourseId}", userId, courseId);
                return false;
            }
        }

        public async Task<IEnumerable<UserActivityDto>> GetRecentActivitiesAsync(int userId, int limit = 10)
        {
            try
            {
                var activities = await _uow.SecurityAuditLogs.Query()
                    .Where(log => log.UserId == userId)
                    .OrderByDescending(log => log.Timestamp)
                    .Take(limit)
                    .Select(log => new UserActivityDto
                    {
                        ActivityType = log.EventType,
                        Description = log.EventDetails ?? (log.Success ? "Action completed successfully" : "Failed action"),
                        Timestamp = log.Timestamp,
                        IpAddress = log.IpAddress,
                        UserAgent = log.UserAgent
                    })
                    .ToListAsync();

                return activities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent activities for user {UserId}", userId);
                return Enumerable.Empty<UserActivityDto>();
            }
        }

    }
}