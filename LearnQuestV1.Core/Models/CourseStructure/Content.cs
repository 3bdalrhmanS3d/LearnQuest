using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Models.LearningAndProgress;
using LearnQuestV1.Core.Models.UserManagement;

namespace LearnQuestV1.Core.Models.CourseStructure
{
    [Table("Contents")]
    public class Content
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ContentId { get; set; }

        [Required]
        public int SectionId { get; set; }

        [ForeignKey(nameof(SectionId))]
        public virtual Section Section { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public ContentType ContentType { get; set; } = ContentType.Text;

        [MaxLength(500)]
        public string? ContentUrl { get; set; }

        public string? ContentText { get; set; }
        public string? ContentDoc { get; set; }

        public int DurationInMinutes { get; set; } = 0;
        public int ContentOrder { get; set; } = 0;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [MaxLength(1000)]
        public string? ContentDescription { get; set; }

        public bool IsVisible { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        public virtual ICollection<UserBookmark> UserBookmarks { get; set; } = new List<UserBookmark>();
        public virtual ICollection<StudySessionContent> StudySessionContents { get; set; } = new List<StudySessionContent>();

        public virtual ICollection<UserContentActivity> UserContentActivities { get; set; } = new List<UserContentActivity>();

    }
}
