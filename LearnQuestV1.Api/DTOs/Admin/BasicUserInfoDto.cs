namespace LearnQuestV1.Api.DTOs.Admin
{
    public class BasicUserInfoDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; } // From VisitHistory
        public string? ProfilePhoto { get; set; }
        public bool IsSystemProtected { get; set; }
        public bool IsVerified { get; set; }
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
