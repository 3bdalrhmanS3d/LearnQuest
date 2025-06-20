using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Models.UserManagement;
using LearnQuestV1.Core.Models.CourseStructure;
using LearnQuestV1.Core.Models.Financial;
using LearnQuestV1.Core.Models.CourseOrganization;
using LearnQuestV1.Core.Models.Communication;
using LearnQuestV1.Core.Models.Administration;
using LearnQuestV1.Core.Models.FeedbackAndReviews;
using LearnQuestV1.Core.Models.LearningAndProgress;
using LearnQuestV1.Core.Models.Quiz;

namespace LearnQuestV1.EF.Application
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        // User Management
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserDetail> UserDetails { get; set; } = null!;
        public DbSet<AccountVerification> AccountVerifications { get; set; } = null!;
        public DbSet<BlacklistToken> BlacklistTokens { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<UserVisitHistory> UserVisitHistory { get; set; } = null!;

        // Course Structure
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<AboutCourse> AboutCourses { get; set; } = null!;
        public DbSet<CourseSkill> CourseSkills { get; set; } = null!;
        public DbSet<Level> Levels { get; set; } = null!;
        public DbSet<Section> Sections { get; set; } = null!;
        public DbSet<Content> Contents { get; set; } = null!;

        // Course Organization
        public DbSet<CourseTrack> CourseTracks { get; set; } = null!;
        public DbSet<CourseTrackCourse> CourseTrackCourses { get; set; } = null!;
        public DbSet<CourseEnrollment> CourseEnrollments { get; set; } = null!;
        public DbSet<FavoriteCourse> FavoriteCourses { get; set; } = null!;

        // Feedback & Reviews
        public DbSet<CourseFeedback> CourseFeedbacks { get; set; } = null!;
        public DbSet<CourseReview> CourseReviews { get; set; } = null!;

        // Financial
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; } = null!;

        // Learning & Progress
        public DbSet<UserCoursePoint> UserCoursePoints { get; set; } = null!;
        public DbSet<UserProgress> UserProgresses { get; set; } = null!;
        public DbSet<CoursePoints> CoursePoints { get; set; } = null!;
        public DbSet<PointTransaction> PointTransactions { get; set; } = null!;
        public DbSet<UserAchievement> UserAchievements { get; set; } = null!;
        public DbSet<UserLearningStreak> UserLearningStreaks { get; set; } = null!;
        public DbSet<UserLearningAnalytics> UserLearningAnalytics { get; set; } = null!;

        // Notifications
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<UserNotification> UserNotifications { get; set; } = null!;

        // Content Activities & Logs
        public DbSet<UserContentActivity> UserContentActivities { get; set; }
        public DbSet<AdminActionLog> AdminActionLogs { get; set; }

        // Quiz System
        public DbSet<Quiz> Quizzes { get; set; } = null!;
        public DbSet<Question> Questions { get; set; } = null!;
        public DbSet<QuestionOption> QuestionOptions { get; set; } = null!;
        public DbSet<QuizQuestion> QuizQuestions { get; set; } = null!;
        public DbSet<QuizAttempt> QuizAttempts { get; set; } = null!;
        public DbSet<UserAnswer> UserAnswers { get; set; } = null!;
        public DbSet<SecurityAuditLog> SecurityAuditLogs { get; set; } = null!;
        public DbSet<Discount> Discounts { get; set; } = null!;
        public DbSet<UserPreferences> UserPreferences { get; set; } = null!;
        public DbSet<UserLearningSession> UserLearningSessions { get; set; } = null!;

        // Study Plans
        public DbSet<UserBookmark> UserBookmarks { get; set; } = null!;
        public DbSet<UserLearningGoal> UserLearningGoals { get; set; } = null!;
        public DbSet<UserStudyPlan> UserStudyPlans { get; set; } = null!;
        public DbSet<StudySession> StudySessions { get; set; } = null!;
        public DbSet<StudySessionContent> StudySessionContents { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ───────────────────────────────────────────────────────────────
            // 1) Unique index on EmailAddress in Users table
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<User>()
                .HasIndex(u => u.EmailAddress)
                .IsUnique();

            // ───────────────────────────────────────────────────────────────
            // 2) One-to-one between User and UserDetail
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserDetail)
                .WithOne(ud => ud.User)
                .HasForeignKey<UserDetail>(ud => ud.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 3) One-to-many between User and AccountVerification
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<AccountVerification>()
                .HasOne(av => av.User)
                .WithMany(u => u.AccountVerifications)
                .HasForeignKey(av => av.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 4) One-to-many between User and RefreshToken
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 5) One-to-many between User and UserVisitHistory
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<UserVisitHistory>()
                .HasOne(vh => vh.User)
                .WithMany(u => u.VisitHistories)
                .HasForeignKey(vh => vh.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 6) One-to-many between User and CourseEnrollment
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<CourseEnrollment>()
                .HasOne(ce => ce.User)
                .WithMany(u => u.CourseEnrollments)
                .HasForeignKey(ce => ce.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 7) One-to-many between Course and CourseEnrollment
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<CourseEnrollment>()
                .HasOne(ce => ce.Course)
                .WithMany(c => c.CourseEnrollments)
                .HasForeignKey(ce => ce.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 8) One-to-many between Course and AboutCourse
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<AboutCourse>()
                .HasOne(ac => ac.Course)
                .WithMany(c => c.AboutCourses)
                .HasForeignKey(ac => ac.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 9) One-to-many between Course and CourseSkill
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<CourseSkill>()
                .HasOne(cs => cs.Course)
                .WithMany(c => c.CourseSkills)
                .HasForeignKey(cs => cs.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 10) One-to-many between Course and Level
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<Level>()
                .HasOne(l => l.Course)
                .WithMany(c => c.Levels)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 11) One-to-many between Level and Section
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<Section>()
                .HasOne(s => s.Level)
                .WithMany(l => l.Sections)
                .HasForeignKey(s => s.LevelId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 12) One-to-many between Section and Content
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<Content>()
                .HasOne(ct => ct.Section)
                .WithMany(s => s.Contents)
                .HasForeignKey(ct => ct.SectionId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 13) One-to-many between User and CourseFeedback
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<CourseFeedback>()
                .HasOne(cf => cf.User)
                .WithMany(u => u.CourseFeedbacks)
                .HasForeignKey(cf => cf.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 14) One-to-many between Course and CourseFeedback
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<CourseFeedback>()
                .HasOne(cf => cf.Course)
                .WithMany(c => c.CourseFeedbacks)
                .HasForeignKey(cf => cf.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 15) One-to-many between User and CourseReview
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<CourseReview>()
                .HasOne(cr => cr.User)
                .WithMany(u => u.CourseReviews)
                .HasForeignKey(cr => cr.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 16) One-to-many between Course and CourseReview
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<CourseReview>()
                .HasOne(cr => cr.Course)
                .WithMany(c => c.CourseReviews)
                .HasForeignKey(cr => cr.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 17) One-to-many between User and Payment
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 18) One-to-many between Course and Payment
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Course)
                .WithMany(c => c.Payments)
                .HasForeignKey(p => p.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 19) One-to-many between User and UserCoursePoint
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<UserCoursePoint>()
                .HasOne(ucp => ucp.User)
                .WithMany(u => u.UserCoursePoints)
                .HasForeignKey(ucp => ucp.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 20) One-to-many between Course and UserCoursePoint
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<UserCoursePoint>()
                .HasOne(ucp => ucp.Course)
                .WithMany(c => c.UserCoursePoints)
                .HasForeignKey(ucp => ucp.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 21) One-to-many between User and UserProgress
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<UserProgress>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserProgresses)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 22) One-to-many between Course and UserProgress
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<UserProgress>()
                .HasOne(up => up.Course)
                .WithMany(c => c.UserProgresses)
                .HasForeignKey(up => up.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 23) One-to-many between Level and UserProgress (for CurrentLevel)
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<UserProgress>()
                .HasOne(up => up.CurrentLevel)
                .WithMany()
                .HasForeignKey(up => up.CurrentLevelId)
                .OnDelete(DeleteBehavior.Restrict);
            // Prevent deleting a Level that’s referenced by UserProgress

            // ───────────────────────────────────────────────────────────────
            // 24) One-to-many between Section and UserProgress (for CurrentSection)
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<UserProgress>()
                .HasOne(up => up.CurrentSection)
                .WithMany()
                .HasForeignKey(up => up.CurrentSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // ───────────────────────────────────────────────────────────────
            // 25) One-to-many between Content and UserProgress (for CurrentContent)
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<UserProgress>()
                .HasOne(up => up.CurrentContent)
                .WithMany()
                .HasForeignKey(up => up.CurrentContentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ───────────────────────────────────────────────────────────────
            // 26) One-to-many between User and FavoriteCourse
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<FavoriteCourse>()
                .HasOne(fc => fc.User)
                .WithMany(u => u.FavoriteCourses)
                .HasForeignKey(fc => fc.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 27) One-to-many between Course and FavoriteCourse
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<FavoriteCourse>()
                .HasOne(fc => fc.Course)
                .WithMany(c => c.FavoriteCourses)
                .HasForeignKey(fc => fc.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 28) Many-to-many between CourseTrack and Course via CourseTrackCourse
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<CourseTrackCourse>()
                .HasOne(ctc => ctc.CourseTrack)
                .WithMany(ct => ct.CourseTrackCourses)
                .HasForeignKey(ctc => ctc.TrackId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CourseTrackCourse>()
                .HasOne(ctc => ctc.Course)
                .WithMany(c => c.CourseTrackCourses)
                .HasForeignKey(ctc => ctc.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // ───────────────────────────────────────────────────────────────
            // 29) Additional indexes (if needed)
            // Example: Prevent duplicate enrollment in CourseEnrollments
            // ───────────────────────────────────────────────────────────────
            modelBuilder.Entity<CourseEnrollment>()
                .HasIndex(ce => new { ce.CourseId, ce.UserId })
                .IsUnique();


            modelBuilder.Entity<AdminActionLog>()
                .HasOne(a => a.Admin)
                .WithMany(u => u.AdminActionsPerformed)
                .HasForeignKey(a => a.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AdminActionLog>()
                .HasOne(a => a.TargetUser)
                .WithMany(u => u.AdminActionsReceived)
                .HasForeignKey(a => a.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SecurityAuditLog>()
                .HasOne(s => s.User)
                .WithMany(u => u.SecurityAuditLogs)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.SetNull);


            #region Quiz System Configuration

            // Quiz Configuration
            modelBuilder.Entity<Quiz>(entity =>
            {
                entity.HasKey(e => e.QuizId);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.QuizType)
                    .IsRequired()
                    .HasConversion<int>();

                entity.Property(e => e.MaxAttempts)
                    .IsRequired()
                    .HasDefaultValue(3);

                entity.Property(e => e.PassingScore)
                    .IsRequired()
                    .HasDefaultValue(70);

                entity.Property(e => e.IsRequired)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.IsDeleted)
                    .HasDefaultValue(false);

                // Relationships
                entity.HasOne(e => e.Course)
                    .WithMany()
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Instructor)
                    .WithMany()
                    .HasForeignKey(e => e.InstructorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Content)
                    .WithMany()
                    .HasForeignKey(e => e.ContentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Section)
                    .WithMany()
                    .HasForeignKey(e => e.SectionId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Level)
                    .WithMany()
                    .HasForeignKey(e => e.LevelId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Indexes
                entity.HasIndex(e => e.CourseId);
                entity.HasIndex(e => e.InstructorId);
                entity.HasIndex(e => e.QuizType);
                entity.HasIndex(e => new { e.ContentId, e.SectionId, e.LevelId });
            });

            // Question Configuration
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(e => e.QuestionId);

                entity.Property(e => e.QuestionText)
                    .IsRequired();

                entity.Property(e => e.QuestionType)
                    .IsRequired()
                    .HasConversion<int>();

                entity.Property(e => e.HasCode)
                    .HasDefaultValue(false);

                entity.Property(e => e.CodeSnippet)
                    .HasMaxLength(5000);

                entity.Property(e => e.ProgrammingLanguage)
                    .HasMaxLength(50);

                entity.Property(e => e.Points)
                    .IsRequired()
                    .HasDefaultValue(1);

                entity.Property(e => e.Explanation)
                    .HasMaxLength(1000);

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                // Relationships
                entity.HasOne(e => e.Course)
                    .WithMany()
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Instructor)
                    .WithMany()
                    .HasForeignKey(e => e.InstructorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Content)
                    .WithMany()
                    .HasForeignKey(e => e.ContentId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Indexes
                entity.HasIndex(e => e.CourseId);
                entity.HasIndex(e => e.InstructorId);
                entity.HasIndex(e => e.QuestionType);
            });

            // Question Option Configuration
            modelBuilder.Entity<QuestionOption>(entity =>
            {
                entity.HasKey(e => e.OptionId);

                entity.Property(e => e.OptionText)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.IsCorrect)
                    .IsRequired();

                entity.Property(e => e.OrderIndex)
                    .IsRequired();

                // Relationships
                entity.HasOne(e => e.Question)
                    .WithMany(q => q.QuestionOptions)
                    .HasForeignKey(e => e.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(e => e.QuestionId);
                entity.HasIndex(e => new { e.QuestionId, e.OrderIndex });
            });

            // Quiz Question Configuration (Many-to-Many)
            modelBuilder.Entity<QuizQuestion>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.OrderIndex)
                    .IsRequired();

                // Relationships
                entity.HasOne(e => e.Quiz)
                    .WithMany(q => q.QuizQuestions)
                    .HasForeignKey(e => e.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Question)
                    .WithMany(q => q.QuizQuestions)
                    .HasForeignKey(e => e.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint
                entity.HasIndex(e => new { e.QuizId, e.QuestionId })
                    .IsUnique();

                // Indexes
                entity.HasIndex(e => new { e.QuizId, e.OrderIndex });
            });

            // Quiz Attempt Configuration
            modelBuilder.Entity<QuizAttempt>(entity =>
            {
                entity.HasKey(e => e.AttemptId);

                entity.Property(e => e.Score)
                    .IsRequired();

                entity.Property(e => e.TotalPoints)
                    .IsRequired();

                entity.Property(e => e.Passed)
                    .IsRequired();

                entity.Property(e => e.StartedAt)
                    .IsRequired();

                entity.Property(e => e.AttemptNumber)
                    .IsRequired();

                // Computed column for ScorePercentage (read-only)
                entity.Ignore(e => e.ScorePercentage);

                // Relationships
                entity.HasOne(e => e.Quiz)
                    .WithMany(q => q.QuizAttempts)
                    .HasForeignKey(e => e.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.QuizId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.QuizId, e.UserId, e.AttemptNumber })
                    .IsUnique();
            });

            // User Answer Configuration
            modelBuilder.Entity<UserAnswer>(entity =>
            {
                entity.HasKey(e => e.UserAnswerId);

                entity.Property(e => e.IsCorrect)
                    .IsRequired();

                entity.Property(e => e.PointsEarned)
                    .IsRequired();

                entity.Property(e => e.AnsweredAt)
                    .IsRequired();

                // Relationships
                entity.HasOne(e => e.Attempt)
                    .WithMany(a => a.UserAnswers)
                    .HasForeignKey(e => e.AttemptId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Question)
                    .WithMany(q => q.UserAnswers)
                    .HasForeignKey(e => e.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.SelectedOption)
                    .WithMany(o => o.UserAnswers)
                    .HasForeignKey(e => e.SelectedOptionId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Unique constraint - one answer per question per attempt
                entity.HasIndex(e => new { e.AttemptId, e.QuestionId })
                    .IsUnique();

                // Indexes
                entity.HasIndex(e => e.AttemptId);
                entity.HasIndex(e => e.QuestionId);
            });

            #endregion


            // User ↔ UserLearningAnalytics
            modelBuilder.Entity<UserLearningAnalytics>()
                .HasOne(ula => ula.User)
                .WithMany(u => u.UserLearningAnalytics)
                .HasForeignKey(ula => ula.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // User ↔ UserLearningStreak
            modelBuilder.Entity<UserLearningStreak>()
                .HasOne(s => s.User)
                .WithMany(u => u.UserLearningStreaks)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // User ↔ UserStudyPlan
            modelBuilder.Entity<UserStudyPlan>()
                .HasOne(p => p.User)
                .WithMany(u => u.UserStudyPlans)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Course ↔ UserStudyPlan
            modelBuilder.Entity<UserStudyPlan>()
                .HasOne(p => p.Course)
                .WithMany(c => c.UserStudyPlans)
                .HasForeignKey(p => p.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // StudyPlan ↔ StudySession
            modelBuilder.Entity<StudySession>()
                .HasOne(s => s.StudyPlan)
                .WithMany(p => p.StudySessions)
                .HasForeignKey(s => s.StudyPlanId)
                .OnDelete(DeleteBehavior.NoAction);

            // StudySession ↔ StudySessionContent
            modelBuilder.Entity<StudySessionContent>()
                .HasOne(sc => sc.StudySession)
                .WithMany(s => s.Contents)
                .HasForeignKey(sc => sc.SessionId)
                .OnDelete(DeleteBehavior.NoAction);

            // Content ↔ StudySessionContent
            modelBuilder.Entity<StudySessionContent>()
                .HasOne(sc => sc.Content)
                .WithMany(c => c.StudySessionContents)
                .HasForeignKey(sc => sc.ContentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserLearningAnalytics>()
                .Property(a => a.AverageQuizScore)
                .HasPrecision(5, 2);

            modelBuilder.Entity<UserLearningAnalytics>()
                .Property(a => a.CompletionRate)
                .HasPrecision(5, 2);

            modelBuilder.Entity<UserLearningAnalytics>()
                .Property(a => a.DailyAverageSessionLength)
                .HasPrecision(5, 2);

            modelBuilder.Entity<UserStudyPlan>()
                .Property(p => p.PlanProgressPercentage)
                .HasPrecision(5, 2);

            #region Student Features Configuration

            // UserBookmark Configuration
            modelBuilder.Entity<UserBookmark>(entity =>
            {
                entity.HasKey(e => e.BookmarkId);

                entity.Property(e => e.BookmarkedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.Notes)
                    .HasMaxLength(1000);

                entity.Property(e => e.Tags)
                    .HasMaxLength(500);

                // Relationships
                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserBookmarks)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Content)
                    .WithMany(c => c.UserBookmarks)
                    .HasForeignKey(e => e.ContentId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ContentId);
                entity.HasIndex(e => new { e.UserId, e.ContentId })
                    .IsUnique();
            });

            // UserLearningGoal Configuration
            modelBuilder.Entity<UserLearningGoal>(entity =>
            {
                entity.HasKey(e => e.GoalId);

                entity.Property(e => e.GoalType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.GoalDescription)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.IsAchieved)
                    .HasDefaultValue(false);

                entity.Property(e => e.SendReminders)
                    .HasDefaultValue(true);

                entity.Property(e => e.PreferredStudyTime)
                    .HasMaxLength(200);

                entity.Property(e => e.PreferredStudyDays)
                    .HasMaxLength(500);

                // Relationships
                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserLearningGoals)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Course)
                    .WithMany(c => c.UserLearningGoals)
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CourseId);
                entity.HasIndex(e => e.IsActive);
            });

            // Achievement Configuration
            modelBuilder.Entity<Achievement>(entity =>
            {
                entity.HasKey(e => e.AchievementId);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.BadgeIcon)
                    .HasMaxLength(200);

                entity.Property(e => e.BadgeColor)
                    .HasMaxLength(50);

                entity.Property(e => e.Category)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.IsRare)
                    .HasDefaultValue(false);

                entity.Property(e => e.DefaultPoints)
                    .HasDefaultValue(0);

                entity.Property(e => e.Criteria)
                    .HasMaxLength(1000);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                // Indexes
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.IsActive);
            });

            // UserAchievement Configuration
            modelBuilder.Entity<UserAchievement>(entity =>
            {
                entity.HasKey(e => e.UserAchievementId);

                entity.Property(e => e.EarnedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.PointsAwarded)
                    .HasDefaultValue(0);

                // Relationships
                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserAchievements)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Achievement)
                    .WithMany(a => a.UserAchievements)
                    .HasForeignKey(e => e.AchievementId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Course)
                    .WithMany(c => c.UserAchievements)   // ربط بالعلاقة الصحيحة
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Indexes
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.AchievementId);
                entity.HasIndex(e => new { e.UserId, e.AchievementId })
                    .IsUnique();
            });

            // UserLearningStreak Configuration
            modelBuilder.Entity<UserLearningStreak>(entity =>
            {
                entity.HasKey(e => e.StreakId);

                entity.Property(e => e.CurrentStreak)
                    .HasDefaultValue(0);

                entity.Property(e => e.LongestStreak)
                    .HasDefaultValue(0);

                entity.Property(e => e.IsStreakActive)
                    .HasDefaultValue(false);

                entity.Property(e => e.WeeklyGoalDays)
                    .HasDefaultValue(5);

                entity.Property(e => e.CurrentWeekDays)
                    .HasDefaultValue(0);

                entity.Property(e => e.HasMetWeeklyGoal)
                    .HasDefaultValue(false);

                entity.Property(e => e.LastUpdated)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                // Relationships
                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserLearningStreaks)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(e => e.UserId)
                    .IsUnique(); // One streak record per user
            });

            // UserNotification Configuration
            modelBuilder.Entity<UserNotification>(entity =>
            {
                entity.HasKey(e => e.NotificationId);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.IsRead)
                    .HasDefaultValue(false);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.ActionUrl)
                    .HasMaxLength(500);

                entity.Property(e => e.Icon)
                    .HasMaxLength(100);

                entity.Property(e => e.Priority)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Normal");

                // Relationships
                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserNotifications)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Course)
                    .WithMany()
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Content)
                    .WithMany()
                    .HasForeignKey(e => e.ContentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Achievement)
                    .WithMany()
                    .HasForeignKey(e => e.AchievementId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Indexes
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.IsRead);
                entity.HasIndex(e => e.CreatedAt);
            });

            // UserLearningAnalytics Configuration
            modelBuilder.Entity<UserLearningAnalytics>(entity =>
            {
                entity.HasKey(e => e.AnalyticsId);

                entity.Property(e => e.AnalyticsDate)
                    .IsRequired()
                    .HasColumnType("date")
                    .HasDefaultValueSql("CAST(GETUTCDATE() AS DATE)");

                entity.Property(e => e.DailyLearningMinutes)
                    .HasDefaultValue(0);

                entity.Property(e => e.DailyContentCompleted)
                    .HasDefaultValue(0);

                entity.Property(e => e.DailySessions)
                    .HasDefaultValue(0);

                entity.Property(e => e.DailyAverageSessionLength)
                    .HasPrecision(5, 2)
                    .HasDefaultValue(0);

                entity.Property(e => e.PreferredLearningHour)
                    .HasMaxLength(20);

                entity.Property(e => e.MostActiveDay)
                    .HasMaxLength(20);

                entity.Property(e => e.PreferredContentType)
                    .HasMaxLength(50);

                entity.Property(e => e.CompletionRate)
                    .HasPrecision(5, 2)
                    .HasDefaultValue(0);

                entity.Property(e => e.AverageQuizScore)
                    .HasPrecision(5, 2)
                    .HasDefaultValue(0);

                entity.Property(e => e.TotalPointsEarned)
                    .HasDefaultValue(0);

                entity.Property(e => e.MetDailyGoal)
                    .HasDefaultValue(false);

                entity.Property(e => e.MetWeeklyGoal)
                    .HasDefaultValue(false);

                entity.Property(e => e.LastUpdated)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                // Relationships
                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserLearningAnalytics)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.AnalyticsDate);
                entity.HasIndex(e => new { e.UserId, e.AnalyticsDate })
                    .IsUnique(); // One analytics record per user per day
            });

            #endregion

            //ConfigureQuizEntities(modelBuilder);

            #region Additional Navigation Properties Configuration

            // Content ↔ UserContentActivity relationship
            modelBuilder.Entity<UserContentActivity>()
                .HasOne(uca => uca.Content)
                .WithMany(c => c.UserContentActivities)
                .HasForeignKey(uca => uca.ContentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Content ↔ UserBookmark relationship (already exists, but ensuring it's configured)
            modelBuilder.Entity<UserBookmark>()
                .HasOne(ub => ub.Content)
                .WithMany(c => c.UserBookmarks)
                .HasForeignKey(ub => ub.ContentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Content ↔ StudySessionContent relationship  
            modelBuilder.Entity<StudySessionContent>()
                .HasOne(ssc => ssc.Content)
                .WithMany(c => c.StudySessionContents)
                .HasForeignKey(ssc => ssc.ContentId)
                .OnDelete(DeleteBehavior.Restrict);

            // CoursePoints ↔ PointTransaction relationship
            modelBuilder.Entity<PointTransaction>()
                .HasOne(pt => pt.CoursePoints)
                .WithMany(cp => cp.PointTransactions)
                .HasForeignKey(pt => pt.CoursePointsId)
                .OnDelete(DeleteBehavior.Cascade);

            // User ↔ CoursePoints relationship
            modelBuilder.Entity<CoursePoints>()
                .HasOne(cp => cp.User)
                .WithMany() // User doesn't have navigation property to CoursePoints
                .HasForeignKey(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Course ↔ CoursePoints relationship
            modelBuilder.Entity<CoursePoints>()
                .HasOne(cp => cp.Course)
                .WithMany() // Course doesn't have navigation property to CoursePoints
                .HasForeignKey(cp => cp.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // User ↔ PointTransaction relationship
            modelBuilder.Entity<PointTransaction>()
                .HasOne(pt => pt.User)
                .WithMany() // User doesn't have navigation property to PointTransactions
                .HasForeignKey(pt => pt.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Course ↔ PointTransaction relationship
            modelBuilder.Entity<PointTransaction>()
                .HasOne(pt => pt.Course)
                .WithMany() // Course doesn't have navigation property to PointTransactions
                .HasForeignKey(pt => pt.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // QuizAttempt ↔ PointTransaction relationship (optional)
            modelBuilder.Entity<PointTransaction>()
                .HasOne(pt => pt.QuizAttempt)
                .WithMany() // QuizAttempt doesn't have navigation property to PointTransactions
                .HasForeignKey(pt => pt.QuizAttemptId)
                .OnDelete(DeleteBehavior.SetNull);

            // AwardedBy User ↔ PointTransaction relationship (optional)
            modelBuilder.Entity<PointTransaction>()
                .HasOne(pt => pt.AwardedBy)
                .WithMany() // User doesn't have navigation property for awarded transactions
                .HasForeignKey(pt => pt.AwardedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            #endregion
        }


    }
}
