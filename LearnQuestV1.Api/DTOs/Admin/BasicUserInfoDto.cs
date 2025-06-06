namespace LearnQuestV1.Api.DTOs.Admin
{
    public class BasicUserInfoDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public UserDetailDto? Details { get; set; }

        public class UserDetailDto
        {
            public DateTime BirthDate { get; set; }
            public string EducationLevel { get; set; } = string.Empty;
            public string Nationality { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }
    }
}
