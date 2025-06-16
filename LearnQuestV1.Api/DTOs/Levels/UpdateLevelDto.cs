using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Levels
{
    public class UpdateLevelDto
    {
        [Required(ErrorMessage = "Level ID is required")]
        public int LevelId { get; set; }

        [StringLength(200, ErrorMessage = "Level name cannot exceed 200 characters")]
        public string? LevelName { get; set; }

        [StringLength(1000, ErrorMessage = "Level details cannot exceed 1000 characters")]
        public string? LevelDetails { get; set; }

        public bool? IsVisible { get; set; }

        public bool? RequiresPreviousLevelCompletion { get; set; }
    }
}
