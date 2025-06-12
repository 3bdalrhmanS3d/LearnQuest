using LearnQuestV1.Core.Models.CourseOrganization;
using LearnQuestV1.Core.Models.FeedbackAndReviews;
using LearnQuestV1.Core.Models.Financial;
using LearnQuestV1.Core.Models.LearningAndProgress;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Models.CourseStructure
{
    [Table("Courses")]
    public class Course
    {
        public Course()
        {
            CreatedAt = DateTime.UtcNow;
            IsActive = false;
            IsDeleted = false;

            AboutCourses = new HashSet<AboutCourse>();
            CourseSkills = new HashSet<CourseSkill>();
            Levels = new HashSet<Level>();
            UserProgresses = new HashSet<UserProgress>();
            CourseEnrollments = new HashSet<CourseEnrollment>();
            CourseReviews = new HashSet<CourseReview>();
            UserCoursePoints = new HashSet<UserCoursePoint>();
            Payments = new HashSet<Payment>();
            CourseFeedbacks = new HashSet<CourseFeedback>();
            FavoriteCourses = new HashSet<FavoriteCourse>();
            CourseTrackCourses = new HashSet<CourseTrackCourse>();
        }


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CourseId { get; set; }

        [Required]
        [MaxLength(200)]
        public string CourseName { get; set; } = string.Empty;

        /// <summary>
        /// Foreign key → the instructor (User) who created this course.
        /// </summary>
        [Required]
        public int InstructorId { get; set; }

        [ForeignKey(nameof(InstructorId))]
        public virtual User Instructor { get; set; } = null!;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }

        [MaxLength(500)]
        public string? CourseImage { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CoursePrice { get; set; }

        public bool IsActive { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        public virtual ICollection<AboutCourse> AboutCourses { get; set; }
        public virtual ICollection<CourseSkill> CourseSkills { get; set; }
        public virtual ICollection<Level> Levels { get; set; }
        public virtual ICollection<UserProgress> UserProgresses { get; set; }
        public virtual ICollection<CourseEnrollment> CourseEnrollments { get; set; }
        public virtual ICollection<CourseReview> CourseReviews { get; set; }
        public virtual ICollection<UserCoursePoint> UserCoursePoints { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<CourseFeedback> CourseFeedbacks { get; set; }
        public virtual ICollection<FavoriteCourse> FavoriteCourses { get; set; }
        public virtual ICollection<CourseTrackCourse> CourseTrackCourses { get; set; }
    }
}
