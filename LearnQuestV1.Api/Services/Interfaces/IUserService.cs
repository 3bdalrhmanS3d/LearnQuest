using LearnQuestV1.Api.DTOs.Payments;
using LearnQuestV1.Api.DTOs.Profile;
using LearnQuestV1.Api.DTOs.Progress;
using LearnQuestV1.Api.DTOs.User.Response;
using LearnQuestV1.Api.DTOs.Users.Request;
using LearnQuestV1.Api.DTOs.Users.Response;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearnQuestV1.Api.Services.Interfaces
{
    /// <summary>
    /// User service interface defining all user-related operations
    /// </summary>
    public interface IUserService
    {
        // =====================================================
        // Profile Management
        // =====================================================

        /// <summary>
        /// Retrieves a user's complete profile information
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <returns>Complete user profile with progress information</returns>
        /// <exception cref="KeyNotFoundException">When user does not exist</exception>
        /// <exception cref="InvalidOperationException">When user profile is incomplete</exception>
        Task<UserProfileDto> GetUserProfileAsync(int userId);

        /// <summary>
        /// Updates a user's profile information
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <param name="dto">Profile update information</param>
        /// <exception cref="KeyNotFoundException">When user does not exist</exception>
        Task UpdateUserProfileAsync(int userId, UserProfileUpdateDto dto);

        // =====================================================
        // Payment and Enrollment Management
        // =====================================================

        /// <summary>
        /// Registers a payment for a course
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <param name="dto">Payment information</param>
        /// <exception cref="KeyNotFoundException">When course does not exist</exception>
        /// <exception cref="InvalidOperationException">When user already paid for the course</exception>
        Task RegisterPaymentAsync(int userId, PaymentRequestDto dto);

        /// <summary>
        /// Confirms a pending payment and enrolls user in course
        /// </summary>
        /// <param name="paymentId">The payment's unique identifier</param>
        /// <exception cref="KeyNotFoundException">When payment does not exist</exception>
        /// <exception cref="InvalidOperationException">When payment is already completed</exception>
        Task ConfirmPaymentAsync(int paymentId);

        // =====================================================
        // Course Access and Management
        // =====================================================

        /// <summary>
        /// Retrieves all courses the user is enrolled in and has paid for
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <returns>List of enrolled courses</returns>
        Task<IEnumerable<MyCourseDto>> GetMyCoursesAsync(int userId);

        /// <summary>
        /// Retrieves all favorite courses for the user
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <returns>List of favorite courses</returns>
        Task<IEnumerable<CourseDto>> GetFavoriteCoursesAsync(int userId);

        // =====================================================
        // Profile Photo Management
        // =====================================================

        /// <summary>
        /// Uploads a new profile photo for the user
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <param name="file">The uploaded image file</param>
        /// <exception cref="KeyNotFoundException">When user does not exist</exception>
        /// <exception cref="InvalidOperationException">When file is invalid</exception>
        Task UploadProfilePhotoAsync(int userId, IFormFile file);

        /// <summary>
        /// Deletes the user's custom profile photo and reverts to default
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <exception cref="KeyNotFoundException">When user does not exist</exception>
        /// <exception cref="InvalidOperationException">When no custom photo exists</exception>
        Task DeleteProfilePhotoAsync(int userId);

        // =====================================================
        // Course Discovery and Tracks
        // =====================================================

        /// <summary>
        /// Retrieves all available course tracks
        /// </summary>
        /// <returns>List of course tracks with course counts</returns>
        Task<IEnumerable<TrackDto>> GetAllTracksAsync();

        /// <summary>
        /// Retrieves all courses in a specific track
        /// </summary>
        /// <param name="trackId">The track's unique identifier</param>
        /// <returns>Track information with associated courses</returns>
        /// <exception cref="KeyNotFoundException">When track does not exist</exception>
        Task<TrackCoursesDto> GetCoursesInTrackAsync(int trackId);

        /// <summary>
        /// Searches for courses by name or description
        /// </summary>
        /// <param name="search">Search term (optional)</param>
        /// <returns>List of matching courses</returns>
        Task<IEnumerable<CourseDto>> SearchCoursesAsync(string? search);

        // =====================================================
        // Learning Content Access
        // =====================================================

        /// <summary>
        /// Retrieves all levels for a course (enrollment required)
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <param name="courseId">The course's unique identifier</param>
        /// <returns>Course levels information</returns>
        /// <exception cref="InvalidOperationException">When user is not enrolled</exception>
        /// <exception cref="KeyNotFoundException">When course does not exist</exception>
        Task<LevelsResponseDto> GetCourseLevelsAsync(int userId, int courseId);

        /// <summary>
        /// Retrieves all sections for a level (enrollment required)
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <param name="levelId">The level's unique identifier</param>
        /// <returns>Level sections information</returns>
        /// <exception cref="InvalidOperationException">When user is not enrolled</exception>
        /// <exception cref="KeyNotFoundException">When level does not exist</exception>
        Task<SectionsResponseDto> GetLevelSectionsAsync(int userId, int levelId);

        /// <summary>
        /// Retrieves all contents for a section (enrollment required)
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <param name="sectionId">The section's unique identifier</param>
        /// <returns>Section contents information</returns>
        /// <exception cref="InvalidOperationException">When user is not enrolled</exception>
        /// <exception cref="KeyNotFoundException">When section does not exist</exception>
        Task<ContentsResponseDto> GetSectionContentsAsync(int userId, int sectionId);

        // =====================================================
        // Learning Progress Tracking
        // =====================================================

        /// <summary>
        /// Starts tracking time for content consumption
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <param name="contentId">The content's unique identifier</param>
        /// <exception cref="InvalidOperationException">When session is already active</exception>
        Task StartContentAsync(int userId, int contentId);

        /// <summary>
        /// Stops tracking time for content consumption
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <param name="contentId">The content's unique identifier</param>
        /// <exception cref="KeyNotFoundException">When no active session exists</exception>
        Task EndContentAsync(int userId, int contentId);

        /// <summary>
        /// Marks a section as completed and advances progress
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <param name="currentSectionId">The section being completed</param>
        /// <returns>Completion result with next section information</returns>
        /// <exception cref="KeyNotFoundException">When section does not exist</exception>
        Task<CompleteSectionResultDto> CompleteSectionAsync(int userId, int currentSectionId);

        /// <summary>
        /// Gets the next section in the learning path
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <param name="courseId">The course's unique identifier</param>
        /// <returns>Next section information or completion message</returns>
        /// <exception cref="KeyNotFoundException">When progress record is not found</exception>
        Task<NextSectionDto> GetNextSectionAsync(int userId, int courseId);

        // =====================================================
        // Statistics and Analytics
        // =====================================================

        /// <summary>
        /// Retrieves comprehensive user learning statistics
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <returns>User statistics including progress and completion data</returns>
        Task<UserStatsDto> GetUserStatsAsync(int userId);

        /// <summary>
        /// Checks if a user has completed all sections in a course
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <param name="courseId">The course's unique identifier</param>
        /// <returns>Course completion status and progress</returns>
        Task<CourseCompletionDto> HasCompletedCourseAsync(int userId, int courseId);

        // =====================================================
        // Notification Management
        // =====================================================

        /// <summary>
        /// Retrieves all notifications for the user
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <returns>List of user notifications ordered by creation date</returns>
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId);

        /// <summary>
        /// Gets the count of unread notifications
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <returns>Number of unread notifications</returns>
        Task<int> GetUnreadNotificationsCountAsync(int userId);

        /// <summary>
        /// Marks a specific notification as read
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <param name="notificationId">The notification's unique identifier</param>
        /// <exception cref="KeyNotFoundException">When notification does not exist</exception>
        Task MarkNotificationAsReadAsync(int userId, int notificationId);

        /// <summary>
        /// Marks all user notifications as read
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        Task MarkAllNotificationsAsReadAsync(int userId);
    }
}