using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Models.CourseStructure
{
    [Table("Sections")]
    public class Section
    {
        public Section()
        {
            Contents = new HashSet<Content>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SectionId { get; set; }

        [Required]
        public int LevelId { get; set; }

        [ForeignKey(nameof(LevelId))]
        public virtual Level Level { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string SectionName { get; set; } = string.Empty;

        public int SectionOrder { get; set; }

        public bool IsDeleted { get; set; } = false;
        public bool IsVisible { get; set; } = true;
        public bool RequiresPreviousSectionCompletion { get; set; } = false;

        public virtual ICollection<Content> Contents { get; set; }

    }
}
