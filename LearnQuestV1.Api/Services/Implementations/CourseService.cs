using LearnQuestV1.Api.DTOs.Browse;
using LearnQuestV1.Api.DTOs.Courses;
using LearnQuestV1.Api.DTOs.Instructor;
using LearnQuestV1.Api.DTOs.Progress;
using LearnQuestV1.Api.DTOs.Public;
using LearnQuestV1.Api.DTOs.Reviews;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Models.CourseStructure;
using LearnQuestV1.Core.Models.FeedbackAndReviews;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<CourseService> _logger;
        private readonly IMemoryCache _cache;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

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

        public CourseService(
            IUnitOfWork uow,
            ILogger<CourseService> logger,
            IMemoryCache cache,
            IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor)
        {
            _uow = uow;
            _logger = logger;
            _cache = cache;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        // =====================================================
        // NEW: PUBLIC COURSE BROWSING IMPLEMENTATIONS
        // =====================================================

        /// <summary>
        /// Browse courses with filtering and pagination for public access
        /// </summary>
        public async Task<PagedResult<PublicCourseDto>> BrowseCoursesAsync(CourseBrowseFilterDto filter)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                // Validate filter
                if (!filter.IsValid(out var errors))
                {
                    throw new ArgumentException($"Invalid filter parameters: {string.Join(", ", errors)}");
                }

                var query = _uow.Courses.Query()
                    .Include(c => c.Instructor)
                    .Include(c => c.CourseTrackCourses)
                        .ThenInclude(ctc => ctc.CourseTrack)
                    .Include(c => c.Levels)
                        .ThenInclude(l => l.Sections)
                            .ThenInclude(s => s.Contents)
                    .Include(c => c.CourseEnrollments)
                    .Include(c => c.CourseReviews)
                    .Where(c => !c.IsDeleted && c.IsActive && c.IsActive);

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.GetSanitizedSearchTerm()!.ToLower();
                    query = query.Where(c =>
                        c.CourseName.ToLower().Contains(searchTerm) ||
                        c.Description.ToLower().Contains(searchTerm) ||
                        c.Instructor.FullName.ToLower().Contains(searchTerm) ||
                        c.CourseTrackCourses.Any(ctc => ctc.CourseTrack.TrackName.ToLower().Contains(searchTerm))
                    );
                }

                // Apply filters
                if (filter.TrackId.HasValue)
                {
                    query = query.Where(c =>
                        c.CourseTrackCourses.Any(ctc => ctc.TrackId == filter.TrackId.Value));
                }

                if (filter.CategoryIds?.Any() == true)
                {
                    query = query.Where(c =>
                        c.CourseTrackCourses.Any(ctc => filter.CategoryIds.Contains(ctc.TrackId)));
                }


                if (filter.MinPrice.HasValue)
                    query = query.Where(c => c.CoursePrice >= filter.MinPrice.Value);

                if (filter.MaxPrice.HasValue)
                    query = query.Where(c => c.CoursePrice <= filter.MaxPrice.Value);


                if (filter.IsFree.HasValue)
                {
                    if (filter.IsFree.Value)
                        query = query.Where(c => c.CoursePrice == 0);
                    else
                        query = query.Where(c => c.CoursePrice > 0);
                }

                if (!string.IsNullOrWhiteSpace(filter.Level))
                {
                    if (Enum.TryParse<CourseLevelType>(filter.Level, true, out var parsedLevel))
                    {
                        query = query.Where(c => c.CourseLevel == parsedLevel);
                    }
                }

                if (filter.HasCertificate.HasValue)
                    query = query.Where(c => c.HasCertificate == filter.HasCertificate);

                if (filter.InstructorId.HasValue)
                    query = query.Where(c => c.InstructorId == filter.InstructorId);

                if (filter.IsFeatured.HasValue)
                    query = query.Where(c => c.IsFeatured == filter.IsFeatured);

                if (filter.IsNew.HasValue && filter.IsNew.Value)
                    query = query.Where(c => c.CreatedAt >= DateTime.UtcNow.AddDays(-30));

                if (filter.MinRating.HasValue)
                {
                    var minRating = (double)filter.MinRating.Value;

                    query = query.Where(c =>
                        c.CourseReviews.Any() &&
                        c.CourseReviews.Average(r => (double)r.Rating) >= minRating);
                }

                // Apply duration filters
                if (filter.MinDuration.HasValue || filter.MaxDuration.HasValue)
                {
                    query = query.Where(c => c.Levels.Any(l => l.Sections.Any(s => s.Contents.Any())));

                    if (filter.MinDuration.HasValue)
                    {
                        query = query.Where(c =>
                            c.Levels.SelectMany(l => l.Sections)
                                   .SelectMany(s => s.Contents)
                                   .Sum(ct => ct.DurationInMinutes) >= filter.MinDuration);
                    }

                    if (filter.MaxDuration.HasValue)
                    {
                        query = query.Where(c =>
                            c.Levels.SelectMany(l => l.Sections)
                                   .SelectMany(s => s.Contents)
                                   .Sum(ct => ct.DurationInMinutes) <= filter.MaxDuration);
                    }
                }

                // Apply sorting
                query = ApplySorting(query, filter.GetNormalizedSortBy());

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination
                var courses = await query
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                // Map to DTOs
                var courseDtos = courses.Select(MapToPublicCourseDto).ToList();

                stopwatch.Stop();

                var result = PagedResult<PublicCourseDto>.Create(
                    courseDtos, totalCount, filter.PageNumber, filter.PageSize);

                result.AppliedFilters = filter;
                result.SearchMetadata = new SearchMetadataDto
                {
                    SearchTerm = filter.SearchTerm,
                    SearchResultCount = totalCount,
                    SearchDurationMs = stopwatch.ElapsedMilliseconds
                };

                _logger.LogInformation("Course browse completed: {SearchTerm}, Results: {Count}, Duration: {Duration}ms",
                    filter.SearchTerm, totalCount, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error browsing courses with filter: {@Filter}", filter);
                throw;
            }
        }

        /// <summary>
        /// Get detailed course information for public viewing
        /// </summary>
        public async Task<PublicCourseDetailsDto?> GetPublicCourseDetailsAsync(int courseId)
        {
            try
            {
                var cacheKey = $"public_course_details_{courseId}";

                if (_cache.TryGetValue(cacheKey, out PublicCourseDetailsDto? cachedDetails))
                {
                    return cachedDetails;
                }

                var course = await _uow.Courses.Query()
                    .Include(c => c.Instructor)
                    .Include(c => c.CourseTrackCourses)
                        .ThenInclude(ctc => ctc.CourseTrack)
                    .Include(c => c.Levels.Where(l => !l.IsDeleted && l.IsVisible))
                        .ThenInclude(l => l.Sections.Where(s => !s.IsDeleted && s.IsVisible))
                            .ThenInclude(s => s.Contents.Where(ct => !ct.IsDeleted && ct.IsVisible))
                    .Include(c => c.CourseEnrollments)
                    .Include(c => c.CourseReviews.Where(r => !r.IsDeleted))
                        .ThenInclude(r => r.User)
                    .Include(c => c.CourseFeedbacks)
                    .FirstOrDefaultAsync(c => c.CourseId == courseId && !c.IsDeleted && c.IsActive );

                if (course == null)
                {
                    return null;
                }

                var details = await MapToPublicCourseDetailsDto(course);



                // Cache for 30 minutes
                _cache.Set(cacheKey, details, TimeSpan.FromMinutes(30));

                _logger.LogInformation("Public course details retrieved for course {CourseId}", courseId);
                return details;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving public course details for course {CourseId}", courseId);
                throw;
            }
        }

        /// <summary>
        /// Get featured courses for homepage
        /// </summary>
        public async Task<List<PublicCourseDto>> GetFeaturedCoursesAsync(int limit = 6)
        {
            try
            {
                var cacheKey = $"featured_courses_{limit}";

                if (_cache.TryGetValue(cacheKey, out List<PublicCourseDto>? cachedCourses))
                {
                    return cachedCourses!;
                }

                var courses = await _uow.Courses.Query()
                    .Include(c => c.Instructor)
                    .Include(c => c.CourseTrackCourses)
                        .ThenInclude(ctc => ctc.CourseTrack)
                    .Include(c => c.Levels)
                        .ThenInclude(l => l.Sections)
                            .ThenInclude(s => s.Contents)
                    .Include(c => c.CourseEnrollments)
                    .Include(c => c.CourseReviews)
                    .Where(c => !c.IsDeleted && c.IsActive && c.IsActive && c.IsFeatured)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(limit)
                    .ToListAsync();

                var featuredCourses = courses.Select(MapToPublicCourseDto).ToList();

                // Cache for 1 hour
                _cache.Set(cacheKey, featuredCourses, TimeSpan.FromHours(1));

                return featuredCourses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving featured courses");
                throw;
            }
        }

        /// <summary>
        /// Get most popular courses based on enrollment and ratings
        /// </summary>
        public async Task<List<PublicCourseDto>> GetPopularCoursesAsync(int limit = 6)
        {
            try
            {
                var cacheKey = $"popular_courses_{limit}";

                if (_cache.TryGetValue(cacheKey, out List<PublicCourseDto>? cachedCourses))
                {
                    return cachedCourses!;
                }

                var courses = await _uow.Courses.Query()
                    .Include(c => c.Instructor)
                    .Include(c => c.CourseTrackCourses)
                        .ThenInclude(ctc => ctc.CourseTrack)
                    .Include(c => c.Levels)
                        .ThenInclude(l => l.Sections)
                            .ThenInclude(s => s.Contents)
                    .Include(c => c.CourseEnrollments)
                    .Include(c => c.CourseReviews)
                    .Where(c => !c.IsDeleted && c.IsActive && c.IsActive)
                    .OrderByDescending(c => c.CourseEnrollments.Count)
                    .ThenByDescending(c => c.CourseReviews.Any() ? c.CourseReviews.Average(r => r.Rating) : 0)
                    .Take(limit)
                    .ToListAsync();

                var popularCourses = courses.Select(MapToPublicCourseDto).ToList();

                // Cache for 2 hours
                _cache.Set(cacheKey, popularCourses, TimeSpan.FromHours(2));

                return popularCourses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving popular courses");
                throw;
            }
        }

        /// <summary>
        /// Get course recommendations for a user
        /// </summary>
        public async Task<List<PublicCourseDto>> GetRecommendedCoursesAsync(int userId, int limit = 6)
        {
            try
            {
                var cacheKey = $"recommended_courses_{userId}_{limit}";

                if (_cache.TryGetValue(cacheKey, out List<PublicCourseDto>? cachedRecommendations))
                {
                    return cachedRecommendations!;
                }

                // Get user's enrolled courses and their tracks
                var userEnrollments = await _uow.CourseEnrollments.Query()
                    .Include(e => e.Course)
                        .ThenInclude(c => c.CourseTrackCourses)
                            .ThenInclude(ctc => ctc.CourseTrack)
                    .Where(e => e.UserId == userId)
                    .ToListAsync();

                var userTracks = userEnrollments
                    .SelectMany(e => e.Course.CourseTrackCourses)
                    .Select(ctc => ctc.TrackId)
                    .Distinct()
                    .ToList();

                // Get recommendations based on user's learning history
                var enrolledCourseIds = userEnrollments.Select(e => e.CourseId).ToHashSet();

                var query = _uow.Courses.Query()
                    .Include(c => c.Instructor)
                    .Include(c => c.CourseTrackCourses)
                        .ThenInclude(ctc => ctc.CourseTrack)
                    .Include(c => c.Levels)
                        .ThenInclude(l => l.Sections)
                            .ThenInclude(s => s.Contents)
                    .Include(c => c.CourseEnrollments)
                    .Include(c => c.CourseReviews)
                    .Where(c => !c.IsDeleted && c.IsActive && !enrolledCourseIds.Contains(c.CourseId));

                if (userTracks.Any())
                {
                    query = query
                        .Where(c => c.CourseTrackCourses.Any(ctc => userTracks.Contains(ctc.TrackId)))
                        .OrderByDescending(c => c.CourseEnrollments.Count)
                        .ThenByDescending(c => c.CourseReviews.Any() ? c.CourseReviews.Average(r => r.Rating) : 0);
                }
                else
                {
                    query = query.OrderByDescending(c => c.CourseEnrollments.Count)
                                 .ThenByDescending(c => c.CourseReviews.Any() ? c.CourseReviews.Average(r => r.Rating) : 0);
                }


                var recommendations = await query.Take(limit).ToListAsync();
                var recommendationDtos = recommendations.Select(MapToPublicCourseDto).ToList();

                // Cache for 6 hours
                _cache.Set(cacheKey, recommendationDtos, TimeSpan.FromHours(6));

                return recommendationDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course recommendations for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get all course categories/tracks
        /// </summary>
        public async Task<List<CourseCategoryDto>> GetCourseCategoriesAsync()
        {
            try
            {
                var cacheKey = "course_categories_v1";

                if (_cache.TryGetValue(cacheKey, out List<CourseCategoryDto>? cachedCategories))
                    return cachedCategories!;

                var tracks = await _uow.CourseTracks.Query()
                    .Include(t => t.CourseTrackCourses)
                        .ThenInclude(ctc => ctc.Course)
                    .Where(t => t.CourseTrackCourses.Any(ctc => !ctc.Course.IsDeleted && ctc.Course.IsActive))
                    .OrderBy(t => t.TrackName)
                    .ToListAsync();

                var categories = tracks.Select(t => new CourseCategoryDto
                {
                    CategoryId = t.TrackId,
                    CategoryName = t.TrackName,
                    CategoryDescription = t.TrackDescription,
                    CourseCount = t.CourseTrackCourses.Count(ctc => !ctc.Course.IsDeleted && ctc.Course.IsActive),
                    IsActive = true, // Based on filtered condition
                    DisplayOrder = 0 // You can optionally add this to Track entity later
                }).ToList();

                _cache.Set(cacheKey, categories, TimeSpan.FromHours(4));
                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course categories");
                throw;
            }
        }

        /// <summary>
        /// Get all tracks (moved from ProgressController)
        /// </summary>
        public async Task<IEnumerable<TrackDto>> GetAllTracksAsync()
        {
            try
            {
                const string cacheKey = "all_tracks_v1";

                if (_cache.TryGetValue(cacheKey, out IEnumerable<TrackDto> cachedTracks))
                {
                    return cachedTracks;
                }

                var tracks = await _uow.CourseTracks.Query()
                    .OrderBy(t => t.TrackName)
                    .ToListAsync();

                var result = tracks.Select(t => new TrackDto
                {
                    TrackId = t.TrackId,
                    TrackName = t.TrackName,
                    TrackDescription = t.TrackDescription ?? string.Empty
                }).ToList();

                _cache.Set(cacheKey, result, TimeSpan.FromHours(4));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all tracks");
                throw;
            }
        }


        /// <summary>
        /// Get courses in a specific track (moved from ProgressController)
        /// </summary>
        public async Task<TrackCoursesDto> GetCoursesInTrackAsync(int trackId)
        {
            try
            {
                var track = await _uow.CourseTracks.Query()
                    .Include(t => t.CourseTrackCourses)
                        .ThenInclude(ctc => ctc.Course)
                            .ThenInclude(c => c.Instructor)
                    .FirstOrDefaultAsync(t => t.TrackId == trackId );

                if (track == null)
                    throw new KeyNotFoundException($"Track with ID {trackId} not found");

                var courses = track.CourseTrackCourses
                    .Where(ctc => !ctc.Course.IsDeleted && ctc.Course.IsActive)
                    .Select(ctc => new CourseInTrackDto
                    {
                        CourseId = ctc.Course.CourseId,
                        CourseName = ctc.Course.CourseName,
                        Description = ctc.Course.Description,
                        CourseImage = ctc.Course.CourseImage!,
                        CoursePrice = ctc.Course.CoursePrice,
                        InstructorName = ctc.Course.Instructor.FullName,
                        CreatedAt = ctc.Course.CreatedAt
                    }).ToList();

                return new TrackCoursesDto
                {
                    TrackId = track.TrackId,
                    TrackName = track.TrackName,
                    TrackDescription = track.TrackDescription ?? string.Empty,
                    TotalCourses = courses.Count,
                    Courses = courses
                };
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                _logger.LogError(ex, "Error retrieving courses for track {TrackId}", trackId);
                throw;
            }
        }


        /// <summary>
        /// Check if user is enrolled in course
        /// </summary>
        public async Task<bool> IsUserEnrolledAsync(int userId, int courseId)
        {
            try
            {
                return await _uow.CourseEnrollments.Query()
                    .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking enrollment for user {UserId} in course {CourseId}", userId, courseId);
                return false;
            }
        }

        /// <summary>
        /// Get public course statistics
        /// </summary>
        public async Task<CoursePublicStatsDto> GetCoursePublicStatsAsync(int courseId)
        {
            try
            {
                var course = await _uow.Courses.Query()
                    .Include(c => c.CourseEnrollments)
                    .Include(c => c.CourseReviews)
                    .FirstOrDefaultAsync(c => c.CourseId == courseId && !c.IsDeleted);

                if (course == null)
                    throw new KeyNotFoundException($"Course {courseId} not found");

                var completedStudents = await _uow.UserProgresses.Query()
                    .CountAsync(p => p.CourseId == courseId && p.CurrentLevelId == course.Levels.OrderByDescending(l => l.LevelOrder).FirstOrDefault().LevelId);

                var recentCutoff = DateTime.UtcNow.AddDays(-30);

                var activeStudents = await _uow.UserContentActivities.Query()
                    .Where(a => a.Content.Section.Level.CourseId == courseId && a.EndTime != null && a.EndTime >= recentCutoff)
                    .Select(a => a.UserId)
                    .Distinct()
                    .CountAsync();

                var totalEnrollments = course.CourseEnrollments.Count;
                var totalReviews = course.CourseReviews.Count;
                var averageRating = totalReviews > 0
                    ? course.CourseReviews.Average(r => (decimal)r.Rating)
                    : 0;

                return new CoursePublicStatsDto
                {
                    CourseId = courseId,
                    TotalEnrollments = totalEnrollments,
                    ActiveStudents = activeStudents,
                    CompletedStudents = completedStudents,
                    CompletionRate = totalEnrollments > 0
                        ? Math.Round((decimal)completedStudents / totalEnrollments * 100, 2)
                        : 0,
                    AverageRating = averageRating,
                    TotalReviews = totalReviews,
                    LastUpdated = DateTime.UtcNow
                };
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                _logger.LogError(ex, "Error retrieving public stats for course {CourseId}", courseId);
                throw;
            }
        }

        /// <summary>
        /// Get course filter options
        /// </summary>
        public async Task<CourseFilterOptionsDto> GetCourseFilterOptionsAsync()
        {
            try
            {
                var cacheKey = "course_filter_options_v1";

                if (_cache.TryGetValue(cacheKey, out CourseFilterOptionsDto? cachedOptions))
                    return cachedOptions!;

                // 1. Categories
                var categories = await GetCourseCategoriesAsync();

                // 2. Price Statistics
                var priceStats = await _uow.Courses.Query()
                    .Where(c => !c.IsDeleted && c.IsActive)
                    .GroupBy(c => 1)
                    .Select(g => new
                    {
                        MinPrice = g.Min(c => c.CoursePrice),
                        MaxPrice = g.Max(c => c.CoursePrice),
                        AveragePrice = g.Average(c => c.CoursePrice),
                        FreeCourseCount = g.Count(c => c.CoursePrice == 0),
                        PaidCourseCount = g.Count(c => c.CoursePrice > 0)
                    })
                    .FirstOrDefaultAsync();

                // 3. Popular Instructors
                var instructors = await _uow.Courses.Query()
                    .Where(c => !c.IsDeleted && c.IsActive)
                    .GroupBy(c => new { c.InstructorId, c.Instructor.FullName })
                    .Select(g => new InstructorFilterDto
                    {
                        InstructorId = g.Key.InstructorId,
                        Name = g.Key.FullName,
                        CourseCount = g.Count(),
                        AverageRating = (decimal)g.SelectMany(c => c.CourseReviews).Average(r => r.Rating)

                    })
                    .OrderByDescending(i => i.CourseCount)
                    .Take(10)
                    .ToListAsync();

                // Assemble filter options
                var options = new CourseFilterOptionsDto
                {
                    Categories = categories,
                    PopularInstructors = instructors,
                    PriceRange = new PriceRangeDto
                    {
                        MinPrice = priceStats?.MinPrice ?? 0,
                        MaxPrice = priceStats?.MaxPrice ?? 0,
                        AveragePrice = priceStats?.AveragePrice ?? 0,
                        FreeCourseCount = priceStats?.FreeCourseCount ?? 0,
                        PaidCourseCount = priceStats?.PaidCourseCount ?? 0
                    }
                };

                // Cache result
                _cache.Set(cacheKey, options, TimeSpan.FromHours(2));

                return options;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course filter options");
                throw;
            }
        }

        /// <summary>
        /// Advanced course search
        /// </summary>
        public async Task<PagedResult<PublicCourseDto>> SearchCoursesAdvancedAsync(string searchTerm, int limit = 20)
        {
            var filter = new CourseBrowseFilterDto
            {
                SearchTerm = searchTerm,
                PageSize = limit,
                PageNumber = 1,
                SortBy = "popular"
            };

            return await BrowseCoursesAsync(filter);
        }

        /// <summary>
        /// Get related courses
        /// </summary>
        public async Task<List<PublicCourseDto>> GetRelatedCoursesAsync(int courseId, int limit = 6)
        {
            try
            {
                var course = await _uow.Courses.Query()
                    .Include(c => c.CourseTrackCourses)
                    .FirstOrDefaultAsync(c => c.CourseId == courseId);

                if (course == null)
                    return new List<PublicCourseDto>();

                var relatedTrackIds = course.CourseTrackCourses.Select(ctc => ctc.TrackId).ToList();
                var instructorId = course.InstructorId;

                var relatedCourses = await _uow.Courses.Query()
                    .Include(c => c.Instructor)
                    .Include(c => c.CourseTrackCourses).ThenInclude(ctc => ctc.CourseTrack)
                    .Include(c => c.Levels).ThenInclude(l => l.Sections).ThenInclude(s => s.Contents)
                    .Include(c => c.CourseEnrollments)
                    .Include(c => c.CourseReviews)
                    .Where(c => !c.IsDeleted && c.IsActive &&
                                c.CourseId != courseId &&
                                (c.InstructorId == instructorId ||
                                 c.CourseTrackCourses.Any(ctc => relatedTrackIds.Contains(ctc.TrackId))))
                    .OrderByDescending(c => c.CourseEnrollments.Count)
                    .Take(limit)
                    .ToListAsync();

                return relatedCourses.Select(MapToPublicCourseDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting related courses for course {CourseId}", courseId);
                return new List<PublicCourseDto>();
            }
        }


        /// <summary>
        /// Get courses by instructor
        /// </summary>
        public async Task<List<PublicCourseDto>> GetCoursesByInstructorAsync(int instructorId, int limit = 10)
        {
            try
            {
                var courses = await _uow.Courses.Query()
                    .Include(c => c.Instructor)
                    .Include(c => c.CourseTrackCourses)
                        .ThenInclude(ctc => ctc.CourseTrack)
                    .Include(c => c.Levels)
                        .ThenInclude(l => l.Sections)
                            .ThenInclude(s => s.Contents)
                    .Include(c => c.CourseEnrollments)
                    .Include(c => c.CourseReviews)
                    .Where(c => !c.IsDeleted && c.IsActive && c.InstructorId == instructorId)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(limit)
                    .ToListAsync();

                return courses.Select(MapToPublicCourseDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courses by instructor {InstructorId}", instructorId);
                return new List<PublicCourseDto>();
            }
        }

        public async Task<CourseReviewsDto> GetCourseReviewsAsync(int courseId, int page = 1, int pageSize = 10, int? currentUserId = null)
        {
            var course = await _uow.Courses.Query()
                .Include(c => c.CourseReviews.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.CourseId == courseId && !c.IsDeleted);

            if (course == null)
                throw new KeyNotFoundException($"Course {courseId} not found");

            var reviews = course.CourseReviews
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewDetailsDto
                {
                    ReviewId = r.CourseReviewId,
                    UserId = r.UserId,
                    UserName = r.User.FullName,
                    UserEmail = r.User.EmailAddress,
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    Rating = r.Rating,
                    ReviewComment = r.ReviewComment ?? string.Empty,
                    CreatedAt = r.CreatedAt,
                    CanEdit = currentUserId.HasValue && currentUserId == r.UserId,
                    CanDelete = currentUserId.HasValue && currentUserId == r.UserId
                })
                .ToList();

            var ratingDistribution = course.CourseReviews
                .GroupBy(r => r.Rating)
                .ToDictionary(g => g.Key, g => g.Count());

            return new CourseReviewsDto
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                AverageRating = course.CourseReviews.Any()
                    ? (decimal?)course.CourseReviews.Average(r => r.Rating)
                    : null,
                TotalReviews = course.CourseReviews.Count,
                RatingDistribution = ratingDistribution,
                Reviews = reviews,
                Pagination = new PaginationInfoDto
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalItems = course.CourseReviews.Count
                }
            };
        }


        // =====================================================
        // HELPER METHODS FOR MAPPING AND SORTING
        // =====================================================

        private IQueryable<Course> ApplySorting(IQueryable<Course> query, string sortBy)
        {
            return sortBy switch
            {
                "oldest" => query.OrderBy(c => c.CreatedAt),
                "popular" => query.OrderByDescending(c => c.CourseEnrollments.Count),
                "rating" => query.OrderByDescending(c => c.CourseReviews.Any() ? c.CourseReviews.Average(r => r.Rating) : 0),
                "price_low" => query.OrderBy(c => c.CoursePrice),
                "price_high" => query.OrderByDescending(c => c.CoursePrice),
                "name" => query.OrderBy(c => c.CourseName),
                "duration" => query.OrderByDescending(c =>
                    c.Levels.SelectMany(l => l.Sections)
                           .SelectMany(s => s.Contents)
                           .Sum(ct => ct.DurationInMinutes)),
                _ => query.OrderByDescending(c => c.CreatedAt) // newest (default)
            };
        }

        private PublicCourseDto MapToPublicCourseDto(Course course)
        {
            var totalDuration = course.Levels
                .SelectMany(l => l.Sections)
                .SelectMany(s => s.Contents)
                .Sum(c => c.DurationInMinutes);

            var averageRating = course.CourseReviews.Any()
                ? course.CourseReviews.Average(r => r.Rating)
                : 0;

            return new PublicCourseDto
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                CourseDescription = course.Description,
                CourseImage = course.CourseImage,
                Price = course.CoursePrice,
                InstructorId = course.InstructorId,
                InstructorName = course.Instructor.FullName,
                InstructorImage = course.Instructor.ProfilePhoto,
                EnrollmentCount = course.CourseEnrollments.Count,
                AverageRating = course.CourseReviews.Any() ? (decimal)course.CourseReviews.Average(r => r.Rating) : 0,
                ReviewCount = course.CourseReviews.Count,
                TotalLevels = course.Levels.Count,
                TotalSections = course.Levels.SelectMany(l => l.Sections).Count(),
                TotalContents = course.Levels.SelectMany(l => l.Sections).SelectMany(s => s.Contents).Count(),
                EstimatedDurationMinutes = course.Levels.SelectMany(l => l.Sections).SelectMany(s => s.Contents).Sum(c => c.DurationInMinutes),
                CourseLevel = course.CourseLevel.ToString(),
                CreatedAt = course.CreatedAt,
                LastUpdated = course.Levels.SelectMany(l => l.Sections).SelectMany(s => s.Contents).Max(c => (DateTime?)c.CreatedAt) ?? course.CreatedAt,
                TrackId = course.CourseTrackCourses.FirstOrDefault()?.TrackId,
                TrackName = course.CourseTrackCourses.FirstOrDefault()?.CourseTrack?.TrackName,
                TrackDescription = course.CourseTrackCourses.FirstOrDefault()?.CourseTrack?.TrackDescription,
                HasCertificate = course.HasCertificate,
                IsFeatured = course.IsFeatured,
                IsNew = course.CreatedAt >= DateTime.UtcNow.AddDays(-30),
                Language = "English" // مؤقتًا ثابت، ممكن نربطه لاحقًا
            };

        }

        private async Task<PublicCourseDetailsDto> MapToPublicCourseDetailsDto(Course course)
        {
            var publicCourse = MapToPublicCourseDto(course);

            var details = new PublicCourseDetailsDto
            {
                CourseId = publicCourse.CourseId,
                CourseName = publicCourse.CourseName,
                CourseDescription = publicCourse.CourseDescription,
                CourseImage = publicCourse.CourseImage,
                Price = publicCourse.Price,
                InstructorId = publicCourse.InstructorId,
                InstructorName = publicCourse.InstructorName,
                InstructorImage = publicCourse.InstructorImage,
                EnrollmentCount = publicCourse.EnrollmentCount,
                AverageRating = publicCourse.AverageRating,
                ReviewCount = publicCourse.ReviewCount,
                TotalLevels = publicCourse.TotalLevels,
                TotalSections = publicCourse.TotalSections,
                TotalContents = publicCourse.TotalContents,
                EstimatedDurationMinutes = publicCourse.EstimatedDurationMinutes,
                CourseLevel = publicCourse.CourseLevel,
                PrerequisiteSkills = publicCourse.PrerequisiteSkills,
                CreatedAt = publicCourse.CreatedAt,
                LastUpdated = publicCourse.LastUpdated,
                TrackId = publicCourse.TrackId,
                TrackName = publicCourse.TrackName,
                TrackDescription = publicCourse.TrackDescription,
                HasCertificate = publicCourse.HasCertificate,
                HasExams = publicCourse.HasExams,
                HasProjects = publicCourse.HasProjects,
                HasDownloadableResources = publicCourse.HasDownloadableResources,
                IsActive = publicCourse.IsActive,
                IsFeatured = publicCourse.IsFeatured,
                IsNew = publicCourse.IsNew,
                Language = publicCourse.Language,
                Tags = publicCourse.Tags,

                // Map levels with preview
                Levels = course.Levels
                    .OrderBy(l => l.LevelOrder)
                    .Select(MapToPublicLevelDto)
                    .ToList(),

                // Map instructor details
                Instructor = await MapToPublicInstructorDto(course.Instructor),

                //// Map recent reviews
                //RecentReviews = course.CourseReviews
                //    .OrderByDescending(r => r.CreatedAt)
                //    .Take(5)
                //    .Select(r => MapToPublicReviewDto(r))
                //    .ToList(),

                // Get related courses
                RelatedCourses = await GetRelatedCoursesAsync(course.CourseId, 4),

                // Course metadata
                CourseStatus = course.IsActive ? "Active" : "Inactive",
                HasLifetimeAccess = true,
                HasMobileAccess = true,
                HasDiscussion = true
            };

            return details;
        }

        private PublicLevelDto MapToPublicLevelDto(Level level)
        {
            return new PublicLevelDto
            {
                LevelId = level.LevelId,
                LevelName = level.LevelName,
                LevelDetails = level.LevelDetails,
                LevelOrder = level.LevelOrder,
                SectionCount = level.Sections.Count,
                ContentCount = level.Sections.SelectMany(s => s.Contents).Count(),
                EstimatedDurationMinutes = level.Sections.SelectMany(s => s.Contents).Sum(c => c.DurationInMinutes),
                IsPreviewAvailable = false, // placeholder
                LevelObjective = "", // placeholder
                PreviewSections = level.Sections
                    .OrderBy(s => s.SectionOrder)
                    .Take(3)
                    .Select(MapToPublicSectionDto)
                    .ToList()
            };
        }

        private PublicSectionDto MapToPublicSectionDto(Section section)
        {
            return new PublicSectionDto
            {
                SectionId = section.SectionId,
                SectionName = section.SectionName,
                SectionOrder = section.SectionOrder,
                ContentCount = section.Contents.Count,
                EstimatedDurationMinutes = section.Contents.Sum(c => c.DurationInMinutes),
                HasFreePreview = false, // placeholder
                SectionObjective = "", // placeholder
                ContentPreviews = section.Contents
                    .OrderBy(c => c.ContentOrder)
                    .Select(MapToPublicContentPreviewDto)
                    .ToList()
            };
        }

        private PublicContentPreviewDto MapToPublicContentPreviewDto(Content content)
        {
            return new PublicContentPreviewDto
            {
                ContentId = content.ContentId,
                Title = content.Title,
                ContentType = content.ContentType.ToString(),
                DurationInMinutes = content.DurationInMinutes,
            };
        }

        private async Task<PublicInstructorDto> MapToPublicInstructorDto(User instructor)
        {
            var instructorCourses = await _uow.Courses.Query()
                .Include(c => c.CourseEnrollments)
                .Include(c => c.CourseReviews)
                .Where(c => c.InstructorId == instructor.UserId && !c.IsDeleted)
                .ToListAsync();

            var totalStudents = instructorCourses.SelectMany(c => c.CourseEnrollments).Count();
            var allReviews = instructorCourses.SelectMany(c => c.CourseReviews).ToList();

            return new PublicInstructorDto
            {
                InstructorId = instructor.UserId,
                FullName = instructor.FullName,
                ProfilePhoto = instructor.ProfilePhoto,
                TotalCourses = instructorCourses.Count,
                TotalStudents = totalStudents,
                AverageRating = allReviews.Any()
                    ? (decimal)allReviews.Average(r => r.Rating)
                    : 0,
                TotalReviews = allReviews.Count
            };
        }

        private ReviewDetailsDto MapToPublicReviewDto(CourseReview review, int? currentUserId = null)
        {
            return new ReviewDetailsDto
            {
                ReviewId = review.CourseReviewId,
                UserId = review.UserId,
                UserName = review.User.FullName,
                UserEmail = review.User.EmailAddress,
                CourseId = review.CourseId,
                CourseName = review.Course.CourseName,
                Rating = review.Rating,
                ReviewComment = review.ReviewComment ?? string.Empty,
                CreatedAt = review.CreatedAt,
                CanEdit = currentUserId.HasValue && currentUserId == review.UserId,
                CanDelete = currentUserId.HasValue && currentUserId == review.UserId
            };
        }


        private List<string> ParseCommaSeparatedString(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new List<string>();

            return input.Split(',', StringSplitOptions.RemoveEmptyEntries)
                       .Select(s => s.Trim())
                       .Where(s => !string.IsNullOrEmpty(s))
                       .ToList();
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