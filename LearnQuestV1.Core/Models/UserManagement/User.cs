﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Models.Administration;
using LearnQuestV1.Core.Models.CourseOrganization;
using LearnQuestV1.Core.Models.CourseStructure;
using LearnQuestV1.Core.Models.FeedbackAndReviews;
using LearnQuestV1.Core.Models.Financial;
using LearnQuestV1.Core.Models.LearningAndProgress;
using LearnQuestV1.Core.Models.UserManagement;

namespace LearnQuestV1.Core.Models
{

    [Table("Users")]
    public class User
    {
        public User()
        {
            // Initialize all collections to avoid null references.
            AccountVerifications = new HashSet<AccountVerification>();
            RefreshTokens = new HashSet<RefreshToken>();
            VisitHistories = new HashSet<UserVisitHistory>();
            CoursesTaught = new HashSet<Course>();
            CourseEnrollments = new HashSet<CourseEnrollment>();
            CourseFeedbacks = new HashSet<CourseFeedback>();
            CourseReviews = new HashSet<CourseReview>();
            Payments = new HashSet<Payment>();
            UserCoursePoints = new HashSet<UserCoursePoint>();
            UserProgresses = new HashSet<UserProgress>();
            FavoriteCourses = new HashSet<FavoriteCourse>();
            AdminActionsPerformed = new HashSet<AdminActionLog>();
            AdminActionsReceived = new HashSet<AdminActionLog>();
            SecurityAuditLogs = new HashSet<SecurityAuditLog>();
            UserBookmarks = new HashSet<UserBookmark>();
            UserLearningGoals = new HashSet<UserLearningGoal>();
            UserStudyPlans = new HashSet<UserStudyPlan>();
            UserAchievements = new HashSet<UserAchievement>();
            UserLearningStreaks = new HashSet<UserLearningStreak>();
            UserNotifications = new HashSet<UserNotification>();
            UserLearningAnalytics = new HashSet<UserLearningAnalytics>();
            
            CreatedAt = DateTime.UtcNow;
            IsActive = false;
            IsDeleted = false;
            Role = UserRole.RegularUser;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string EmailAddress { get; set; } = string.Empty;


        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp indicating when the user account was created.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        [DefaultValue(UserRole.RegularUser)]
        public UserRole Role { get; set; }

        /// <summary>
        /// Optional URL or path to the user’s profile photo.
        /// </summary>
        [MaxLength(500)]
        public string? ProfilePhoto { get; set; }

        /// <summary>
        /// Indicates if the user account is protected by the system (e.g., cannot be deleted).
        /// </summary>
        public bool IsSystemProtected { get; set; } = false;


        /// <summary>
        /// Indicates if the user account is active (able to log in).
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Soft-delete flag. If true, the user is considered “deleted” without removing the row.
        /// </summary>
        public bool IsDeleted { get; set; }


        /// <summary>
        /// One-to-one: additional details for this user.
        /// Required navigation if you always want a detail record.
        /// </summary>
        public virtual UserDetail? UserDetail { get; set; }

        /// <summary>
        /// One-to-many: track every verification attempt for this user.
        /// If you only ever want one “live” verification, you could change this to a single navigation.
        /// </summary>
        public virtual ICollection<AccountVerification> AccountVerifications { get; set; }

        /// <summary>
        /// One-to-many: all refresh tokens issued to this user.
        /// </summary>
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }

        /// <summary>
        /// One-to-many: visit-history records for this user.
        /// </summary>
        public virtual ICollection<UserVisitHistory> VisitHistories { get; set; }

        /// <summary>
        /// One-to-many: courses taught by this user (if the user is an Instructor).
        /// </summary>
        public virtual ICollection<Course> CoursesTaught { get; set; }

        /// <summary>
        /// One-to-many: course enrollment records for this user.
        /// </summary>
        public virtual ICollection<CourseEnrollment> CourseEnrollments { get; set; }

        /// <summary>
        /// One-to-many: feedback entries that this user has left on courses.
        /// </summary>
        public virtual ICollection<CourseFeedback> CourseFeedbacks { get; set; }

        /// <summary>
        /// One-to-many: reviews that this user has written about courses.
        /// </summary>
        public virtual ICollection<CourseReview> CourseReviews { get; set; }

        /// <summary>
        /// One-to-many: payment records made by this user.
        /// </summary>
        public virtual ICollection<Payment> Payments { get; set; }

        /// <summary>
        /// One-to-many: points that this user has accumulated in courses.
        /// </summary>
        public virtual ICollection<UserCoursePoint> UserCoursePoints { get; set; }

        /// <summary>
        /// One-to-many: progress tracking entries for this user.
        /// </summary>
        public virtual ICollection<UserProgress> UserProgresses { get; set; }

        /// <summary>
        /// One-to-many: favorite courses for this user.
        /// </summary>
        public virtual ICollection<FavoriteCourse> FavoriteCourses { get; set; }

        /// <summary>
        /// All admin‐action logs where this user acted as the Admin (the actor).
        /// </summary>
        public virtual ICollection<AdminActionLog> AdminActionsPerformed { get; set; }

        /// <summary>
        /// All admin‐action logs where this user was the TargetUser (the “victim” of the action).
        /// </summary>
        public virtual ICollection<AdminActionLog> AdminActionsReceived { get; set; }

        public virtual ICollection<SecurityAuditLog> SecurityAuditLogs { get; set; }

        public virtual ICollection<UserBookmark> UserBookmarks { get; set; } = new List<UserBookmark>();
        public virtual ICollection<UserLearningGoal> UserLearningGoals { get; set; } = new List<UserLearningGoal>();
        public virtual ICollection<UserStudyPlan> UserStudyPlans { get; set; } = new List<UserStudyPlan>();
        public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
        public virtual ICollection<UserLearningStreak> UserLearningStreaks { get; set; } = new List<UserLearningStreak>();
        public virtual ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
        public virtual ICollection<UserLearningAnalytics> UserLearningAnalytics { get; set; } = new List<UserLearningAnalytics>();

    }
}
