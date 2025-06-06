namespace LearnQuestV1.Api.DTOs.Admin
{
    public class SystemStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActivatedUsers { get; set; }
        public int NotActivatedUsers { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalCourses { get; set; }
    }
}
