using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Levels
{
    public class ReorderLevelDto
    {
        [Required]
        public int LevelId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "New order must be greater than 0")]
        public int NewOrder { get; set; }
    }
}
