using static System.Collections.Specialized.BitVector32;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Core.Models
{
    [Table("UserProgress")]
    public class UserProgress
    {
        public UserProgress()
        {
            LastUpdated = DateTime.UtcNow;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserProgressId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [Required]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public virtual Course Course { get; set; } = null!;

        [Required]
        public int CurrentLevelId { get; set; }

        [ForeignKey(nameof(CurrentLevelId))]
        public virtual Level CurrentLevel { get; set; } = null!;

        [Required]
        public int CurrentSectionId { get; set; }

        [ForeignKey(nameof(CurrentSectionId))]
        public virtual Section CurrentSection { get; set; } = null!;

        public int? CurrentContentId { get; set; }

        [ForeignKey(nameof(CurrentContentId))]
        public virtual Content? CurrentContent { get; set; }

        [Required]
        public DateTime LastUpdated { get; set; }

    }
}