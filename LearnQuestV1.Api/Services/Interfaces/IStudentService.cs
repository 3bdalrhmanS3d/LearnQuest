using LearnQuestV1.Api.DTOs.Progress;
using LearnQuestV1.Api.DTOs.Student;
using LearnQuestV1.Api.DTOs.User.Response;
using LearnQuestV1.Api.DTOs.Users.Response;

namespace LearnQuestV1.Api.Services.Interfaces
{
    /// <summary>
    /// Service interface for student-specific operations
    /// </summary>
    public interface IStudentService
    {
        // =====================================================
        // DASHBOARD AND OVERVIEW
        // =====================================================

        /// <summary>
        /// Get comprehensive student dashboard with progress, recent activities, and recommendations
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <returns>Student dashboard data</returns>
        Task<StudentDashboardDto> GetStudentDashboardAsync(int userId);

        /// <summary>
        /// Get detailed user statistics and progress across all courses
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <returns>User statistics</returns>
        Task<StudentDashboardResponseDto> GetUserStatsAsync(int userId);

        /// <summary>
        /// Get recent learning activities for the student
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <param name="limit">Maximum number of activities to return</param>
        /// <returns>List of recent activities</returns>
        Task<IEnumerable<StudentActivityDto>> GetRecentActivitiesAsync(int userId, int limit = 10);

        // =====================================================
        // COURSE ACCESS AND NAVIGATION
        // =====================================================

        /// <summary>
        /// Get course levels for enrolled student with progress tracking
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <param name="courseId">Course ID</param>
        /// <returns>Course levels with progress</returns>
        /// <exception cref="UnauthorizedAccessException">When user is not enrolled</exception>
        /// <exception cref="KeyNotFoundException">When course is not found</exception>
        Task<LevelsResponseDto> GetCourseLevelsAsync(int userId, int courseId);

        /// <summary>
        /// Get level sections for enrolled student with progress tracking
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <param name="levelId">Level ID</param>
        /// <returns>Level sections with progress</returns>
        /// <exception cref="UnauthorizedAccessException">When user is not enrolled</exception>
        /// <exception cref="KeyNotFoundException">When level is not found</exception>
        Task<SectionsResponseDto> GetLevelSectionsAsync(int userId, int levelId);

        /// <summary>
        /// Get section contents for enrolled student with progress tracking
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <param name="sectionId">Section ID</param>
        /// <returns>Section contents with progress</returns>
        /// <exception cref="UnauthorizedAccessException">When user is not enrolled</exception>
        /// <exception cref="KeyNotFoundException">When section is not found</exception>
        Task<ContentsResponseDto> GetSectionContentsAsync(int userId, int sectionId);

        /// <summary>
        /// Get comprehensive learning path for a course showing progress and next steps
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <param name="courseId">Course ID</param>
        /// <returns>Learning path with progress visualization</returns>
        /// <exception cref="UnauthorizedAccessException">When user is not enrolled</exception>
        /// <exception cref="KeyNotFoundException">When course is not found</exception>
        Task<LearningPathDto> GetLearningPathAsync(int userId, int courseId);

        /// <summary>
        /// Get next section in course progression
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <param name="courseId">Course ID</param>
        /// <returns>Next section information</returns>
        /// <exception cref="UnauthorizedAccessException">When user is not enrolled</exception>
        /// <exception cref="KeyNotFoundException">When course or progress is not found</exception>
        Task<NextSectionDto> GetNextSectionAsync(int userId, int courseId);

        // =====================================================
        // CONTENT INTERACTION AND PROGRESS
        // =====================================================

        /// <summary>
        /// Start content session and track beginning of learning activity
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <param name="contentId">Content ID</param>
        /// <exception cref="UnauthorizedAccessException">When user is not enrolled</exception>
        /// <exception cref="InvalidOperationException">When content session cannot be started</exception>
        Task StartContentAsync(int userId, int contentId);

        /// <summary>
        /// End content session and track completion/progress
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <param name="contentId">Content ID</param>
        /// <exception cref="KeyNotFoundException">When no active session is found</exception>
        Task EndContentAsync(int userId, int contentId);

        /// <summary>
        /// Complete section and automatically progress to next section
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <param name="sectionId">Section ID</param>
        /// <returns>Completion result with next section information</returns>
        /// <exception cref="KeyNotFoundException">When section is not found</exception>
        Task<CompleteSectionResultDto> CompleteSectionAsync(int userId, int sectionId);

        /// <summary>
        /// Get course completion status and certificate eligibility
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <param name="courseId">Course ID</param>
        /// <returns>Course completion information</returns>
        Task<DTOs.Student.CourseCompletionDto> GetCourseCompletionAsync(int userId, int courseId);

        // =====================================================
        // BOOKMARKS AND FAVORITES
        // =====================================================

        /// <summary>
        /// Bookmark content for later reference
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <param name="contentId">Content ID</param>
        /// <exception cref="KeyNotFoundException">When content is not found</exception>
        /// <exception cref="InvalidOperationException">When content is already bookmarked</exception>
        Task BookmarkContentAsync(int userId, int contentId);

        /// <summary>
        /// Remove bookmark from content
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <param name="contentId">Content ID</param>
        /// <exception cref="KeyNotFoundException">When bookmark is not found</exception>
        Task RemoveBookmarkAsync(int userId, int contentId);

        /// <summary>
        /// Get user bookmarks with optional course filtering
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <param name="courseId">Optional course ID filter</param>
        /// <returns>List of bookmarked content</returns>
        Task<IEnumerable<BookmarkDto>> GetBookmarksAsync(int userId, int? courseId = null);

        // =====================================================
        // LEARNING ANALYTICS AND INSIGHTS
        // =====================================================

        /// <summary>
        /// Get current learning streak and consistency metrics
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <returns>Learning streak information</returns>
        Task<LearningStreakDto> GetLearningStreakAsync(int userId);

        /// <summary>
        /// Get user achievements, badges, and milestones
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <returns>List of user achievements</returns>
        Task<IEnumerable<AchievementDto>> GetAchievementsAsync(int userId);

        /// <summary>
        /// Get personalized study recommendations based on learning history
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <param name="limit">Maximum number of recommendations</param>
        /// <returns>List of study recommendations</returns>
        Task<IEnumerable<StudyRecommendationDto>> GetStudyRecommendationsAsync(int userId, int limit = 5);

        // =====================================================
        // STUDY PLANS AND GOALS
        // =====================================================

        /// <summary>
        /// Get personalized study plan for a course
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <param name="courseId">Course ID</param>
        /// <returns>Personalized study plan</returns>
        /// <exception cref="UnauthorizedAccessException">When user is not enrolled</exception>
        /// <exception cref="KeyNotFoundException">When course is not found</exception>
        Task<StudyPlanDto> GetStudyPlanAsync(int userId, int courseId);

        /// <summary>
        /// Set learning goal for a course (completion time, daily study time, etc.)
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <param name="courseId">Course ID</param>
        /// <param name="goalRequest">Learning goal parameters</param>
        /// <exception cref="InvalidOperationException">When goal cannot be set</exception>
        Task SetLearningGoalAsync(int userId, int courseId, SetLearningGoalDto goalRequest);

        /// <summary>
        /// Get learning goals and progress towards them
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <param name="courseId">Optional course ID filter</param>
        /// <returns>List of learning goals with progress</returns>
        Task<IEnumerable<LearningGoalDto>> GetLearningGoalsAsync(int userId, int? courseId = null);

        // =====================================================
        // PROGRESS INSIGHTS AND ANALYTICS
        // =====================================================

        /// <summary>
        /// Get detailed progress analytics for a course
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <param name="courseId">Course ID</param>
        /// <param name="timeRange">Time range for analytics (default: last 30 days)</param>
        /// <returns>Detailed progress analytics</returns>
        Task<CourseProgressAnalyticsDto> GetCourseProgressAnalyticsAsync(int userId, int courseId, int timeRangeDays = 30);

        /// <summary>
        /// Get time spent learning analytics
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <param name="timeRange">Time range for analytics (default: last 30 days)</param>
        /// <returns>Time spent analytics</returns>
        Task<TimeSpentAnalyticsDto> GetTimeSpentAnalyticsAsync(int userId, int timeRangeDays = 30);

        /// <summary>
        /// Get learning insights and suggestions for improvement
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <returns>Personalized learning insights</returns>
        Task<LearningInsightsDto> GetLearningInsightsAsync(int userId);

        // =====================================================
        // HELPER METHODS
        // =====================================================

        /// <summary>
        /// Check if user is enrolled in a specific course
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <param name="courseId">Course ID</param>
        /// <returns>True if enrolled, false otherwise</returns>
        Task<bool> IsUserEnrolledInCourseAsync(int userId, int courseId);

        /// <summary>
        /// Get user's enrollment status for a course
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <param name="courseId">Course ID</param>
        /// <returns>Enrollment status information</returns>
        Task<EnrollmentStatusDto> GetEnrollmentStatusAsync(int userId, int courseId);

        /// <summary>
        /// Calculate user's overall progress percentage across all enrolled courses
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <returns>Overall progress percentage</returns>
        Task<decimal> GetOverallProgressPercentageAsync(int userId);

        /// <summary>
        /// Get upcoming deadlines and important dates for the student
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <param name="daysAhead">Number of days to look ahead (default: 7)</param>
        /// <returns>List of upcoming deadlines</returns>
        Task<IEnumerable<UpcomingDeadlineDto>> GetUpcomingDeadlinesAsync(int userId, int daysAhead = 7);

        /// <summary>
        /// Get student's current active sessions (for limiting concurrent access)
        /// </summary>
        /// <param name="userId">Student user ID</param>
        /// <returns>List of active content sessions</returns>
        Task<IEnumerable<ActiveSessionDto>> GetActiveSessionsAsync(int userId);
    }
}