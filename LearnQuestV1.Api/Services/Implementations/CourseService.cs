using LearnQuestV1.Api.DTOs.Courses;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models.CourseStructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _uow;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CourseService> _logger;

        // Predefined skills to ensure consistency
        private static readonly HashSet<string> PredefinedSkills = new()
        {
            "C#", "Java", "Python", "JavaScript", "TypeScript", "React", "Angular", "Vue.js",
            "Node.js", "ASP.NET Core", "Spring Boot", "Express.js", "MongoDB", "SQL Server",
            "MySQL", "PostgreSQL", "Redis", "Docker", "Kubernetes", "AWS", "Azure", "Git",
            "HTML", "CSS", "Bootstrap", "Tailwind CSS", "SASS", "REST APIs", "GraphQL",
            "Microservices", "System Design", "Data Structures", "Algorithms", "OOP",
            "SOLID Principles", "Design Patterns", "Unit Testing", "Integration Testing",
            "CI/CD", "DevOps", "Linux", "Networking", "Security", "Machine Learning",
            "AI", "Data Science", "Analytics", "Project Management", "Agile", "Scrum"
        };

        public CourseService(IUnitOfWork uow, IHttpContextAccessor httpContextAccessor, ILogger<CourseService> logger)
        {
            _uow = uow;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<IEnumerable<CourseCDto>> GetAllCoursesForInstructorAsync(int? instructorId = null, int pageNumber = 1, int pageSize = 10)
        {
            var user = GetCurrentUser();
            var currentUserId = user.GetCurrentUserId();

            if (currentUserId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            // Use provided instructorId or current user's ID
            var targetInstructorId = instructorId ?? currentUserId.Value;

            // If not admin and trying to view another instructor's courses, deny access
            if (!user.IsInRole("Admin") && targetInstructorId != currentUserId.Value)
                throw new UnauthorizedAccessException("Access denied.");

            var coursesQuery = _uow.Courses.Query()
                .Include(c => c.Instructor)
                .Include(c => c.CourseEnrollments)
                .Include(c => c.CourseReviews)
                .Include(c => c.Levels)
                    .ThenInclude(l => l.Sections)
                        .ThenInclude(s => s.Contents)
                .Where(c => c.InstructorId == targetInstructorId && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt);

            var courses = await coursesQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return courses.Select(c => new CourseCDto
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                CourseImage = c.CourseImage ?? "/uploads/courses/default.jpg",
                CoursePrice = c.CoursePrice,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                InstructorName = c.Instructor.FullName,
                EnrollmentCount = c.CourseEnrollments.Count,
                AverageRating = c.CourseReviews.Any() ? (decimal)c.CourseReviews.Average(r => r.Rating) : null,
                ReviewCount = c.CourseReviews.Count,
                LevelsCount = c.Levels.Count,
                SectionsCount = c.Levels.SelectMany(l => l.Sections).Count(),
                ContentsCount = c.Levels.SelectMany(l => l.Sections).SelectMany(s => s.Contents).Count()
            });
        }

        public async Task<IEnumerable<CourseCDto>> GetAllCoursesForAdminAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool? isActive = null)
        {
            var user = GetCurrentUser();
            if (!user.IsInRole("Admin"))
                throw new UnauthorizedAccessException("Admin access required.");

            var coursesQuery = _uow.Courses.Query()
                .Include(c => c.Instructor)
                .Include(c => c.CourseEnrollments)
                .Include(c => c.CourseReviews)
                .Include(c => c.Levels)
                    .ThenInclude(l => l.Sections)
                        .ThenInclude(s => s.Contents)
                .Where(c => !c.IsDeleted);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                coursesQuery = coursesQuery.Where(c =>
                    c.CourseName.ToLower().Contains(searchTerm) ||
                    c.Description.ToLower().Contains(searchTerm) ||
                    c.Instructor.FullName.ToLower().Contains(searchTerm));
            }

            if (isActive.HasValue)
            {
                coursesQuery = coursesQuery.Where(c => c.IsActive == isActive.Value);
            }

            var courses = await coursesQuery
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return courses.Select(c => new CourseCDto
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                CourseImage = c.CourseImage ?? "/uploads/courses/default.jpg",
                CoursePrice = c.CoursePrice,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                InstructorName = c.Instructor.FullName,
                EnrollmentCount = c.CourseEnrollments.Count,
                AverageRating = c.CourseReviews.Any() ? (decimal)c.CourseReviews.Average(r => r.Rating) : null,
                ReviewCount = c.CourseReviews.Count,
                LevelsCount = c.Levels.Count,
                SectionsCount = c.Levels.SelectMany(l => l.Sections).Count(),
                ContentsCount = c.Levels.SelectMany(l => l.Sections).SelectMany(s => s.Contents).Count()
            });
        }

        public async Task<CourseOverviewDto> GetCourseOverviewAsync(int courseId)
        {
            var user = GetCurrentUser();
            var course = await GetCourseWithValidationAsync(courseId);

            // Calculate statistics
            var enrollments = await _uow.CourseEnrollments.Query()
                .Where(e => e.CourseId == courseId)
                .ToListAsync();

            var payments = await _uow.Payments.Query()
                .Where(p => p.CourseId == courseId && p.Status == PaymentStatus.Completed)
                .ToListAsync();

            var progresses = await _uow.UserProgresses.Query()
                .Include(p => p.Course)
                    .ThenInclude(c => c.Levels)
                        .ThenInclude(l => l.Sections)
                            .ThenInclude(s => s.Contents)
                .Where(p => p.CourseId == courseId)
                .ToListAsync();

            var quizzes = await _uow.Quizzes.Query()
                .Where(q => q.CourseId == courseId && !q.IsDeleted)
                .CountAsync();

            var contents = await _uow.Contents.Query()
                .Include(c => c.Section)
                    .ThenInclude(s => s.Level)
                .Where(c => c.Section.Level.CourseId == courseId && c.IsVisible)
                .ToListAsync();

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var recentEnrollments = enrollments.Count(e => e.EnrolledAt >= thirtyDaysAgo);
            var recentPayments = payments.Where(p => p.PaymentDate >= thirtyDaysAgo).Sum(p => p.Amount);

            // Calculate completion rate
            var totalContent = contents.Count;
            var completedEnrollments = 0;
            var activeEnrollments = 0;

            foreach (var progress in progresses)
            {
                var userContents = await _uow.UserContentActivities.Query()
                    .Where(a => a.UserId == progress.UserId && contents.Select(c => c.ContentId).Contains(a.ContentId))
                    .CountAsync();

                if (userContents >= totalContent * 0.9) // 90% completion threshold
                    completedEnrollments++;
                else if (userContents > 0)
                    activeEnrollments++;
            }

            return new CourseOverviewDto
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                Description = course.Description,
                CourseImage = course.CourseImage ?? "/uploads/courses/default.jpg",
                CoursePrice = course.CoursePrice,
                IsActive = course.IsActive,
                CreatedAt = course.CreatedAt,
                InstructorName = course.Instructor.FullName,
                InstructorId = course.InstructorId,

                EnrollmentCount = enrollments.Count,
                ActiveEnrollmentCount = activeEnrollments,
                CompletedEnrollmentCount = completedEnrollments,
                TotalRevenue = payments.Sum(p => p.Amount),

                LevelsCount = course.Levels.Count,
                SectionsCount = course.Levels.SelectMany(l => l.Sections).Count(),
                ContentsCount = contents.Count,
                QuizzesCount = quizzes,
                TotalDurationMinutes = contents.Sum(c => c.DurationInMinutes),

                AverageRating = course.CourseReviews.Any() ? (decimal)course.CourseReviews.Average(r => r.Rating) : null,
                ReviewCount = course.CourseReviews.Count,
                ReviewSummary = await GetCourseReviewSummaryAsync(courseId),

                RecentEnrollments = recentEnrollments,
                RecentCompletions = 0, // This would need more complex calculation
                RecentRevenue = recentPayments
            };
        }

        public async Task<CourseDetailsDto> GetCourseDetailsAsync(int courseId)
        {
            var course = await GetCourseWithValidationAsync(courseId);

            var levels = await _uow.Levels.Query()
                .Include(l => l.Sections)
                    .ThenInclude(s => s.Contents)
                .Where(l => l.CourseId == courseId && !l.IsDeleted)
                .OrderBy(l => l.LevelOrder)
                .ToListAsync();

            var quizCounts = await _uow.Quizzes.Query()
                .Where(q => q.CourseId == courseId
                            && !q.IsDeleted
                            && q.LevelId.HasValue)
                .GroupBy(q => q.LevelId!.Value)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            return new CourseDetailsDto
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                Description = course.Description,
                CourseImage = course.CourseImage ?? "/uploads/courses/default.jpg",
                CoursePrice = course.CoursePrice,
                IsActive = course.IsActive,
                CreatedAt = course.CreatedAt,
                InstructorName = course.Instructor.FullName,
                InstructorId = course.InstructorId,

                AboutCourses = course.AboutCourses.Select(a => new AboutCourseItem
                {
                    AboutCourseId = a.AboutCourseId,
                    AboutCourseText = a.AboutCourseText,
                    OutcomeType = a.OutcomeType.ToString()
                }),

                CourseSkills = course.CourseSkills.Select(s => s.CourseSkillText),

                Levels = levels.Select(l => new LevelOverviewDto
                {
                    LevelId = l.LevelId,
                    LevelName = l.LevelName,
                    LevelOrder = l.LevelOrder,
                    SectionsCount = l.Sections.Count,
                    ContentsCount = l.Sections.SelectMany(s => s.Contents).Count(),
                    QuizzesCount = quizCounts.GetValueOrDefault(l.LevelId, 0),
                    IsVisible = l.IsVisible
                })
            };
        }

        public async Task<int> CreateCourseAsync(CreateCourseDto input, IFormFile? imageFile)
        {
            var user = GetCurrentUser();
            var currentUserId = user.GetCurrentUserId();

            if (currentUserId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            // Verify user can create courses (Instructor or Admin)
            if (!user.IsInRole("Instructor") && !user.IsInRole("Admin"))
                throw new UnauthorizedAccessException("Only instructors and admins can create courses.");

            var userExists = await _uow.Users.Query()
                .AnyAsync(u => u.UserId == currentUserId.Value && !u.IsDeleted);

            if (!userExists)
                throw new KeyNotFoundException("User not found or is deleted.");

            // Create course entity
            var newCourse = new Course
            {
                CourseName = input.CourseName.Trim(),
                Description = input.Description.Trim(),
                CoursePrice = input.CoursePrice,
                CourseImage = "/uploads/courses/default.jpg",
                InstructorId = currentUserId.Value,
                CreatedAt = DateTime.UtcNow,
                IsActive = input.IsActive
            };

            await _uow.Courses.AddAsync(newCourse);
            await _uow.SaveAsync();

            if (imageFile != null && imageFile.Length > 0)
            {
                // Upload course image
                var imageUrl = await UploadCourseImageAsync(newCourse.CourseId, imageFile);
                newCourse.CourseImage = imageUrl;
                _uow.Courses.Update(newCourse);
            }

            // Add AboutCourse entries
            if (input.AboutCourseInputs != null && input.AboutCourseInputs.Any())
            {
                foreach (var aboutInput in input.AboutCourseInputs)
                {
                    var about = new AboutCourse
                    {
                        CourseId = newCourse.CourseId,
                        AboutCourseText = FormatText(aboutInput.AboutCourseText),
                        OutcomeType = Enum.TryParse<CourseOutcomeType>(aboutInput.OutcomeType, true, out var o)
                                      ? o
                                      : CourseOutcomeType.Learn
                    };
                    await _uow.AboutCourses.AddAsync(about);
                }
            }

            // Add normalized CourseSkill entries
            if (input.CourseSkillInputs != null && input.CourseSkillInputs.Any())
            {
                var normalizedSkills = await GetNormalizedSkillsAsync(input.CourseSkillInputs);
                foreach (var skillText in normalizedSkills)
                {
                    var skill = new CourseSkill
                    {
                        CourseId = newCourse.CourseId,
                        CourseSkillText = skillText
                    };
                    await _uow.CourseSkills.AddAsync(skill);
                }
            }

            await _uow.SaveAsync();
            return newCourse.CourseId;
        }

        public async Task UpdateCourseAsync(int courseId, UpdateCourseDto input)
        {
            var course = await GetCourseWithValidationAsync(courseId);

            // Update basic fields
            if (!string.IsNullOrWhiteSpace(input.CourseName))
                course.CourseName = input.CourseName.Trim();
            if (!string.IsNullOrWhiteSpace(input.Description))
                course.Description = input.Description.Trim();
            if (input.CoursePrice.HasValue && input.CoursePrice.Value >= 0)
                course.CoursePrice = input.CoursePrice.Value;
            if (!string.IsNullOrWhiteSpace(input.CourseImage))
                course.CourseImage = input.CourseImage.Trim();
            if (input.IsActive.HasValue)
                course.IsActive = input.IsActive.Value;

            // Handle AboutCourse updates
            if (input.AboutCourseInputs != null)
            {
                var existingAbout = course.AboutCourses.ToList();
                var updatedIds = input.AboutCourseInputs.Select(ai => ai.AboutCourseId).ToList();

                // Remove deleted items
                foreach (var about in existingAbout.Where(a => !updatedIds.Contains(a.AboutCourseId)))
                {
                    _uow.AboutCourses.Remove(about);
                }

                // Add or update items
                foreach (var aboutInput in input.AboutCourseInputs)
                {
                    if (aboutInput.AboutCourseId == 0)
                    {
                        // New item
                        var newAbout = new AboutCourse
                        {
                            CourseId = course.CourseId,
                            AboutCourseText = FormatText(aboutInput.AboutCourseText),
                            OutcomeType = Enum.TryParse<Core.Enums.CourseOutcomeType>(aboutInput.OutcomeType, true, out var o)
                                          ? o
                                          : Core.Enums.CourseOutcomeType.Learn
                        };
                        await _uow.AboutCourses.AddAsync(newAbout);
                    }
                    else
                    {
                        // Update existing
                        var existing = existingAbout.FirstOrDefault(a => a.AboutCourseId == aboutInput.AboutCourseId);
                        if (existing != null)
                        {
                            existing.AboutCourseText = FormatText(aboutInput.AboutCourseText);
                            existing.OutcomeType = Enum.TryParse<Core.Enums.CourseOutcomeType>(aboutInput.OutcomeType, true, out var o2)
                                                   ? o2
                                                   : Core.Enums.CourseOutcomeType.Learn;
                            _uow.AboutCourses.Update(existing);
                        }
                    }
                }
            }

            // Handle CourseSkill updates with normalization
            if (input.CourseSkillInputs != null)
            {
                // Remove all existing skills
                var existingSkills = course.CourseSkills.ToList();
                foreach (var skill in existingSkills)
                {
                    _uow.CourseSkills.Remove(skill);
                }

                // Add normalized skills
                var normalizedSkills = await GetNormalizedSkillsAsync(input.CourseSkillInputs);
                foreach (var skillText in normalizedSkills)
                {
                    var newSkill = new CourseSkill
                    {
                        CourseId = course.CourseId,
                        CourseSkillText = skillText
                    };
                    await _uow.CourseSkills.AddAsync(newSkill);
                }
            }

            _uow.Courses.Update(course);
            await _uow.SaveAsync();
        }

        public async Task DeleteCourseAsync(int courseId)
        {
            var course = await GetCourseWithValidationAsync(courseId);
            course.IsDeleted = true;
            _uow.Courses.Update(course);
            await _uow.SaveAsync();
        }

        public async Task ToggleCourseStatusAsync(int courseId)
        {
            var course = await GetCourseWithValidationAsync(courseId);
            course.IsActive = !course.IsActive;
            _uow.Courses.Update(course);
            await _uow.SaveAsync();
        }

        public async Task<string> UploadCourseImageAsync(int courseId, IFormFile file)
        {
            var course = await GetCourseWithValidationAsync(courseId);

            if (file == null || file.Length == 0)
                throw new InvalidDataException("No file uploaded.");

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
                throw new InvalidDataException("Invalid file type. Only JPG, JPEG, PNG, GIF, and WebP are allowed.");

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                throw new InvalidDataException("File size cannot exceed 5MB.");

            // Create uploads directory
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "courses");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Generate unique filename
            var fileName = $"course_{courseId}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Delete old image if exists
            if (!string.IsNullOrWhiteSpace(course.CourseImage) && course.CourseImage != "/uploads/courses/default.jpg")
            {
                var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", course.CourseImage.TrimStart('/'));
                if (File.Exists(oldImagePath))
                {
                    try
                    {
                        File.Delete(oldImagePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete old course image: {ImagePath}", oldImagePath);
                    }
                }
            }

            // Save new image
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Update course image URL
            var relativeUrl = $"/uploads/courses/{fileName}";
            course.CourseImage = relativeUrl;
            _uow.Courses.Update(course);
            await _uow.SaveAsync();

            return relativeUrl;
        }

        public async Task<AvailableSkillsDto> GetAvailableSkillsAsync(string? searchTerm = null, int pageNumber = 1, int pageSize = 50)
        {
            var skills = PredefinedSkills.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLowerInvariant();
                skills = skills.Where(s => s.ToLowerInvariant().Contains(searchTerm));
            }

            var totalCount = skills.Count();
            var pagedSkills = skills
                .OrderBy(s => s)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new AvailableSkillsDto
            {
                Skills = pagedSkills,
                TotalCount = totalCount
            };
        }

        public async Task<IEnumerable<string>> GetNormalizedSkillsAsync(IEnumerable<string> skillInputs)
        {
            var normalizedSkills = new HashSet<string>();

            foreach (var input in skillInputs.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                var trimmedInput = input.Trim();

                // Find exact match first
                var exactMatch = PredefinedSkills.FirstOrDefault(s =>
                    string.Equals(s, trimmedInput, StringComparison.OrdinalIgnoreCase));

                if (exactMatch != null)
                {
                    normalizedSkills.Add(exactMatch);
                }
                else
                {
                    // Find partial match
                    var partialMatch = PredefinedSkills.FirstOrDefault(s =>
                        s.ToLowerInvariant().Contains(trimmedInput.ToLowerInvariant()) ||
                        trimmedInput.ToLowerInvariant().Contains(s.ToLowerInvariant()));

                    if (partialMatch != null)
                    {
                        normalizedSkills.Add(partialMatch);
                    }
                    else
                    {
                        // Add as custom skill if no match found
                        normalizedSkills.Add(FormatText(trimmedInput));
                    }
                }
            }

            return normalizedSkills;
        }

        public async Task<CourseAnalyticsDto> GetCourseAnalyticsAsync(int courseId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var course = await GetCourseWithValidationAsync(courseId);

            startDate ??= DateTime.UtcNow.AddMonths(-6);
            endDate ??= DateTime.UtcNow;

            // This would be a complex implementation
            // For now, returning a basic structure
            return new CourseAnalyticsDto
            {
                CourseId = courseId,
                CourseName = course.CourseName,
                // Additional analytics would be implemented here
            };
        }

        public async Task<ReviewSummaryDto> GetCourseReviewSummaryAsync(int courseId)
        {
            var reviews = await _uow.CourseReviews.Query()
                .Include(r => r.User)
                .Where(r => r.CourseId == courseId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var feedbacks = await _uow.CourseFeedbacks.Query()
                .Include(f => f.User)
                .Where(f => f.CourseId == courseId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            var ratingDistribution = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
            {
                ratingDistribution[i] = reviews.Count(r => r.Rating == i);
            }

            return new ReviewSummaryDto
            {
                AverageRating = reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : null,
                TotalReviews = reviews.Count,
                RatingDistribution = ratingDistribution,
                RecentReviews = reviews.Take(5).Select(r => new ReviewItemDto
                {
                    ReviewId = r.CourseReviewId,
                    UserName = r.User.FullName,
                    Rating = r.Rating,
                    Comment = r.ReviewComment,
                    CreatedAt = r.CreatedAt
                }),
                RecentFeedbacks = feedbacks.Take(5).Select(f => new FeedbackItemDto
                {
                    FeedbackId = f.FeedbackId,
                    UserName = f.User.FullName,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    CreatedAt = f.CreatedAt
                })
            };
        }

        public async Task<IEnumerable<CourseEnrollmentDetailsDto>> GetCourseEnrollmentsAsync(int courseId, int pageNumber = 1, int pageSize = 20)
        {
            await GetCourseWithValidationAsync(courseId);

            var enrollments = await _uow.CourseEnrollments.Query()
                .Include(e => e.User)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Levels)
                        .ThenInclude(l => l.Sections)
                            .ThenInclude(s => s.Contents)
                .Where(e => e.CourseId == courseId)
                .OrderByDescending(e => e.EnrolledAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var enrollmentDetails = new List<CourseEnrollmentDetailsDto>();

            foreach (var enrollment in enrollments)
            {
                var progress = await _uow.UserProgresses.Query()
                    .Include(p => p.CurrentLevel)
                    .Include(p => p.CurrentSection)
                    .FirstOrDefaultAsync(p => p.UserId == enrollment.UserId && p.CourseId == courseId);

                var userPoints = await _uow.UserCoursePoints.Query()
                    .Where(p => p.UserId == enrollment.UserId && p.CourseId == courseId)
                    .FirstOrDefaultAsync();

                var hasReview = await _uow.CourseReviews.Query()
                    .AnyAsync(r => r.UserId == enrollment.UserId && r.CourseId == courseId);

                var lastActivity = await _uow.UserContentActivities.Query()
                    .Include(a => a.Content)
                        .ThenInclude(c => c.Section)
                            .ThenInclude(s => s.Level)
                    .Where(a => a.UserId == enrollment.UserId && a.Content.Section.Level.CourseId == courseId)
                    .OrderByDescending(a => a.StartTime)
                    .FirstOrDefaultAsync();

                // Calculate progress percentage
                var totalContents = enrollment.Course.Levels
                    .SelectMany(l => l.Sections)
                    .SelectMany(s => s.Contents)
                    .Count();

                var completedContents = await _uow.UserContentActivities.Query()
                    .Include(a => a.Content)
                        .ThenInclude(c => c.Section)
                            .ThenInclude(s => s.Level)
                    .Where(a => a.UserId == enrollment.UserId &&
                               a.Content.Section.Level.CourseId == courseId &&
                               a.EndTime.HasValue)
                    .CountAsync();

                var progressPercentage = totalContents > 0 ? (decimal)completedContents / totalContents * 100 : 0;

                enrollmentDetails.Add(new CourseEnrollmentDetailsDto
                {
                    EnrollmentId = enrollment.CourseEnrollmentId,
                    UserId = enrollment.UserId,
                    UserName = enrollment.User.FullName,
                    UserEmail = enrollment.User.EmailAddress,
                    EnrolledAt = enrollment.EnrolledAt,
                    CurrentLevelId = progress?.CurrentLevelId,
                    CurrentLevelName = progress?.CurrentLevel?.LevelName,
                    CurrentSectionId = progress?.CurrentSectionId,
                    CurrentSectionName = progress?.CurrentSection?.SectionName,
                    ProgressPercentage = progressPercentage,
                    LastActivity = lastActivity?.StartTime,
                    TotalPointsEarned = userPoints?.TotalPoints ?? 0,
                    HasReviewed = hasReview
                });
            }

            return enrollmentDetails;
        }

        public async Task<BulkActionResultDto> BulkCourseActionAsync(BulkCourseActionDto request)
        {
            var user = GetCurrentUser();
            if (!user.IsInRole("Admin"))
                throw new UnauthorizedAccessException("Admin access required for bulk operations.");

            var result = new BulkActionResultDto
            {
                ProcessedCourseIds = new List<int>(),
                Errors = new List<string>()
            };

            foreach (var courseId in request.CourseIds)
            {
                try
                {
                    var course = await _uow.Courses.Query()
                        .FirstOrDefaultAsync(c => c.CourseId == courseId && !c.IsDeleted);

                    if (course == null)
                    {
                        ((List<string>)result.Errors).Add($"Course {courseId} not found");
                        result.FailCount++;
                        continue;
                    }

                    switch (request.Action.ToLowerInvariant())
                    {
                        case "activate":
                            course.IsActive = true;
                            break;
                        case "deactivate":
                            course.IsActive = false;
                            break;
                        case "delete":
                            course.IsDeleted = true;
                            break;
                        default:
                            ((List<string>)result.Errors).Add($"Unknown action: {request.Action}");
                            result.FailCount++;
                            continue;
                    }

                    _uow.Courses.Update(course);
                    ((List<int>)result.ProcessedCourseIds).Add(courseId);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    ((List<string>)result.Errors).Add($"Error processing course {courseId}: {ex.Message}");
                    result.FailCount++;
                }
            }

            if (result.SuccessCount > 0)
            {
                await _uow.SaveAsync();
            }

            return result;
        }

        public async Task<IEnumerable<CourseCDto>> GetMyCoursesAsync(int pageNumber = 1, int pageSize = 10)
        {
            return await GetAllCoursesForInstructorAsync(null, pageNumber, pageSize);
        }

        public async Task<bool> IsInstructorOwnerOfCourseAsync(int courseId, int instructorId)
        {
            return await _uow.Courses.Query()
                .AnyAsync(c => c.CourseId == courseId && c.InstructorId == instructorId && !c.IsDeleted);
        }

        public async Task<IEnumerable<CourseCDto>> SearchCoursesAsync(string searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            var user = GetCurrentUser();
            if (!user.IsInRole("Admin"))
                throw new UnauthorizedAccessException("Admin access required.");

            return await GetAllCoursesForAdminAsync(pageNumber, pageSize, searchTerm);
        }

        public async Task TransferCourseOwnershipAsync(int courseId, int newInstructorId)
        {
            var user = GetCurrentUser();
            if (!user.IsInRole("Admin"))
                throw new UnauthorizedAccessException("Admin access required.");

            var course = await _uow.Courses.Query()
                .FirstOrDefaultAsync(c => c.CourseId == courseId && !c.IsDeleted);

            if (course == null)
                throw new KeyNotFoundException($"Course {courseId} not found.");

            var newInstructor = await _uow.Users.Query()
                .FirstOrDefaultAsync(u => u.UserId == newInstructorId &&
                                          (u.Role == UserRole.Instructor || u.Role == UserRole.Admin) &&
                                          !u.IsDeleted);

            if (newInstructor == null)
                throw new KeyNotFoundException("New instructor not found or invalid role.");

            course.InstructorId = newInstructorId;
            _uow.Courses.Update(course);
            await _uow.SaveAsync();
        }

        public async Task<bool> ValidateCourseAccessAsync(int courseId, int? requestingUserId = null)
        {
            var user = GetCurrentUser();
            var userId = requestingUserId ?? user.GetCurrentUserId();

            if (userId == null)
                return false;

            if (user.IsInRole("Admin"))
                return true;

            return await _uow.Courses.Query()
                .AnyAsync(c => c.CourseId == courseId && c.InstructorId == userId.Value && !c.IsDeleted);
        }

        // Helper Methods
        private ClaimsPrincipal GetCurrentUser()
        {
            return _httpContextAccessor.HttpContext?.User
                   ?? throw new InvalidOperationException("Unable to determine current user.");
        }

        private async Task<Course> GetCourseWithValidationAsync(int courseId)
        {
            var user = GetCurrentUser();
            var currentUserId = user.GetCurrentUserId();

            if (currentUserId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            var courseQuery = _uow.Courses.Query()
                .Include(c => c.Instructor)
                .Include(c => c.AboutCourses)
                .Include(c => c.CourseSkills)
                .Include(c => c.CourseReviews)
                .Include(c => c.Levels)
                .Where(c => c.CourseId == courseId && !c.IsDeleted);

            // Admin can access any course, instructor can only access their own
            if (!user.IsInRole("Admin"))
            {
                courseQuery = courseQuery.Where(c => c.InstructorId == currentUserId.Value);
            }

            var course = await courseQuery.FirstOrDefaultAsync();

            if (course == null)
                throw new KeyNotFoundException($"Course {courseId} not found or access denied.");

            return course;
        }

        private static string FormatText(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = input.Trim();
            input = Regex.Replace(input, @"\s+", " ");

            if (input.Length > 0)
                return char.ToUpper(input[0]) + input.Substring(1);

            return input;
        }
    }
}