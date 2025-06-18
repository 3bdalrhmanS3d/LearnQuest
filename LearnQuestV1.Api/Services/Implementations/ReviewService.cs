using LearnQuestV1.Api.DTOs.Reviews;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models.FeedbackAndReviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LearnQuestV1.Api.Services.Implementations
{
    /// <summary>
    /// Service for managing course reviews
    /// </summary>
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(IUnitOfWork uow, ILogger<ReviewService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        // =====================================================
        // User Review Operations
        // =====================================================

        public async Task<ReviewDetailsDto> CreateReviewAsync(int userId, CreateReviewDto createReviewDto)
        {
            _logger.LogInformation("Creating review for user {UserId} on course {CourseId}", userId, createReviewDto.CourseId);

            // Validate user can review the course
            var validation = await ValidateUserCanReviewCourseAsync(userId, createReviewDto.CourseId);
            if (!validation.IsValid)
            {
                var errors = string.Join(", ", validation.ValidationErrors);
                throw new InvalidOperationException($"Cannot create review: {errors}");
            }

            var review = new CourseReview
            {
                UserId = userId,
                CourseId = createReviewDto.CourseId,
                Rating = createReviewDto.Rating,
                ReviewComment = createReviewDto.ReviewComment,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.CourseReviews.AddAsync(review);
            await _uow.SaveAsync();

            _logger.LogInformation("Review created successfully with ID {ReviewId}", review.CourseReviewId);
            return await GetReviewByIdAsync(review.CourseReviewId, userId);
        }

        public async Task<ReviewDetailsDto> UpdateReviewAsync(int userId, int reviewId, UpdateReviewDto updateReviewDto)
        {
            _logger.LogInformation("Updating review {ReviewId} by user {UserId}", reviewId, userId);

            var review = await _uow.CourseReviews.Query()
                .FirstOrDefaultAsync(r => r.CourseReviewId == reviewId && r.UserId == userId);

            if (review == null)
            {
                throw new KeyNotFoundException("Review not found or you don't have permission to update it");
            }

            review.Rating = updateReviewDto.Rating;
            review.ReviewComment = updateReviewDto.ReviewComment;

            await _uow.CourseReviews.UpdateAsync(review);
            await _uow.SaveAsync();

            _logger.LogInformation("Review {ReviewId} updated successfully", reviewId);
            return await GetReviewByIdAsync(reviewId, userId);
        }

        public async Task<bool> DeleteReviewAsync(int userId, int reviewId)
        {
            _logger.LogInformation("Deleting review {ReviewId} by user {UserId}", reviewId, userId);

            var review = await _uow.CourseReviews.Query()
                .FirstOrDefaultAsync(r => r.CourseReviewId == reviewId && r.UserId == userId);

            if (review == null)
            {
                throw new KeyNotFoundException("Review not found or you don't have permission to delete it");
            }

            await _uow.CourseReviews.DeleteAsync(review);
            await _uow.SaveAsync();

            _logger.LogInformation("Review {ReviewId} deleted successfully", reviewId);
            return true;
        }

        public async Task<ReviewDetailsDto> GetReviewByIdAsync(int reviewId, int? requestingUserId = null)
        {
            var review = await _uow.CourseReviews.Query()
                .Include(r => r.User)
                .Include(r => r.Course)
                .FirstOrDefaultAsync(r => r.CourseReviewId == reviewId);

            if (review == null)
            {
                throw new KeyNotFoundException("Review not found");
            }

            return new ReviewDetailsDto
            {
                ReviewId = review.CourseReviewId,
                UserId = review.UserId,
                UserName = review.User.FullName,
                UserEmail = review.User.EmailAddress,
                CourseId = review.CourseId,
                CourseName = review.Course.CourseName,
                Rating = review.Rating,
                ReviewComment = review.ReviewComment,
                CreatedAt = review.CreatedAt,
                CanEdit = requestingUserId == review.UserId,
                CanDelete = requestingUserId == review.UserId
            };
        }

        // =====================================================
        // Course Reviews Operations
        // =====================================================

        public async Task<CourseReviewsDto> GetCourseReviewsAsync(int courseId, int pageNumber = 1, int pageSize = 20,
            string sortBy = "CreatedAt", string sortOrder = "desc")
        {
            _logger.LogInformation("Getting reviews for course {CourseId}, page {PageNumber}", courseId, pageNumber);

            var query = _uow.CourseReviews.Query()
                .Include(r => r.User)
                .Include(r => r.Course)
                .Where(r => r.CourseId == courseId);

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "rating" => sortOrder.ToLower() == "asc" ? query.OrderBy(r => r.Rating) : query.OrderByDescending(r => r.Rating),
                "username" => sortOrder.ToLower() == "asc" ? query.OrderBy(r => r.User.FullName) : query.OrderByDescending(r => r.User.FullName),
                _ => sortOrder.ToLower() == "asc" ? query.OrderBy(r => r.CreatedAt) : query.OrderByDescending(r => r.CreatedAt)
            };

            var totalReviews = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalReviews / pageSize);

            var reviews = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var course = await _uow.Courses.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new KeyNotFoundException("Course not found");
            }

            // Calculate rating distribution
            var allReviews = await _uow.CourseReviews.Query()
                .Where(r => r.CourseId == courseId)
                .ToListAsync();

            var ratingDistribution = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
            {
                ratingDistribution[i] = allReviews.Count(r => r.Rating == i);
            }

            return new CourseReviewsDto
            {
                CourseId = courseId,
                CourseName = course.CourseName,
                AverageRating = allReviews.Any() ? (decimal)allReviews.Average(r => r.Rating) : null,
                TotalReviews = totalReviews,
                RatingDistribution = ratingDistribution,
                Reviews = reviews.Select(r => new ReviewDetailsDto
                {
                    ReviewId = r.CourseReviewId,
                    UserId = r.UserId,
                    UserName = r.User.FullName,
                    UserEmail = r.User.EmailAddress,
                    CourseId = r.CourseId,
                    CourseName = r.Course.CourseName,
                    Rating = r.Rating,
                    ReviewComment = r.ReviewComment,
                    CreatedAt = r.CreatedAt,
                    CanEdit = false, // Set based on requesting user
                    CanDelete = false // Set based on requesting user
                }),
                Pagination = new PaginationInfoDto
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalReviews,
                    TotalPages = totalPages,
                    HasPreviousPage = pageNumber > 1,
                    HasNextPage = pageNumber < totalPages
                }
            };
        }

        public async Task<CourseReviewsDto> GetCourseReviewsWithFilterAsync(int courseId, ReviewFilterDto filter)
        {
            var query = _uow.CourseReviews.Query()
                .Include(r => r.User)
                .Include(r => r.Course)
                .Where(r => r.CourseId == courseId);

            // Apply filters
            if (filter.Rating.HasValue)
                query = query.Where(r => r.Rating == filter.Rating.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(r => r.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(r => r.CreatedAt <= filter.EndDate.Value);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                query = query.Where(r => r.ReviewComment.Contains(filter.SearchTerm) ||
                                        r.User.FullName.Contains(filter.SearchTerm));

            if (filter.HasComment.HasValue)
                query = filter.HasComment.Value ?
                    query.Where(r => !string.IsNullOrWhiteSpace(r.ReviewComment)) :
                    query.Where(r => string.IsNullOrWhiteSpace(r.ReviewComment));

            return await GetCourseReviewsFromQuery(query, courseId, filter.PageNumber, filter.PageSize, filter.SortBy, filter.SortOrder);
        }

        public async Task<ReviewStatisticsDto> GetCourseReviewSummaryAsync(int courseId)
        {
            var reviews = await _uow.CourseReviews.Query()
                .Where(r => r.CourseId == courseId)
                .ToListAsync();

            var ratingDistribution = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
            {
                ratingDistribution[i] = reviews.Count(r => r.Rating == i);
            }

            var thisMonth = DateTime.UtcNow.AddMonths(-1);
            var thisWeek = DateTime.UtcNow.AddDays(-7);

            return new ReviewStatisticsDto
            {
                TotalReviews = reviews.Count,
                AverageRating = reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0,
                RatingDistribution = ratingDistribution,
                ReviewsThisMonth = reviews.Count(r => r.CreatedAt >= thisMonth),
                ReviewsThisWeek = reviews.Count(r => r.CreatedAt >= thisWeek),
                RatingTrend = CalculateRatingTrend(reviews),
                TopReviewedCourses = new List<TopReviewedCourseDto>(),
                RecentReviews = reviews.OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .Select(r => new RecentReviewDto
                    {
                        ReviewId = r.CourseReviewId,
                        UserName = r.User?.FullName ?? "Unknown",
                        CourseName = r.Course?.CourseName ?? "Unknown",
                        Rating = r.Rating,
                        ReviewComment = r.ReviewComment,
                        CreatedAt = r.CreatedAt
                    })
            };
        }

        // =====================================================
        // User Reviews Operations
        // =====================================================

        public async Task<UserReviewsDto> GetUserReviewsAsync(int userId, int pageNumber = 1, int pageSize = 20)
        {
            var query = _uow.CourseReviews.Query()
                .Include(r => r.User)
                .Include(r => r.Course)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt);

            var totalReviews = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalReviews / pageSize);

            var reviews = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var user = await _uow.Users.GetByIdAsync(userId);
            var allUserReviews = await _uow.CourseReviews.Query()
                .Where(r => r.UserId == userId)
                .ToListAsync();

            return new UserReviewsDto
            {
                UserId = userId,
                UserName = user?.FullName ?? "Unknown",
                TotalReviews = totalReviews,
                AverageRatingGiven = allUserReviews.Any() ? (decimal)allUserReviews.Average(r => r.Rating) : 0,
                Reviews = reviews.Select(r => new ReviewDetailsDto
                {
                    ReviewId = r.CourseReviewId,
                    UserId = r.UserId,
                    UserName = r.User.FullName,
                    UserEmail = r.User.EmailAddress,
                    CourseId = r.CourseId,
                    CourseName = r.Course.CourseName,
                    Rating = r.Rating,
                    ReviewComment = r.ReviewComment,
                    CreatedAt = r.CreatedAt,
                    CanEdit = true,
                    CanDelete = true
                }),
                Pagination = new PaginationInfoDto
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalReviews,
                    TotalPages = totalPages,
                    HasPreviousPage = pageNumber > 1,
                    HasNextPage = pageNumber < totalPages
                }
            };
        }

        public async Task<bool> HasUserReviewedCourseAsync(int userId, int courseId)
        {
            return await _uow.CourseReviews.Query()
                .AnyAsync(r => r.UserId == userId && r.CourseId == courseId);
        }

        public async Task<ReviewDetailsDto?> GetUserReviewForCourseAsync(int userId, int courseId)
        {
            var review = await _uow.CourseReviews.Query()
                .Include(r => r.User)
                .Include(r => r.Course)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == courseId);

            if (review == null) return null;

            return new ReviewDetailsDto
            {
                ReviewId = review.CourseReviewId,
                UserId = review.UserId,
                UserName = review.User.FullName,
                UserEmail = review.User.EmailAddress,
                CourseId = review.CourseId,
                CourseName = review.Course.CourseName,
                Rating = review.Rating,
                ReviewComment = review.ReviewComment,
                CreatedAt = review.CreatedAt,
                CanEdit = true,
                CanDelete = true
            };
        }

        // =====================================================
        // Instructor/Admin Management Operations
        // =====================================================

        public async Task<IEnumerable<ReviewDetailsDto>> GetInstructorCourseReviewsAsync(int instructorId, ReviewFilterDto? filter = null)
        {
            var query = _uow.CourseReviews.Query()
                .Include(r => r.User)
                .Include(r => r.Course)
                .Where(r => r.Course.InstructorId == instructorId);

            if (filter != null)
            {
                query = ApplyFilters(query, filter);
            }

            var reviews = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();

            return reviews.Select(r => new ReviewDetailsDto
            {
                ReviewId = r.CourseReviewId,
                UserId = r.UserId,
                UserName = r.User.FullName,
                UserEmail = r.User.EmailAddress,
                CourseId = r.CourseId,
                CourseName = r.Course.CourseName,
                Rating = r.Rating,
                ReviewComment = r.ReviewComment,
                CreatedAt = r.CreatedAt,
                CanEdit = false,
                CanDelete = true // Instructor can delete reviews on their courses
            });
        }

        public async Task<ReviewStatisticsDto> GetInstructorReviewStatisticsAsync(int instructorId)
        {
            var instructorCourses = await _uow.Courses.Query()
                .Where(c => c.InstructorId == instructorId)
                .Select(c => c.CourseId)
                .ToListAsync();

            var reviews = await _uow.CourseReviews.Query()
                .Include(r => r.Course)
                .Include(r => r.User)
                .Where(r => instructorCourses.Contains(r.CourseId))
                .ToListAsync();

            var ratingDistribution = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
            {
                ratingDistribution[i] = reviews.Count(r => r.Rating == i);
            }

            var topReviewedCourses = reviews
                .GroupBy(r => new { r.CourseId, r.Course.CourseName })
                .Select(g => new TopReviewedCourseDto
                {
                    CourseId = g.Key.CourseId,
                    CourseName = g.Key.CourseName,
                    InstructorName = reviews.First(r => r.CourseId == g.Key.CourseId).Course.Instructor?.FullName ?? "Unknown",
                    ReviewCount = g.Count(),
                    AverageRating = (decimal)g.Average(r => r.Rating)
                })
                .OrderByDescending(c => c.ReviewCount)
                .Take(5);

            var thisMonth = DateTime.UtcNow.AddMonths(-1);
            var thisWeek = DateTime.UtcNow.AddDays(-7);

            return new ReviewStatisticsDto
            {
                TotalReviews = reviews.Count,
                AverageRating = reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0,
                RatingDistribution = ratingDistribution,
                ReviewsThisMonth = reviews.Count(r => r.CreatedAt >= thisMonth),
                ReviewsThisWeek = reviews.Count(r => r.CreatedAt >= thisWeek),
                RatingTrend = CalculateRatingTrend(reviews),
                TopReviewedCourses = topReviewedCourses,
                RecentReviews = reviews.OrderByDescending(r => r.CreatedAt)
                    .Take(10)
                    .Select(r => new RecentReviewDto
                    {
                        ReviewId = r.CourseReviewId,
                        UserName = r.User.FullName,
                        CourseName = r.Course.CourseName,
                        Rating = r.Rating,
                        ReviewComment = r.ReviewComment,
                        CreatedAt = r.CreatedAt
                    })
            };
        }

        public async Task<IEnumerable<ReviewDetailsDto>> GetAllReviewsForAdminAsync(ReviewFilterDto filter)
        {
            IQueryable<CourseReview> query = _uow.CourseReviews.Query()
                .Include(r => r.User)
                .Include(r => r.Course);

            query = ApplyFilters(query, filter);

            var reviews = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return reviews.Select(r => new ReviewDetailsDto
            {
                ReviewId = r.CourseReviewId,
                UserId = r.UserId,
                UserName = r.User.FullName,
                UserEmail = r.User.EmailAddress,
                CourseId = r.CourseId,
                CourseName = r.Course.CourseName,
                Rating = r.Rating,
                ReviewComment = r.ReviewComment,
                CreatedAt = r.CreatedAt,
                CanEdit = true, // Admin can edit any review
                CanDelete = true // Admin can delete any review
            });
        }

        public async Task<bool> PerformAdminBulkActionAsync(AdminReviewActionDto adminActionDto, int adminUserId)
        {
            _logger.LogInformation("Admin {AdminId} performing bulk action {Action} on {Count} reviews",
                adminUserId, adminActionDto.Action, adminActionDto.ReviewIds.Count());

            var reviews = await _uow.CourseReviews.Query()
                .Where(r => adminActionDto.ReviewIds.Contains(r.CourseReviewId))
                .ToListAsync();

            switch (adminActionDto.Action.ToUpper())
            {
                case "DELETE":
                    foreach (var review in reviews)
                    {
                        await _uow.CourseReviews.DeleteAsync(review);
                    }
                    break;

                default:
                    throw new ArgumentException($"Unsupported admin action: {adminActionDto.Action}");
            }

            await _uow.SaveAsync();
            _logger.LogInformation("Bulk action {Action} completed successfully", adminActionDto.Action);
            return true;
        }

        public async Task<bool> AdminDeleteReviewAsync(int reviewId, int adminUserId, string? reason = null)
        {
            _logger.LogInformation("Admin {AdminId} deleting review {ReviewId}. Reason: {Reason}",
                adminUserId, reviewId, reason ?? "No reason provided");

            var review = await _uow.CourseReviews.GetByIdAsync(reviewId);
            if (review == null)
            {
                throw new KeyNotFoundException("Review not found");
            }

            await _uow.CourseReviews.DeleteAsync(review);
            await _uow.SaveAsync();

            return true;
        }

        // =====================================================
        // Validation and Statistics
        // =====================================================

        public async Task<ReviewValidationDto> ValidateUserCanReviewCourseAsync(int userId, int courseId)
        {
            var validation = new ReviewValidationDto();

            // Check if course exists
            var course = await _uow.Courses.GetByIdAsync(courseId);
            validation.CourseExists = course != null;

            // Check if user has already reviewed this course
            validation.UserHasAlreadyReviewed = await HasUserReviewedCourseAsync(userId, courseId);

            // Check if user is enrolled in the course
            validation.UserIsEnrolledInCourse = await _uow.CourseEnrollments.Query()
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);

            // Determine if validation passes
            var errors = new List<string>();

            if (!validation.CourseExists)
                errors.Add("Course does not exist");

            if (validation.UserHasAlreadyReviewed)
                errors.Add("User has already reviewed this course");

            if (!validation.UserIsEnrolledInCourse)
                errors.Add("User must be enrolled in the course to leave a review");

            validation.ValidationErrors = errors;
            validation.IsValid = !errors.Any();

            return validation;
        }

        public async Task<ReviewStatisticsDto> GetPlatformReviewStatisticsAsync()
        {
            var allReviews = await _uow.CourseReviews.Query()
                .Include(r => r.Course)
                .Include(r => r.User)
                .ToListAsync();

            var ratingDistribution = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
            {
                ratingDistribution[i] = allReviews.Count(r => r.Rating == i);
            }

            var topReviewedCourses = allReviews
                .GroupBy(r => new { r.CourseId, r.Course.CourseName, InstructorName = r.Course.Instructor.FullName })
                .Select(g => new TopReviewedCourseDto
                {
                    CourseId = g.Key.CourseId,
                    CourseName = g.Key.CourseName,
                    InstructorName = g.Key.InstructorName,
                    ReviewCount = g.Count(),
                    AverageRating = (decimal)g.Average(r => r.Rating)
                })
                .OrderByDescending(c => c.ReviewCount)
                .Take(10);

            var thisMonth = DateTime.UtcNow.AddMonths(-1);
            var thisWeek = DateTime.UtcNow.AddDays(-7);

            return new ReviewStatisticsDto
            {
                TotalReviews = allReviews.Count,
                AverageRating = allReviews.Any() ? (decimal)allReviews.Average(r => r.Rating) : 0,
                RatingDistribution = ratingDistribution,
                ReviewsThisMonth = allReviews.Count(r => r.CreatedAt >= thisMonth),
                ReviewsThisWeek = allReviews.Count(r => r.CreatedAt >= thisWeek),
                RatingTrend = CalculateRatingTrend(allReviews),
                TopReviewedCourses = topReviewedCourses,
                RecentReviews = allReviews.OrderByDescending(r => r.CreatedAt)
                    .Take(20)
                    .Select(r => new RecentReviewDto
                    {
                        ReviewId = r.CourseReviewId,
                        UserName = r.User.FullName,
                        CourseName = r.Course.CourseName,
                        Rating = r.Rating,
                        ReviewComment = r.ReviewComment,
                        CreatedAt = r.CreatedAt
                    })
            };
        }

        public async Task<IEnumerable<RecentReviewDto>> GetTrendingReviewsAsync(int limit = 10)
        {
            // Get recent high-rated reviews
            var recentDate = DateTime.UtcNow.AddDays(-30);

            var trendingReviews = await _uow.CourseReviews.Query()
                .Include(r => r.User)
                .Include(r => r.Course)
                .Where(r => r.CreatedAt >= recentDate && r.Rating >= 4)
                .OrderByDescending(r => r.Rating)
                .ThenByDescending(r => r.CreatedAt)
                .Take(limit)
                .ToListAsync();

            return trendingReviews.Select(r => new RecentReviewDto
            {
                ReviewId = r.CourseReviewId,
                UserName = r.User.FullName,
                CourseName = r.Course.CourseName,
                Rating = r.Rating,
                ReviewComment = r.ReviewComment,
                CreatedAt = r.CreatedAt
            });
        }

        // =====================================================
        // Private Helper Methods
        // =====================================================

        private async Task<CourseReviewsDto> GetCourseReviewsFromQuery(IQueryable<CourseReview> query, int courseId,
            int pageNumber, int pageSize, string sortBy, string sortOrder)
        {
            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "rating" => sortOrder.ToLower() == "asc" ? query.OrderBy(r => r.Rating) : query.OrderByDescending(r => r.Rating),
                "username" => sortOrder.ToLower() == "asc" ? query.OrderBy(r => r.User.FullName) : query.OrderByDescending(r => r.User.FullName),
                _ => sortOrder.ToLower() == "asc" ? query.OrderBy(r => r.CreatedAt) : query.OrderByDescending(r => r.CreatedAt)
            };

            var totalReviews = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalReviews / pageSize);

            var reviews = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var course = await _uow.Courses.GetByIdAsync(courseId);

            // Calculate rating distribution for the course
            var allCourseReviews = await _uow.CourseReviews.Query()
                .Where(r => r.CourseId == courseId)
                .ToListAsync();

            var ratingDistribution = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
            {
                ratingDistribution[i] = allCourseReviews.Count(r => r.Rating == i);
            }

            return new CourseReviewsDto
            {
                CourseId = courseId,
                CourseName = course?.CourseName ?? "Unknown",
                AverageRating = allCourseReviews.Any() ? (decimal)allCourseReviews.Average(r => r.Rating) : null,
                TotalReviews = totalReviews,
                RatingDistribution = ratingDistribution,
                Reviews = reviews.Select(r => new ReviewDetailsDto
                {
                    ReviewId = r.CourseReviewId,
                    UserId = r.UserId,
                    UserName = r.User.FullName,
                    UserEmail = r.User.EmailAddress,
                    CourseId = r.CourseId,
                    CourseName = r.Course.CourseName,
                    Rating = r.Rating,
                    ReviewComment = r.ReviewComment,
                    CreatedAt = r.CreatedAt,
                    CanEdit = false,
                    CanDelete = false
                }),
                Pagination = new PaginationInfoDto
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalReviews,
                    TotalPages = totalPages,
                    HasPreviousPage = pageNumber > 1,
                    HasNextPage = pageNumber < totalPages
                }
            };
        }

        private IQueryable<CourseReview> ApplyFilters(IQueryable<CourseReview> query, ReviewFilterDto filter)
        {
            if (filter.CourseId.HasValue)
                query = query.Where(r => r.CourseId == filter.CourseId.Value);

            if (filter.UserId.HasValue)
                query = query.Where(r => r.UserId == filter.UserId.Value);

            if (filter.Rating.HasValue)
                query = query.Where(r => r.Rating == filter.Rating.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(r => r.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(r => r.CreatedAt <= filter.EndDate.Value);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                query = query.Where(r => r.ReviewComment.Contains(filter.SearchTerm) ||
                                        r.User.FullName.Contains(filter.SearchTerm) ||
                                        r.Course.CourseName.Contains(filter.SearchTerm));

            if (filter.HasComment.HasValue)
                query = filter.HasComment.Value ?
                    query.Where(r => !string.IsNullOrWhiteSpace(r.ReviewComment)) :
                    query.Where(r => string.IsNullOrWhiteSpace(r.ReviewComment));

            return query;
        }

        private decimal CalculateRatingTrend(List<CourseReview> reviews)
        {
            if (reviews.Count < 2) return 0;

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var recentReviews = reviews.Where(r => r.CreatedAt >= thirtyDaysAgo).ToList();
            var olderReviews = reviews.Where(r => r.CreatedAt < thirtyDaysAgo).ToList();

            if (!recentReviews.Any() || !olderReviews.Any()) return 0;

            var recentAverage = (decimal)recentReviews.Average(r => r.Rating);
            var olderAverage = (decimal)olderReviews.Average(r => r.Rating);

            return recentAverage - olderAverage;
        }
    }
}