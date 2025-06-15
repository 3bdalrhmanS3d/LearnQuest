using LearnQuestV1.Api.DTOs.Users.Request;
using LearnQuestV1.Api.DTOs.Users.Response;
using LearnQuestV1.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LearnQuestV1.Api.Utilities;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;
using LearnQuestV1.Api.DTOs.Profile;
using Microsoft.AspNetCore.RateLimiting;

namespace LearnQuestV1.Api.Controllers
{
    /// <summary>
    /// Profile management controller for authenticated users
    /// </summary>
    [Route("api/profile")]
    [ApiController]
    [Authorize(Roles = "RegularUser,Instructor,Admin")]
    [Produces("application/json")]
    [EnableRateLimiting("DefaultPolicy")]
    public class ProfileController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ProfileController> _logger;
        private readonly ISecurityAuditLogger _securityAuditLogger;

        private const int PROFILE_CACHE_MINUTES = 5;
        private const int COURSES_CACHE_MINUTES = 10;
        private const int FAVORITES_CACHE_MINUTES = 15;
        private const long MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB

        public ProfileController(
            IUserService userService,
            IMemoryCache cache,
            ILogger<ProfileController> logger,
            ISecurityAuditLogger securityAuditLogger)
        {
            _userService = userService;
            _cache = cache;
            _logger = logger;
            _securityAuditLogger = securityAuditLogger;
        }

        /// <summary>
        /// Get user dashboard information
        /// </summary>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDashboard()
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Dashboard access attempted with invalid token from IP: {IP}",
                    HttpContext.Connection.RemoteIpAddress);
                await _securityAuditLogger.LogSecurityEventAsync("InvalidTokenAccess",
                    "Dashboard access with invalid token", HttpContext, false);
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                await _securityAuditLogger.LogUserActionAsync(userId.Value, "DashboardAccessed",
                    "User accessed dashboard", HttpContext);

                _logger.LogInformation("User {UserId} accessed dashboard successfully", userId);

                return Ok(ApiResponse.Success(new
                {
                    message = "Welcome to your dashboard!",
                    userId = userId,
                    timestamp = DateTime.UtcNow,
                    sessionInfo = new
                    {
                        ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        userAgent = HttpContext.Request.Headers["User-Agent"].ToString()
                    }
                }, "Dashboard loaded successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard for user {UserId}", userId);
                return StatusCode(500, ApiResponse.Error("An error occurred while loading dashboard"));
            }
        }

        /// <summary>
        /// Get current user profile information
        /// </summary>
        [HttpGet("")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Profile access attempted with invalid token from IP: {IP}",
                    HttpContext.Connection.RemoteIpAddress);
                await _securityAuditLogger.LogSecurityEventAsync("InvalidTokenAccess",
                    "Profile access with invalid token", HttpContext, false);
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                // Check cache first with user-specific key
                var cacheKey = $"user_profile_{userId}_{User.Identity?.Name}";
                if (_cache.TryGetValue(cacheKey, out UserProfileDto cachedProfile))
                {
                    _logger.LogDebug("Profile served from cache for user {UserId}", userId);
                    return Ok(ApiResponse.Success(cachedProfile, "Profile retrieved from cache"));
                }

                var profile = await _userService.GetUserProfileAsync(userId.Value);

                // Cache with sliding expiration
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(PROFILE_CACHE_MINUTES),
                    Priority = CacheItemPriority.Normal,
                    Size = 1
                };
                _cache.Set(cacheKey, profile, cacheOptions);

                await _securityAuditLogger.LogUserActionAsync(userId.Value, "ProfileViewed",
                    "User viewed profile information", HttpContext);

                _logger.LogInformation("Profile retrieved successfully for user {UserId}", userId);
                return Ok(ApiResponse.Success(profile, "Profile retrieved successfully"));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Profile not found for user {UserId}", userId);
                await _securityAuditLogger.LogUserActionAsync(userId.Value, "ProfileNotFound",
                    "Profile access for non-existent user", HttpContext);
                return NotFound(ApiResponse.Error("User profile not found"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Incomplete profile access for user {UserId}: {Message}", userId, ex.Message);
                return BadRequest(ApiResponse.Error(
                    "User profile is incomplete. Please complete your profile first.",
                    new
                    {
                        requiredFields = new
                        {
                            BirthDate = "YYYY-MM-DD format required",
                            Education = "Education Level is required",
                            Nationality = "Nationality is required"
                        },
                        profileCompletionUrl = "/api/profile/update"
                    }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile for user {UserId}", userId);
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving your profile"));
            }
        }

        /// <summary>
        /// Update user profile information
        /// </summary>
        [HttpPost("update")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [EnableRateLimiting("UpdatePolicy")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileUpdateDto input)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Invalid profile update request: {Errors}", string.Join(", ", errors));
                return BadRequest(ApiResponse.ValidationError(ModelState));
            }

            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Profile update attempted with invalid token from IP: {IP}",
                    HttpContext.Connection.RemoteIpAddress);
                await _securityAuditLogger.LogSecurityEventAsync("InvalidTokenAccess",
                    "Profile update with invalid token", HttpContext, false);
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            // Additional validation
            if (input.BirthDate > DateTime.Now.AddYears(-13))
            {
                _logger.LogWarning("Invalid birth date provided by user {UserId}: {BirthDate}", userId, input.BirthDate);
                return BadRequest(ApiResponse.Error("You must be at least 13 years old to use this platform"));
            }

            if (input.BirthDate < DateTime.Now.AddYears(-120))
            {
                _logger.LogWarning("Unrealistic birth date provided by user {UserId}: {BirthDate}", userId, input.BirthDate);
                return BadRequest(ApiResponse.Error("Please provide a valid birth date"));
            }

            try
            {
                await _userService.UpdateUserProfileAsync(userId.Value, input);

                // Invalidate all related caches
                var cacheKeys = new[]
                {
                    $"user_profile_{userId}_{User.Identity?.Name}",
                    $"user_profile_{userId}",
                    $"user_stats_{userId}"
                };

                foreach (var key in cacheKeys)
                {
                    _cache.Remove(key);
                }

                await _securityAuditLogger.LogUserActionAsync(userId.Value, "ProfileUpdated",
                    $"User updated profile information: Education={input.Edu}, Nationality={input.National}",
                    HttpContext);

                _logger.LogInformation("Profile updated successfully for user {UserId}", userId);
                return Ok(ApiResponse.Success("Profile updated successfully"));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Profile update attempted for non-existent user {UserId}", userId);
                return NotFound(ApiResponse.Error("User not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
                return StatusCode(500, ApiResponse.Error("An error occurred while updating your profile"));
            }
        }

        /// <summary>
        /// Register payment for a course
        /// </summary>
        [HttpPost("pay-course")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [EnableRateLimiting("PaymentPolicy")]
        public async Task<IActionResult> PayForCourse([FromBody] PaymentRequestDto input)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Invalid payment request: {Errors}", string.Join(", ", errors));
                return BadRequest(ApiResponse.ValidationError(ModelState));
            }

            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Payment attempted with invalid token from IP: {IP}",
                    HttpContext.Connection.RemoteIpAddress);
                await _securityAuditLogger.LogSecurityEventAsync("InvalidTokenAccess",
                    "Payment attempt with invalid token", HttpContext, false);
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            // Additional payment validation
            if (input.Amount <= 0)
            {
                _logger.LogWarning("Invalid payment amount provided by user {UserId}: {Amount}", userId, input.Amount);
                return BadRequest(ApiResponse.Error("Payment amount must be greater than zero"));
            }

            if (string.IsNullOrWhiteSpace(input.TransactionId))
            {
                _logger.LogWarning("Missing transaction ID for payment by user {UserId}", userId);
                return BadRequest(ApiResponse.Error("Transaction ID is required"));
            }

            try
            {
                await _userService.RegisterPaymentAsync(userId.Value, input);

                // Invalidate course-related cache
                _cache.Remove($"user_courses_{userId}");

                await _securityAuditLogger.LogUserActionAsync(userId.Value, "PaymentRegistered",
                    $"User registered payment for course {input.CourseId}, Amount: {input.Amount:C}, Transaction: {input.TransactionId}",
                    HttpContext);

                _logger.LogInformation("Payment registered for user {UserId}, course {CourseId}, amount {Amount}",
                    userId, input.CourseId, input.Amount);

                return Ok(ApiResponse.Success(new
                {
                    message = "Payment recorded successfully. Awaiting confirmation",
                    courseId = input.CourseId,
                    amount = input.Amount,
                    transactionId = input.TransactionId,
                    status = "Pending"
                }));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Payment attempted for non-existent course {CourseId} by user {UserId}",
                    input.CourseId, userId);
                return NotFound(ApiResponse.Error("Course not found"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid payment operation for user {UserId}, course {CourseId}: {Message}",
                    userId, input.CourseId, ex.Message);
                return BadRequest(ApiResponse.Error(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for user {UserId}, course {CourseId}",
                    userId, input.CourseId);
                return StatusCode(500, ApiResponse.Error("An error occurred while processing your payment"));
            }
        }

        /// <summary>
        /// Confirm a pending payment
        /// </summary>
        [HttpPost("confirm-payment/{paymentId:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [EnableRateLimiting("PaymentPolicy")]
        public async Task<IActionResult> ConfirmPayment([Range(1, int.MaxValue)] int paymentId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Payment confirmation attempted with invalid token from IP: {IP}",
                    HttpContext.Connection.RemoteIpAddress);
                await _securityAuditLogger.LogSecurityEventAsync("InvalidTokenAccess",
                    "Payment confirmation with invalid token", HttpContext, false);
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                await _userService.ConfirmPaymentAsync(paymentId);

                // Invalidate related caches
                var cacheKeys = new[]
                {
                    $"user_courses_{userId}",
                    $"user_stats_{userId}"
                };

                foreach (var key in cacheKeys)
                {
                    _cache.Remove(key);
                }

                await _securityAuditLogger.LogUserActionAsync(userId.Value, "PaymentConfirmed",
                    $"Payment {paymentId} confirmed and enrollment completed", HttpContext);

                _logger.LogInformation("Payment {PaymentId} confirmed for user {UserId}", paymentId, userId);

                return Ok(ApiResponse.Success(new
                {
                    message = "Payment confirmed. Course enrollment successful",
                    paymentId = paymentId,
                    status = "Completed",
                    enrollmentDate = DateTime.UtcNow
                }));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Payment confirmation attempted for non-existent payment {PaymentId} by user {UserId}",
                    paymentId, userId);
                return NotFound(ApiResponse.Error("Payment record not found"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid payment confirmation for payment {PaymentId} by user {UserId}: {Message}",
                    paymentId, userId, ex.Message);
                return BadRequest(ApiResponse.Error(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment {PaymentId} for user {UserId}", paymentId, userId);
                return StatusCode(500, ApiResponse.Error("An error occurred while confirming your payment"));
            }
        }

        /// <summary>
        /// Get user's enrolled courses
        /// </summary>
        [HttpGet("my-courses")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyCourses()
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("My courses access attempted with invalid token from IP: {IP}",
                    HttpContext.Connection.RemoteIpAddress);
                await _securityAuditLogger.LogSecurityEventAsync("InvalidTokenAccess",
                    "My courses access with invalid token", HttpContext, false);
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                // Check cache first
                var cacheKey = $"user_courses_{userId}";
                if (_cache.TryGetValue(cacheKey, out object cachedCourses))
                {
                    _logger.LogDebug("Courses served from cache for user {UserId}", userId);
                    return Ok(cachedCourses);
                }

                var courses = await _userService.GetMyCoursesAsync(userId.Value);

                var result = ApiResponse.Success(new
                {
                    count = courses.Count(),
                    courses = courses,
                    lastUpdated = DateTime.UtcNow
                }, courses.Any() ? $"Found {courses.Count()} enrolled courses" : "No enrolled courses found");

                // Cache with higher priority for enrolled courses
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(COURSES_CACHE_MINUTES),
                    Priority = CacheItemPriority.High,
                    Size = 2
                };
                _cache.Set(cacheKey, result, cacheOptions);

                _logger.LogInformation("Retrieved {CourseCount} courses for user {UserId}", courses.Count(), userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving courses for user {UserId}", userId);
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving your courses"));
            }
        }

        /// <summary>
        /// Get user's favorite courses
        /// </summary>
        [HttpGet("favorite-courses")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetFavoriteCourses()
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Favorite courses access attempted with invalid token from IP: {IP}",
                    HttpContext.Connection.RemoteIpAddress);
                await _securityAuditLogger.LogSecurityEventAsync("InvalidTokenAccess",
                    "Favorite courses access with invalid token", HttpContext, false);
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                // Check cache first
                var cacheKey = $"user_favorites_{userId}";
                if (_cache.TryGetValue(cacheKey, out object cachedFavorites))
                {
                    _logger.LogDebug("Favorites served from cache for user {UserId}", userId);
                    return Ok(cachedFavorites);
                }

                var favorites = await _userService.GetFavoriteCoursesAsync(userId.Value);

                var result = ApiResponse.Success(new
                {
                    count = favorites.Count(),
                    favorites = favorites,
                    lastUpdated = DateTime.UtcNow
                }, favorites.Any() ? $"Found {favorites.Count()} favorite courses" : "No favorite courses found");

                // Cache favorites with longer expiration
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(FAVORITES_CACHE_MINUTES),
                    Priority = CacheItemPriority.Normal,
                    Size = 2
                };
                _cache.Set(cacheKey, result, cacheOptions);

                _logger.LogInformation("Retrieved {FavoriteCount} favorite courses for user {UserId}",
                    favorites.Count(), userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving favorite courses for user {UserId}", userId);
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving your favorite courses"));
            }
        }

        /// <summary>
        /// Upload user profile photo
        /// </summary>
        [HttpPost("upload-photo")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status413PayloadTooLarge)]
        [RequestSizeLimit(MAX_FILE_SIZE)]
        [EnableRateLimiting("UploadPolicy")]
        public async Task<IActionResult> UploadProfilePhoto(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Empty file upload attempted from IP: {IP}",
                    HttpContext.Connection.RemoteIpAddress);
                return BadRequest(ApiResponse.Error("No file uploaded"));
            }

            // Enhanced file validation
            if (file.Length > MAX_FILE_SIZE)
            {
                _logger.LogWarning("File too large: {FileSize} bytes from IP: {IP}",
                    file.Length, HttpContext.Connection.RemoteIpAddress);
                return BadRequest(ApiResponse.Error($"File size must be less than {MAX_FILE_SIZE / (1024 * 1024)}MB"));
            }

            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                _logger.LogWarning("Invalid file type: {ContentType} from IP: {IP}",
                    file.ContentType, HttpContext.Connection.RemoteIpAddress);
                return BadRequest(ApiResponse.Error("Only JPEG, PNG, GIF, and WebP files are allowed"));
            }

            // Additional file name validation
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                _logger.LogWarning("Invalid file extension: {Extension} from IP: {IP}",
                    fileExtension, HttpContext.Connection.RemoteIpAddress);
                return BadRequest(ApiResponse.Error("Invalid file extension"));
            }

            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Photo upload attempted with invalid token from IP: {IP}",
                    HttpContext.Connection.RemoteIpAddress);
                await _securityAuditLogger.LogSecurityEventAsync("InvalidTokenAccess",
                    "Photo upload with invalid token", HttpContext, false);
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                await _userService.UploadProfilePhotoAsync(userId.Value, file);

                // Invalidate profile cache
                var cacheKeys = new[]
                {
                    $"user_profile_{userId}_{User.Identity?.Name}",
                    $"user_profile_{userId}"
                };

                foreach (var key in cacheKeys)
                {
                    _cache.Remove(key);
                }

                await _securityAuditLogger.LogUserActionAsync(userId.Value, "ProfilePhotoUploaded",
                    $"User uploaded profile photo: {file.FileName} ({file.Length} bytes)", HttpContext);

                _logger.LogInformation("Profile photo uploaded successfully for user {UserId}, size: {FileSize} bytes",
                    userId, file.Length);

                return Ok(ApiResponse.Success(new
                {
                    message = "Profile photo uploaded successfully",
                    fileName = file.FileName,
                    fileSize = file.Length,
                    uploadedAt = DateTime.UtcNow
                }));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Photo upload attempted for non-existent user {UserId}", userId);
                return NotFound(ApiResponse.Error("User not found"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid photo upload operation for user {UserId}: {Message}", userId, ex.Message);
                return BadRequest(ApiResponse.Error(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photo for user {UserId}", userId);
                return StatusCode(500, ApiResponse.Error("An error occurred while uploading your photo"));
            }
        }

        /// <summary>
        /// Delete user profile photo
        /// </summary>
        [HttpDelete("delete-photo")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [EnableRateLimiting("UpdatePolicy")]
        public async Task<IActionResult> DeleteProfilePhoto()
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Photo deletion attempted with invalid token from IP: {IP}",
                    HttpContext.Connection.RemoteIpAddress);
                await _securityAuditLogger.LogSecurityEventAsync("InvalidTokenAccess",
                    "Photo deletion with invalid token", HttpContext, false);
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                await _userService.DeleteProfilePhotoAsync(userId.Value);

                // Invalidate profile cache
                var cacheKeys = new[]
                {
                    $"user_profile_{userId}_{User.Identity?.Name}",
                    $"user_profile_{userId}"
                };

                foreach (var key in cacheKeys)
                {
                    _cache.Remove(key);
                }

                await _securityAuditLogger.LogUserActionAsync(userId.Value, "ProfilePhotoDeleted",
                    "User deleted profile photo and restored default", HttpContext);

                _logger.LogInformation("Profile photo deleted for user {UserId}", userId);

                return Ok(ApiResponse.Success(new
                {
                    message = "Profile photo deleted. Default restored",
                    deletedAt = DateTime.UtcNow,
                    defaultPhoto = "/uploads/profile-pictures/default.png"
                }));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Photo deletion attempted for non-existent user {UserId}", userId);
                return NotFound(ApiResponse.Error("User not found"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid photo deletion operation for user {UserId}: {Message}", userId, ex.Message);
                return BadRequest(ApiResponse.Error(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting photo for user {UserId}", userId);
                return StatusCode(500, ApiResponse.Error("An error occurred while deleting your photo"));
            }
        }

        /// <summary>
        /// Get user statistics and progress summary
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserStats()
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Stats access attempted with invalid token from IP: {IP}",
                    HttpContext.Connection.RemoteIpAddress);
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                // Check cache first
                var cacheKey = $"user_stats_{userId}";
                if (_cache.TryGetValue(cacheKey, out object cachedStats))
                {
                    _logger.LogDebug("Stats served from cache for user {UserId}", userId);
                    return Ok(cachedStats);
                }

                var stats = await _userService.GetUserStatsAsync(userId.Value);

                var result = ApiResponse.Success(new
                {
                    enrolledCourses = stats.SharedCourses,
                    completedSections = stats.CompletedSections,
                    progressDetails = stats.Progress,
                    lastUpdated = DateTime.UtcNow,
                    summary = new
                    {
                        totalActiveCourses = stats.SharedCourses,
                        averageProgress = stats.Progress.Any() ?
                            (int)stats.Progress.Average(p => p.ProgressPercentage) : 0,
                        completionRate = stats.SharedCourses > 0 ?
                            (double)stats.CompletedSections / stats.SharedCourses : 0
                    }
                }, "User statistics retrieved successfully");

                // Cache stats for shorter time
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(3),
                    Priority = CacheItemPriority.Normal,
                    Size = 1
                };
                _cache.Set(cacheKey, result, cacheOptions);

                _logger.LogInformation("Retrieved statistics for user {UserId}: {Courses} courses, {Sections} sections",
                    userId, stats.SharedCourses, stats.CompletedSections);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving statistics for user {UserId}", userId);
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving your statistics"));
            }
        }
    }

    /// <summary>
    /// Standardized API response wrapper with enhanced features
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public object Errors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string RequestId { get; set; } = Guid.NewGuid().ToString("N")[..8];
    }

    public static class ApiResponse
    {
        public static ApiResponse<T> Success<T>(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<object> Success(string message)
        {
            return new ApiResponse<object>
            {
                Success = true,
                Message = message,
                Data = null
            };
        }

        public static ApiResponse<object> Error(string message, object errors = null)
        {
            return new ApiResponse<object>
            {
                Success = false,
                Message = message,
                Data = null,
                Errors = errors
            };
        }

        public static ApiResponse<object> ValidationError(Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
        {
            var errors = modelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return new ApiResponse<object>
            {
                Success = false,
                Message = "Validation failed",
                Data = null,
                Errors = new
                {
                    validationErrors = errors,
                    errorCount = errors.Sum(e => e.Value.Length),
                    fields = errors.Keys.ToArray()
                }
            };
        }
    }
}