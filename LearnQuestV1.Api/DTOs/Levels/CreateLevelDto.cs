using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Levels
{
    public class CreateLevelDto
    {

        [Required(ErrorMessage = "Course ID is required")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Level name is required")]
        [StringLength(200, ErrorMessage = "Level name cannot exceed 200 characters")]
        public string LevelName { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Level details cannot exceed 1000 characters")]
        public string? LevelDetails { get; set; }

        public bool IsVisible { get; set; } = true;

        [Range(1, int.MaxValue, ErrorMessage = "Level order must be greater than 0")]
        public int? LevelOrder { get; set; } // Optional, will auto-assign if null
    }
}
