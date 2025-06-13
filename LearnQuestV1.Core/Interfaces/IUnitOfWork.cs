using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Models.Administration;
using LearnQuestV1.Core.Models.Communication;
using LearnQuestV1.Core.Models.CourseOrganization;
using LearnQuestV1.Core.Models.CourseStructure;
using LearnQuestV1.Core.Models.FeedbackAndReviews;
using LearnQuestV1.Core.Models.Financial;
using LearnQuestV1.Core.Models.LearningAndProgress;
using LearnQuestV1.Core.Models.UserManagement;
using LearnQuestV1.Core.Models.Quiz;
using Microsoft.EntityFrameworkCore.Storage;

namespace LearnQuestV1.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // User Management
        IBaseRepo<User> Users { get; }
        IBaseRepo<UserDetail> UserDetails { get; }
        IBaseRepo<AccountVerification> AccountVerifications { get; }
        IBaseRepo<RefreshToken> RefreshTokens { get; }
        IBaseRepo<UserVisitHistory> UserVisitHistories { get; }
        IBaseRepo<BlacklistToken> BlacklistTokens { get; }

        // Course Structure
        IBaseRepo<Course> Courses { get; }
        IBaseRepo<AboutCourse> AboutCourses { get; }
        IBaseRepo<CourseSkill> CourseSkills { get; }
        IBaseRepo<Level> Levels { get; }
        IBaseRepo<Section> Sections { get; }
        IBaseRepo<Content> Contents { get; }

        // Course Organization
        IBaseRepo<CourseEnrollment> CourseEnrollments { get; }
        IBaseRepo<CourseFeedback> CourseFeedbacks { get; }
        IBaseRepo<CourseReview> CourseReviews { get; }
        IBaseRepo<CourseTrack> CourseTracks { get; }
        IBaseRepo<CourseTrackCourse> CourseTrackCourses { get; }
        IBaseRepo<FavoriteCourse> FavoriteCourses { get; }

        // Financial & Progress
        IBaseRepo<Payment> Payments { get; }
        IBaseRepo<UserCoursePoint> UserCoursePoints { get; }
        IBaseRepo<UserProgress> UserProgresses { get; }
        IBaseRepo<UserContentActivity> UserContentActivities { get; }

        // Communication & Administration
        IBaseRepo<Notification> Notifications { get; }
        IBaseRepo<AdminActionLog> AdminActionLogs { get; }

        // Quiz System Repositories (Specialized)
        IQuizRepository Quizzes { get; }
        IQuestionRepository Questions { get; }
        IQuizAttemptRepository QuizAttempts { get; }

        // Quiz System Basic Repositories
        IBaseRepo<QuizQuestion> QuizQuestions { get; }
        IBaseRepo<QuestionOption> QuestionOptions { get; }
        IBaseRepo<UserAnswer> UserAnswers { get; }

        IBaseRepo<SecurityAuditLog> SecurityAuditLogs { get; }

        // Transaction Methods
        Task<int> SaveAsync();
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}