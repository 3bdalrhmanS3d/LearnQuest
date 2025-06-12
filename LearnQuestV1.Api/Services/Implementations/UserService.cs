using LearnQuestV1.Api.DTOs.Payments;
using LearnQuestV1.Api.DTOs.Profile;
using LearnQuestV1.Api.DTOs.Progress;
using LearnQuestV1.Api.DTOs.User.Response;
using LearnQuestV1.Api.DTOs.Users.Request;
using LearnQuestV1.Api.DTOs.Users.Response;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Models.Financial;
using LearnQuestV1.Core.Models.LearningAndProgress;
using LearnQuestV1.Core.Models.UserManagement;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _env;

        public UserService(IUnitOfWork uow, IWebHostEnvironment env)
        {
            _uow = uow;
            _env = env;
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
            // Include UserDetail and UserProgresses → Course for each progress
            var user = await _uow.Users.Query()
                .Include(u => u.UserDetail)
                .Include(u => u.UserProgresses)
                    .ThenInclude(up => up.Course)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            if (user.UserDetail == null)
                throw new InvalidOperationException($"UserDetail for user ID {userId} is missing.");

            var progressDtos = user.UserProgresses.Select(up => new UserProgressDto
            {
                CourseId = up.CourseId,
                CourseName = up.Course?.CourseName ?? string.Empty,
                CurrentLevelId = up.CurrentLevelId,
                CurrentSectionId = up.CurrentSectionId,
                LastUpdated = up.LastUpdated
            }).ToList();

            return new UserProfileDto
            {
                FullName = user.FullName,
                EmailAddress = user.EmailAddress,
                Role = user.Role.ToString(),
                ProfilePhoto = user.ProfilePhoto ?? string.Empty,
                CreatedAt = user.CreatedAt,
                BirthDate = user.UserDetail.BirthDate,
                Edu = user.UserDetail.EducationLevel,
                National = user.UserDetail.Nationality,
                Progress = progressDtos
            };
        }

        /// <summary>
        /// Creates or updates a user's personal details.
        /// Throws KeyNotFoundException if the user does not exist.
        /// </summary>
        public async Task UpdateUserProfileAsync(int userId, UserProfileUpdateDto dto)
        {
            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

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
            }
            else
            {
                detail.BirthDate = dto.BirthDate;
                detail.EducationLevel = dto.Edu;
                detail.Nationality = dto.National;
                _uow.UserDetails.Update(detail);
            }

            await _uow.SaveAsync();
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
            var course = await _uow.Courses.GetByIdAsync(dto.CourseId);
            if (course == null)
                throw new KeyNotFoundException($"Course with ID {dto.CourseId} not found.");

            var existingPayment = (await _uow.Payments.FindAsync(p =>
                p.UserId == userId &&
                p.CourseId == dto.CourseId &&
                p.Status == PaymentStatus.Completed))
                .Any();
            if (existingPayment)
                throw new InvalidOperationException("You have already paid for this course.");

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
        }

        /// <summary>
        /// Confirms a pending payment and enrolls the user in the course.
        /// Throws KeyNotFoundException if the payment record does not exist.
        /// Throws InvalidOperationException if it's already completed.
        /// </summary>
        public async Task ConfirmPaymentAsync(int paymentId)
        {
            var payment = await _uow.Payments.GetByIdAsync(paymentId);
            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {paymentId} not found.");

            if (payment.Status == PaymentStatus.Completed)
                throw new InvalidOperationException("Payment already completed.");

            payment.Status = PaymentStatus.Completed;
            _uow.Payments.Update(payment);
            await _uow.SaveAsync();

            var enrollment = new CourseEnrollment
            {
                UserId = payment.UserId,
                CourseId = payment.CourseId,
                EnrolledAt = DateTime.UtcNow
            };
            await _uow.CourseEnrollments.AddAsync(enrollment);
            await _uow.SaveAsync();
        }

        // =====================================================
        // 3) User's Courses & Favorites
        // =====================================================

        /// <summary>
        /// Retrieves all courses the user has paid for and is enrolled in.
        /// Returns an empty list if none.
        /// </summary>
        public async Task<IEnumerable<MyCourseDto>> GetMyCoursesAsync(int userId)
        {
            var enrollments = await _uow.CourseEnrollments.FindAsync(e => e.UserId == userId);
            var result = new List<MyCourseDto>();

            foreach (var e in enrollments)
            {
                var course = await _uow.Courses.GetByIdAsync(e.CourseId);
                if (course == null)
                    continue;

                var isPaid = (await _uow.Payments.FindAsync(p =>
                    p.UserId == userId && p.CourseId == e.CourseId && p.Status == PaymentStatus.Completed))
                    .Any();
                if (!isPaid)
                    continue;

                result.Add(new MyCourseDto
                {
                    CourseId = e.CourseId,
                    CourseName = course.CourseName,
                    Description = course.Description,
                    EnrolledAt = e.EnrolledAt
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
            var favorites = await _uow.FavoriteCourses.FindAsync(f => f.UserId == userId);
            var list = new List<CourseDto>();

            foreach (var f in favorites)
            {
                var course = await _uow.Courses.GetByIdAsync(f.CourseId);
                if (course == null)
                    continue;

                list.Add(new CourseDto
                {
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    Description = course.Description,
                    CourseImage = course.CourseImage ?? string.Empty,
                    CoursePrice = course.CoursePrice
                });
            }

            return list;
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
            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            if (file == null || file.Length == 0)
                throw new InvalidOperationException("No file uploaded.");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/profile-pictures");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"user_{userId}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            user.ProfilePhoto = $"/uploads/profile-pictures/{fileName}";
            _uow.Users.Update(user);
            await _uow.SaveAsync();
        }

        /// <summary>
        /// Deletes the user's custom profile photo and reverts to default.
        /// Throws KeyNotFoundException if the user does not exist.
        /// Throws InvalidOperationException if no custom photo to delete.
        /// </summary>
        public async Task DeleteProfilePhotoAsync(int userId)
        {
            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            if (string.IsNullOrEmpty(user.ProfilePhoto) || user.ProfilePhoto.EndsWith("default.png"))
                throw new InvalidOperationException("No custom profile photo to delete.");

            var filePath = Path.Combine(_env.WebRootPath, user.ProfilePhoto.TrimStart('/'));
            if (File.Exists(filePath))
                File.Delete(filePath);

            user.ProfilePhoto = "/uploads/profile-pictures/default.png";
            _uow.Users.Update(user);
            await _uow.SaveAsync();
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
            var tracks = await _uow.CourseTracks.Query()
                .Include(t => t.CourseTrackCourses)
                    .ThenInclude(ctc => ctc.Course)
                .ToListAsync();

            if (!tracks.Any())
                return Array.Empty<TrackDto>();

            return tracks.Select(t => new TrackDto
            {
                TrackId = t.TrackId,
                TrackName = t.TrackName,
                TrackDescription = t.TrackDescription ?? string.Empty,
                CourseCount = t.CourseTrackCourses.Count(ctc =>
                    !ctc.Course.IsDeleted && ctc.Course.IsActive)
            }).ToList();
        }

        /// <summary>
        /// Retrieves all courses in a given track.
        /// Throws KeyNotFoundException if the track does not exist.
        /// </summary>
        public async Task<TrackCoursesDto> GetCoursesInTrackAsync(int trackId)
        {
            var track = await _uow.CourseTracks.Query()
                .Include(t => t.CourseTrackCourses)
                    .ThenInclude(ctc => ctc.Course)
                        .ThenInclude(c => c.Instructor)
                .Include(t => t.CourseTrackCourses)
                    .ThenInclude(ctc => ctc.Course)
                        .ThenInclude(c => c.Levels)
                .FirstOrDefaultAsync(t => t.TrackId == trackId);

            if (track == null)
                throw new KeyNotFoundException($"Track with ID {trackId} not found.");

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

            return new TrackCoursesDto
            {
                TrackId = track.TrackId,
                TrackName = track.TrackName,
                TrackDescription = track.TrackDescription ?? string.Empty,
                TotalCourses = courses.Count,
                Courses = courses
            };
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
                return Array.Empty<CourseDto>();

            return list.Select(c => new CourseDto
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                Description = c.Description,
                CourseImage = c.CourseImage ?? string.Empty,
                CoursePrice = c.CoursePrice
            }).ToList();
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
            var enrolled = await _uow.CourseEnrollments.Query()
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
            if (!enrolled)
                throw new InvalidOperationException("User is not enrolled in this course.");

            var course = await _uow.Courses.Query()
                .Include(c => c.Levels.Where(l => !l.IsDeleted && l.IsVisible)
                    .OrderBy(l => l.LevelOrder))
                .FirstOrDefaultAsync(c => c.CourseId == courseId && !c.IsDeleted);

            if (course == null)
                throw new KeyNotFoundException($"Course with ID {courseId} not found.");

            var levels = course.Levels.Select(l => new LevelDto
            {
                LevelId = l.LevelId,
                LevelName = l.LevelName,
                LevelDetails = l.LevelDetails,
                LevelOrder = l.LevelOrder,
                IsVisible = l.IsVisible
            }).ToList();

            return new LevelsResponseDto
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                Description = course.Description,
                CourseImage = course.CourseImage ?? string.Empty,
                LevelsCount = levels.Count,
                Levels = levels
            };
        }

        /// <summary>
        /// Returns all visible sections for a given level, if the user is enrolled.
        /// Throws InvalidOperationException if not enrolled.
        /// Throws KeyNotFoundException if the level does not exist.
        /// Returns an empty sections list if none visible.
        /// </summary>
        public async Task<SectionsResponseDto> GetLevelSectionsAsync(int userId, int levelId)
        {
            var level = await _uow.Levels.Query()
                .Include(l => l.Sections.OrderBy(s => s.SectionOrder))
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LevelId == levelId && !l.IsDeleted);

            if (level == null)
                throw new KeyNotFoundException($"Level with ID {levelId} not found.");

            var enrolled = await _uow.CourseEnrollments.Query()
                .AnyAsync(e => e.UserId == userId && e.CourseId == level.CourseId);
            if (!enrolled)
                throw new InvalidOperationException("User is not enrolled in this course.");

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

            return new SectionsResponseDto
            {
                LevelId = level.LevelId,
                LevelName = level.LevelName,
                LevelDetails = level.LevelDetails,
                Sections = sections
            };
        }

        /// <summary>
        /// Returns all visible contents for a given section, if the user is enrolled.
        /// Throws InvalidOperationException if not enrolled.
        /// Throws KeyNotFoundException if the section does not exist.
        /// Returns an empty contents list if none visible.
        /// </summary>
        public async Task<ContentsResponseDto> GetSectionContentsAsync(int userId, int sectionId)
        {
            var section = await _uow.Sections.Query()
                .Include(s => s.Level)
                    .ThenInclude(l => l.Course)
                .Include(s => s.Contents.Where(c => c.IsVisible).OrderBy(c => c.ContentOrder))
                .FirstOrDefaultAsync(s => s.SectionId == sectionId && !s.IsDeleted && s.IsVisible);

            if (section == null)
                throw new KeyNotFoundException($"Section with ID {sectionId} not found.");

            var enrolled = await _uow.CourseEnrollments.Query()
                .AnyAsync(e => e.UserId == userId && e.CourseId == section.Level.CourseId);
            if (!enrolled)
                throw new InvalidOperationException("User is not enrolled in this course.");

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

            return new ContentsResponseDto
            {
                SectionId = section.SectionId,
                SectionName = section.SectionName,
                Contents = contents
            };
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
            var exists = (await _uow.UserContentActivities.FindAsync(a =>
                a.UserId == userId && a.ContentId == contentId && a.EndTime == null))
                .FirstOrDefault();
            if (exists != null)
                throw new InvalidOperationException("Content session already started and not yet ended.");

            var activity = new UserContentActivity
            {
                UserId = userId,
                ContentId = contentId,
                StartTime = DateTime.UtcNow
            };
            await _uow.UserContentActivities.AddAsync(activity);
            await _uow.SaveAsync();
        }

        /// <summary>
        /// Marks the end time for a user's content activity.
        /// Throws KeyNotFoundException if no active session is found.
        /// </summary>
        public async Task EndContentAsync(int userId, int contentId)
        {
            var activity = (await _uow.UserContentActivities.FindAsync(a =>
                a.UserId == userId && a.ContentId == contentId && a.EndTime == null))
                .FirstOrDefault();
            if (activity == null)
                throw new KeyNotFoundException("No active content session found to end.");

            activity.EndTime = DateTime.UtcNow;
            _uow.UserContentActivities.Update(activity);
            await _uow.SaveAsync();
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
            var current = await _uow.Sections.Query()
                .Include(s => s.Level)
                .FirstOrDefaultAsync(s => s.SectionId == currentSectionId);
            if (current == null)
                throw new KeyNotFoundException($"Section with ID {currentSectionId} not found.");

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
            }
            else
            {
                progress.CurrentLevelId = current.LevelId;
                progress.CurrentSectionId = next?.SectionId ?? currentSectionId;
                progress.LastUpdated = DateTime.UtcNow;
                _uow.UserProgresses.Update(progress);
            }
            await _uow.SaveAsync();

            return new CompleteSectionResultDto
            {
                Message = next != null ? "Moved to next section." : "This was the last section in this level.",
                NextSectionId = next?.SectionId
            };
        }

        /// <summary>
        /// Retrieves the next section in the same level or the first section of the next level.
        /// If the course is finished, returns a message indicating completion.
        /// Throws KeyNotFoundException if the current progress or section cannot be found.
        /// </summary>
        public async Task<NextSectionDto> GetNextSectionAsync(int userId, int courseId)
        {
            var progress = (await _uow.UserProgresses.FindAsync(p =>
                p.UserId == userId && p.CourseId == courseId))
                .FirstOrDefault();
            if (progress == null)
                throw new KeyNotFoundException($"No progress record found for user {userId} in course {courseId}.");

            var currentSection = await _uow.Sections.GetByIdAsync(progress.CurrentSectionId);
            if (currentSection == null)
                throw new KeyNotFoundException($"Section with ID {progress.CurrentSectionId} not found.");

            var next = await _uow.Sections.Query()
                .Where(s => s.LevelId == currentSection.LevelId && s.SectionOrder > currentSection.SectionOrder)
                .OrderBy(s => s.SectionOrder)
                .FirstOrDefaultAsync();

            if (next != null)
                return new NextSectionDto { SectionId = next.SectionId, SectionName = next.SectionName };

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
                    return new NextSectionDto { SectionId = firstSection.SectionId, SectionName = firstSection.SectionName };
            }

            return new NextSectionDto { Message = "You have completed the course 🎉" };
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
        public async Task<UserStatsDto> GetUserStatsAsync(int userId)
        {
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

            return new UserStatsDto
            {
                SharedCourses = enrolledCourseIds.Count,
                CompletedSections = completedSectionIds.Count,
                Progress = progressDtos
            };
        }

        /// <summary>
        /// Checks whether the user has completed all sections in a course.
        /// Returns total sections, completed sections count, and a boolean indicator.
        /// </summary>
        public async Task<CourseCompletionDto> HasCompletedCourseAsync(int userId, int courseId)
        {
            var totalSections = await _uow.Sections.Query()
                .Where(s => s.Level.CourseId == courseId && !s.IsDeleted && s.IsVisible)
                .CountAsync();

            var completedCount = (await _uow.UserContentActivities.FindAsync(a =>
                    a.UserId == userId && a.EndTime != null && a.Content.Section.Level.CourseId == courseId))
                .Select(a => a.Content.SectionId)
                .Distinct()
                .Count();

            return new CourseCompletionDto
            {
                TotalSections = totalSections,
                CompletedSections = completedCount,
                IsCompleted = completedCount == totalSections
            };
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
            var notifications = (await _uow.Notifications.FindAsync(n => n.UserId == userId))
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return notifications.Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList();
        }

        /// <summary>
        /// Returns the count of unread notifications for the user.
        /// </summary>
        public async Task<int> GetUnreadNotificationsCountAsync(int userId)
        {
            return await _uow.Notifications.Query()
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        /// <summary>
        /// Marks a single notification as read.
        /// Throws KeyNotFoundException if the notification does not exist.
        /// </summary>
        public async Task MarkNotificationAsReadAsync(int userId, int notificationId)
        {
            var notification = await _uow.Notifications.Query()
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);
            if (notification == null)
                throw new KeyNotFoundException($"Notification with ID {notificationId} not found.");

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                _uow.Notifications.Update(notification);
                await _uow.SaveAsync();
            }
        }

        /// <summary>
        /// Marks all unread notifications for the user as read.
        /// </summary>
        public async Task MarkAllNotificationsAsReadAsync(int userId)
        {
            var notifs = (await _uow.Notifications.FindAsync(n => n.UserId == userId && !n.IsRead))
                .ToList();

            if (!notifs.Any())
                return;

            foreach (var n in notifs)
                n.IsRead = true;

            await _uow.SaveAsync();
        }
    }
}
