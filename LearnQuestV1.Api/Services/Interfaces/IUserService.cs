using LearnQuestV1.Api.DTOs.Payments;
using LearnQuestV1.Api.DTOs.Profile;
using LearnQuestV1.Api.DTOs.Progress;
using LearnQuestV1.Api.DTOs.User.Response;
using LearnQuestV1.Api.DTOs.Users.Request;

namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IUserService
    {
        // ===== الملف الشخصي =====
        Task<UserProfileDto> GetUserProfileAsync(int userId);
        Task UpdateUserProfileAsync(int userId, UserProfileUpdateDto dto);

        // ===== المدفوعات والتسجيل في الدورة =====
        Task RegisterPaymentAsync(int userId, PaymentRequestDto dto);
        Task ConfirmPaymentAsync(int paymentId);

        // ===== الدورات للمستخدم والدورات المفضلة =====
        Task<IEnumerable<MyCourseDto>> GetMyCoursesAsync(int userId);
        Task<IEnumerable<CourseDto>> GetFavoriteCoursesAsync(int userId);

        // ===== رفع/حذف صورة الملف الشخصي =====
        Task UploadProfilePhotoAsync(int userId, IFormFile file);
        Task DeleteProfilePhotoAsync(int userId);

        // ===== المسارات ودورات كل مسار =====
        Task<IEnumerable<TrackDto>> GetAllTracksAsync();
        Task<TrackCoursesDto> GetCoursesInTrackAsync(int trackId);

        // ===== البحث في الدورات =====
        Task<IEnumerable<CourseDto>> SearchCoursesAsync(string? search);

        // ===== المستويات والأقسام والمحتويات =====
        Task<LevelsResponseDto> GetCourseLevelsAsync(int userId, int courseId);
        Task<SectionsResponseDto> GetLevelSectionsAsync(int userId, int levelId);
        Task<ContentsResponseDto> GetSectionContentsAsync(int userId, int sectionId);

        // ===== تتبُّع وقت المحتوى =====
        Task StartContentAsync(int userId, int contentId);
        Task EndContentAsync(int userId, int contentId);

        // ===== إكمال قسم والحصول على القسم التالي =====
        Task<CompleteSectionResultDto> CompleteSectionAsync(int userId, int currentSectionId);
        Task<NextSectionDto> GetNextSectionAsync(int userId, int courseId);

        // ===== إحصاءات المستخدم =====
        Task<UserStatsDto> GetUserStatsAsync(int userId);
        Task<CourseCompletionDto> HasCompletedCourseAsync(int userId, int courseId);

        // ===== الإشعارات =====
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId);
        Task<int> GetUnreadNotificationsCountAsync(int userId);
        Task MarkNotificationAsReadAsync(int userId, int notificationId);
        Task MarkAllNotificationsAsReadAsync(int userId);
    }
}
