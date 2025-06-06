namespace LearnQuestV1.Api.DTOs.Courses
{
    public class CourseDetailsDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CourseImage { get; set; } = string.Empty;
        public decimal CoursePrice { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public IEnumerable<AboutCourseItem> AboutCourses { get; set; } = new List<AboutCourseItem>();
        public IEnumerable<CourseSkillItem> CourseSkills { get; set; } = new List<CourseSkillItem>();

    }

    public class AboutCourseItem
    {
        public int AboutCourseId { get; set; }
        public string AboutCourseText { get; set; } = string.Empty;
        public string OutcomeType { get; set; } = string.Empty;
    }

    public class CourseSkillItem
    {
        public int CourseSkillId { get; set; }
        public string CourseSkillText { get; set; } = string.Empty;
    }
}
