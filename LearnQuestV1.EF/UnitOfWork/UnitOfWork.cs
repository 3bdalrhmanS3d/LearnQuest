using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models.Administration;
using LearnQuestV1.Core.Models.Communication;
using LearnQuestV1.Core.Models.CourseOrganization;
using LearnQuestV1.Core.Models.CourseStructure;
using LearnQuestV1.Core.Models.FeedbackAndReviews;
using LearnQuestV1.Core.Models.Financial;
using LearnQuestV1.Core.Models.LearningAndProgress;
using LearnQuestV1.Core.Models.Quiz;
using LearnQuestV1.Core.Models.UserManagement;
using LearnQuestV1.EF.Application;
using LearnQuestV1.EF.Repositories;
using LearnQuestV1.Core.Models;

namespace LearnQuestV1.EF.UnitOfWork
{
    public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
    {

        // User Management
        public IBaseRepo<User> Users { get; } = new BaseRepo<User>(context);
        public IBaseRepo<UserDetail> UserDetails { get; } = new BaseRepo<UserDetail>(context);
        public IBaseRepo<AccountVerification> AccountVerifications { get; } = new BaseRepo<AccountVerification>(context);
        public IBaseRepo<RefreshToken> RefreshTokens { get; } = new BaseRepo<RefreshToken>(context);
        public IBaseRepo<UserVisitHistory> UserVisitHistories { get; } = new BaseRepo<UserVisitHistory>(context);
        public IBaseRepo<BlacklistToken> BlacklistTokens { get; } = new BaseRepo<BlacklistToken>(context);

        // Course Structure
        public IBaseRepo<Course> Courses { get; } = new BaseRepo<Course>(context);
        public IBaseRepo<AboutCourse> AboutCourses { get; } = new BaseRepo<AboutCourse>(context);
        public IBaseRepo<CourseSkill> CourseSkills { get; } = new BaseRepo<CourseSkill>(context);
        public IBaseRepo<Level> Levels { get; } = new BaseRepo<Level>(context);
        public IBaseRepo<Section> Sections { get; } = new BaseRepo<Section>(context);
        public IBaseRepo<Content> Contents { get; } = new BaseRepo<Content>(context);

        // Course Organization
        public IBaseRepo<CourseEnrollment> CourseEnrollments { get; } = new BaseRepo<CourseEnrollment>(context);
        public IBaseRepo<FavoriteCourse> FavoriteCourses { get; } = new BaseRepo<FavoriteCourse>(context);
        public IBaseRepo<CourseTrack> CourseTracks { get; } = new BaseRepo<CourseTrack>(context);
        public IBaseRepo<CourseTrackCourse> CourseTrackCourses { get; } = new BaseRepo<CourseTrackCourse>(context);

        // Financial & Progress
        public IBaseRepo<Payment> Payments { get; } = new BaseRepo<Payment>(context);
        public IBaseRepo<PaymentTransaction> PaymentTransactions { get; } = new BaseRepo<PaymentTransaction>(context);
        public IBaseRepo<Discount> Discounts { get; } = new BaseRepo<Discount>(context);
        public IBaseRepo<UserPreferences> UserPreferences { get; } = new BaseRepo<UserPreferences>(context);
        public IBaseRepo<UserLearningSession> UserLearningSessions { get; } = new BaseRepo<UserLearningSession>(context);
        public IBaseRepo<UserCoursePoint> UserCoursePoints { get; } = new BaseRepo<UserCoursePoint>(context);
        public IBaseRepo<UserProgress> UserProgresses { get; } = new BaseRepo<UserProgress>(context);
        public IBaseRepo<UserContentActivity> UserContentActivities { get; } = new BaseRepo<UserContentActivity>(context);

        // Administration & Communication
        public IBaseRepo<Notification> Notifications { get; } = new BaseRepo<Notification>(context);
        public IBaseRepo<AdminActionLog> AdminActionLogs { get; } = new BaseRepo<AdminActionLog>(context);
        public IBaseRepo<SecurityAuditLog> SecurityAuditLogs { get; } = new BaseRepo<SecurityAuditLog>(context);

        // Quiz System - Specialized
        public IQuizRepository Quizzes { get; } = new QuizRepository(context);
        public IQuestionRepository Questions { get; } = new QuestionRepository(context);
        public IQuizAttemptRepository QuizAttempts { get; } = new QuizAttemptRepository(context);

        // Quiz System - Basic
        public IBaseRepo<QuizQuestion> QuizQuestions { get; } = new BaseRepo<QuizQuestion>(context);
        public IBaseRepo<QuestionOption> QuestionOptions { get; } = new BaseRepo<QuestionOption>(context);
        public IBaseRepo<UserAnswer> UserAnswers { get; } = new BaseRepo<UserAnswer>(context);

        // Learning & Progress - Additional
        public IBaseRepo<UserAchievement> UserAchievements { get; } = new BaseRepo<UserAchievement>(context);
        public IBaseRepo<Achievement> Achievements { get; } = new BaseRepo<Achievement>(context);
        public IBaseRepo<UserLearningStreak> UserLearningStreaks { get; } = new BaseRepo<UserLearningStreak>(context);
        public IBaseRepo<UserNotification> UserNotifications { get; } = new BaseRepo<UserNotification>(context);
        public IBaseRepo<UserLearningAnalytics> UserLearningAnalytics { get; } = new BaseRepo<UserLearningAnalytics>(context);

        // Study Plans
        public IBaseRepo<UserBookmark> UserBookmarks { get; } = new BaseRepo<UserBookmark>(context);
        public IBaseRepo<UserLearningGoal> UserLearningGoals { get; } = new BaseRepo<UserLearningGoal>(context);
        public IBaseRepo<UserStudyPlan> UserStudyPlans { get; } = new BaseRepo<UserStudyPlan>(context);
        public IBaseRepo<StudySession> StudySessions { get; } = new BaseRepo<StudySession>(context);
        public IBaseRepo<StudySessionContent> StudySessionContents { get; } = new BaseRepo<StudySessionContent>(context);

        // Point Transactions
        public ICoursePointsRepository CoursePoints { get; } = new CoursePointsRepository(context);
        public IPointTransactionRepository PointTransactions { get; } = new PointTransactionRepository(context);

        // Reviews and Feedback
        public IBaseRepo<CourseFeedback> CourseFeedbacks { get; } = new BaseRepo<CourseFeedback>(context);
        public IBaseRepo<CourseReview> CourseReviews { get; } = new BaseRepo<CourseReview>(context);
        // Transaction Methods
        public async Task<int> SaveAsync() => await context.SaveChangesAsync();
        public async Task<int> SaveChangesAsync() => await context.SaveChangesAsync();
        public async Task<IDbContextTransaction> BeginTransactionAsync() => await context.Database.BeginTransactionAsync();

        public void Dispose() => context.Dispose();
    }
}
