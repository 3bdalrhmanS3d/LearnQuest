using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using LearnQuestV1.Core.Enums;

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

        [Required]
        [MaxLength(200)]
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

        [MaxLength(1000)]
        public string? ContentDescription { get; set; }

        public bool IsVisible { get; set; } = true;
    }
}