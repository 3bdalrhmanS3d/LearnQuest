namespace LearnQuestV1.Api.DTOs.Courses
{
    public class UpdateCourseDto
    {
        public string? CourseName { get; set; }
        public string? Description { get; set; }
        public decimal? CoursePrice { get; set; }
        public string? CourseImage { get; set; }
        public bool? IsActive { get; set; }

        public List<AboutCourseInput>? AboutCourseInputs { get; set; }
        public List<CourseSkillInput>? CourseSkillInputs { get; set; }
    }
}
