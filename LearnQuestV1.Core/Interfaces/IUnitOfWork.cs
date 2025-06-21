using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using LearnQuestV1.Core.Models.UserManagement;
using LearnQuestV1.Core.Models.Administration;
using LearnQuestV1.Core.Models.Communication;
using LearnQuestV1.Core.Models.CourseStructure;
using LearnQuestV1.Core.Models.CourseOrganization;
using LearnQuestV1.Core.Models.FeedbackAndReviews;
using LearnQuestV1.Core.Models.Financial;
using LearnQuestV1.Core.Models.LearningAndProgress;
using LearnQuestV1.Core.Models.Quiz;
using LearnQuestV1.Core.Models;

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
        IBaseRepo<CourseTrack> CourseTracks { get; }
        IBaseRepo<CourseTrackCourse> CourseTrackCourses { get; }
        IBaseRepo<CourseEnrollment> CourseEnrollments { get; }
        IBaseRepo<FavoriteCourse> FavoriteCourses { get; }
        IBaseRepo<CourseFeedback> CourseFeedbacks { get; }
        IBaseRepo<CourseReview> CourseReviews { get; }

        // Financial
        IBaseRepo<Payment> Payments { get; }
        IBaseRepo<PaymentTransaction> PaymentTransactions { get; }
        IBaseRepo<Discount> Discounts { get; }

        // User Enhancement
        IBaseRepo<UserPreferences> UserPreferences { get; }
        IBaseRepo<UserLearningSession> UserLearningSessions { get; }
        IBaseRepo<UserCoursePoint> UserCoursePoints { get; }
        IBaseRepo<UserProgress> UserProgresses { get; }
        IBaseRepo<UserContentActivity> UserContentActivities { get; }

        // Learning and Progress
        IBaseRepo<UserBookmark> UserBookmarks { get; }
        IBaseRepo<UserLearningGoal> UserLearningGoals { get; }
        IBaseRepo<UserStudyPlan> UserStudyPlans { get; }
        IBaseRepo<StudySession> StudySessions { get; }
        IBaseRepo<StudySessionContent> StudySessionContents { get; }
        IBaseRepo<UserAchievement> UserAchievements { get; }
        IBaseRepo<Achievement> Achievements { get; }
        IBaseRepo<UserLearningStreak> UserLearningStreaks { get; }
        IBaseRepo<UserNotification> UserNotifications { get; }
        IBaseRepo<UserLearningAnalytics> UserLearningAnalytics { get; }

        // Communication & Administration
        IBaseRepo<Notification> Notifications { get; }
        IBaseRepo<AdminActionLog> AdminActionLogs { get; }
        IBaseRepo<SecurityAuditLog> SecurityAuditLogs { get; }

        // Quiz System - Specialized
        IQuizRepository Quizzes { get; }
        IQuestionRepository Questions { get; }
        IQuizAttemptRepository QuizAttempts { get; }

        // Quiz System - Basic
        IBaseRepo<QuizQuestion> QuizQuestions { get; }
        IBaseRepo<QuestionOption> QuestionOptions { get; }
        IBaseRepo<UserAnswer> UserAnswers { get; }

        // Point Transactions
        ICoursePointsRepository CoursePoints { get; }
        IPointTransactionRepository PointTransactions { get; }

        // Transaction Methods
        Task<int> CompleteAsync();
        int Complete();
        Task<int> SaveAsync();
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}