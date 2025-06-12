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

namespace LearnQuestV1.EF.Application
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserDetail> UserDetails { get; set; } = null!;
        public DbSet<AccountVerification> AccountVerifications { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<UserVisitHistory> UserVisitHistory { get; set; } = null!;
        public DbSet<BlacklistToken> BlacklistTokens { get; set; } = null!;

        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<AboutCourse> AboutCourses { get; set; } = null!;
        public DbSet<CourseSkill> CourseSkills { get; set; } = null!;
        public DbSet<Level> Levels { get; set; } = null!;
        public DbSet<Section> Sections { get; set; } = null!;
        public DbSet<Content> Contents { get; set; } = null!;

        public DbSet<CourseEnrollment> CourseEnrollments { get; set; } = null!;
        public DbSet<CourseFeedback> CourseFeedbacks { get; set; } = null!;
        public DbSet<CourseReview> CourseReviews { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<UserCoursePoint> UserCoursePoints { get; set; } = null!;
        public DbSet<UserProgress> UserProgresses { get; set; } = null!;
        public DbSet<FavoriteCourse> FavoriteCourses { get; set; } = null!;
        public DbSet<CourseTrack> CourseTracks { get; set; } = null!;
        public DbSet<CourseTrackCourse> CourseTrackCourses { get; set; } = null!;

        public DbSet<Notification> Notifications { get; set; } = null!;

        public DbSet<UserContentActivity> UserContentActivities { get; set; }
        public DbSet<AdminActionLog> AdminActionLogs { get; set; }

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
        }
    }
}
