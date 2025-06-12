using static System.Collections.Specialized.BitVector32;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Core.Models.CourseStructure
{
    [Table("Levels")]
    public class Level
    {
        public Level()
        {
            Sections = new HashSet<Section>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LevelId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public virtual Course Course { get; set; } = null!;

        public int LevelOrder { get; set; }

        [Required]
        [MaxLength(200)]
        public string LevelName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string LevelDetails { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;
        public bool IsVisible { get; set; } = true;
        public bool RequiresPreviousLevelCompletion { get; set; } = false;

        public virtual ICollection<Section> Sections { get; set; }
    }
}