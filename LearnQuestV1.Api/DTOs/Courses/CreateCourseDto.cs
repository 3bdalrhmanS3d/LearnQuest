using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Courses
{
    public class CreateCourseDto
    {
        [Required] public string CourseName { get; set; } = string.Empty;
        [Required] public string Description { get; set; } = string.Empty;
        [Range(0, double.MaxValue)] public decimal CoursePrice { get; set; }
        public string? CourseImage { get; set; }
        public bool IsActive { get; set; } = false;

        public List<AboutCourseInput>? AboutCourseInputs { get; set; }
        public List<CourseSkillInput>? CourseSkillInputs { get; set; }
    }

    public class AboutCourseInput
    {
        public int AboutCourseId { get; set; } // zero for new
        [Required] public string AboutCourseText { get; set; } = string.Empty;
        public string OutcomeType { get; set; } = "Learn";
    }

    public class CourseSkillInput
    {
        public int CourseSkillId { get; set; } // zero for new
        [Required] public string CourseSkillText { get; set; } = string.Empty;
    }
}
