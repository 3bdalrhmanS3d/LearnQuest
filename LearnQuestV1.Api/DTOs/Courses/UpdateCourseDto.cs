using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Courses
{
    public class UpdateCourseDto
    {
        [StringLength(200, ErrorMessage = "Course name cannot exceed 200 characters")]
        public string? CourseName { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Course price must be non-negative")]
        public decimal? CoursePrice { get; set; }

        public string? CourseImage { get; set; }
        public bool? IsActive { get; set; }

        public IEnumerable<AboutCourseInputDto>? AboutCourseInputs { get; set; }
        public IEnumerable<string>? CourseSkillInputs { get; set; } // Changed to string array for normalized skills
    }
}
