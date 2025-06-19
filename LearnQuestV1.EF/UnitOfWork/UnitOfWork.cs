using System;
using System.Threading.Tasks;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
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
using Microsoft.EntityFrameworkCore.Storage;

namespace LearnQuestV1.EF.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;

            // User Management
            Users = new BaseRepo<User>(context);
            UserDetails = new BaseRepo<UserDetail>(context);
            AccountVerifications = new BaseRepo<AccountVerification>(context);
            RefreshTokens = new BaseRepo<RefreshToken>(context);
            UserVisitHistories = new BaseRepo<UserVisitHistory>(context);
            BlacklistTokens = new BaseRepo<BlacklistToken>(context);

            // Course Structure
            Courses = new BaseRepo<Course>(context);
            AboutCourses = new BaseRepo<AboutCourse>(context);
            CourseSkills = new BaseRepo<CourseSkill>(context);
            Levels = new BaseRepo<Level>(context);
            Sections = new BaseRepo<Section>(context);
            Contents = new BaseRepo<Content>(context);

            // Course Organization
            CourseEnrollments = new BaseRepo<CourseEnrollment>(context);
            CourseFeedbacks = new BaseRepo<CourseFeedback>(context);
            CourseReviews = new BaseRepo<CourseReview>(context);
            CourseTracks = new BaseRepo<CourseTrack>(context);
            CourseTrackCourses = new BaseRepo<CourseTrackCourse>(context);
            FavoriteCourses = new BaseRepo<FavoriteCourse>(context);

            // Financial & Progress
            Payments = new BaseRepo<Payment>(context);
            UserCoursePoints = new BaseRepo<UserCoursePoint>(context);
            UserProgresses = new BaseRepo<UserProgress>(context);
            UserContentActivities = new BaseRepo<UserContentActivity>(context);

            // Communication & Administration
            Notifications = new BaseRepo<Notification>(context);
            AdminActionLogs = new BaseRepo<AdminActionLog>(context);
            SecurityAuditLogs = new BaseRepo<SecurityAuditLog>(context);

            // Quiz System Repositories (Specialized)
            Quizzes = new QuizRepository(context);
            Questions = new QuestionRepository(context);
            QuizAttempts = new QuizAttemptRepository(context);

            // Quiz System Basic Repositories
            QuizQuestions = new BaseRepo<QuizQuestion>(context);
            QuestionOptions = new BaseRepo<QuestionOption>(context);
            UserAnswers = new BaseRepo<UserAnswer>(context);
            PaymentTransactions = new BaseRepo<PaymentTransaction>(context);
            Discounts = new BaseRepo<Discount>(context);
            UserPreferences = new BaseRepo<UserPreferences>(context);
            UserAchievements = new BaseRepo<UserAchievement>(context);
            UserLearningSessions = new BaseRepo<UserLearningSession>(context);
            SecurityAuditLogs = new BaseRepo<SecurityAuditLog>(context);

            // User Enhancements
            UserBookmarks = new BaseRepo<UserBookmark>(_context);
            UserLearningGoals = new BaseRepo<UserLearningGoal>(_context);
            UserStudyPlans = new BaseRepo<UserStudyPlan>(_context);
            StudySessions = new BaseRepo<StudySession>(_context);
            StudySessionContents = new BaseRepo<StudySessionContent>(_context);
            UserAchievements = new BaseRepo<UserAchievement>(_context);
            Achievements = new BaseRepo<Achievement>(_context);
            UserLearningStreaks = new BaseRepo<UserLearningStreak>(_context);
            UserNotifications = new BaseRepo<UserNotification>(_context);
            UserLearningAnalytics = new BaseRepo<UserLearningAnalytics>(_context);

            CoursePoints = new CoursePointsRepository(context);
            PointTransactions = new PointTransactionRepository(context);
        }

        // User Management
        public IBaseRepo<User> Users { get; }
        public IBaseRepo<UserDetail> UserDetails { get; }
        public IBaseRepo<AccountVerification> AccountVerifications { get; }
        public IBaseRepo<RefreshToken> RefreshTokens { get; }
        public IBaseRepo<UserVisitHistory> UserVisitHistories { get; }
        public IBaseRepo<BlacklistToken> BlacklistTokens { get; }

        // Course Structure
        public IBaseRepo<Course> Courses { get; }
        public IBaseRepo<AboutCourse> AboutCourses { get; }
        public IBaseRepo<CourseSkill> CourseSkills { get; }
        public IBaseRepo<Level> Levels { get; }
        public IBaseRepo<Section> Sections { get; }
        public IBaseRepo<Content> Contents { get; }

        // Course Organization
        public IBaseRepo<CourseEnrollment> CourseEnrollments { get; }
        public IBaseRepo<CourseFeedback> CourseFeedbacks { get; }
        public IBaseRepo<CourseReview> CourseReviews { get; }
        public IBaseRepo<CourseTrack> CourseTracks { get; }
        public IBaseRepo<CourseTrackCourse> CourseTrackCourses { get; }
        public IBaseRepo<FavoriteCourse> FavoriteCourses { get; }

        // Financial & Progress
        public IBaseRepo<Payment> Payments { get; }
        public IBaseRepo<UserCoursePoint> UserCoursePoints { get; }
        public IBaseRepo<UserProgress> UserProgresses { get; }
        public IBaseRepo<UserContentActivity> UserContentActivities { get; }

        // Communication & Administration
        public IBaseRepo<Notification> Notifications { get; }
        public IBaseRepo<AdminActionLog> AdminActionLogs { get; }

        public IBaseRepo<SecurityAuditLog> SecurityAuditLogs { get; }

        // Quiz System Repositories (Specialized)
        public IQuizRepository Quizzes { get; }
        public IQuestionRepository Questions { get; }
        public IQuizAttemptRepository QuizAttempts { get; }

        // Quiz System Basic Repositories
        public IBaseRepo<QuizQuestion> QuizQuestions { get; }
        public IBaseRepo<QuestionOption> QuestionOptions { get; }
        public IBaseRepo<UserAnswer> UserAnswers { get; }

        public IBaseRepo<PaymentTransaction> PaymentTransactions { get; }
        public IBaseRepo<Discount> Discounts { get; }
        public IBaseRepo<UserPreferences> UserPreferences { get; }
        public IBaseRepo<UserAchievement> UserAchievements { get; }
        public IBaseRepo<UserLearningSession> UserLearningSessions { get; }

        public IBaseRepo<UserBookmark> UserBookmarks { get; private set; }
        public IBaseRepo<UserLearningGoal> UserLearningGoals { get; private set; }
        public IBaseRepo<UserStudyPlan> UserStudyPlans { get; private set; }
        public IBaseRepo<StudySession> StudySessions { get; private set; }
        public IBaseRepo<StudySessionContent> StudySessionContents { get; private set; }
        public IBaseRepo<Achievement> Achievements { get; private set; }
        public IBaseRepo<UserLearningStreak> UserLearningStreaks { get; private set; }
        public IBaseRepo<UserNotification> UserNotifications { get; private set; }
        public IBaseRepo<UserLearningAnalytics> UserLearningAnalytics { get; private set; }

        public ICoursePointsRepository CoursePoints { get; }
        public IPointTransactionRepository PointTransactions { get; }

        // Transaction Methods
        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}