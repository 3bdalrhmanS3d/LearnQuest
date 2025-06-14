﻿namespace LearnQuestV1.Api.DTOs.Admin
{
    public class AdminUserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string? ProfilePhoto { get; set; }
        public bool IsSystemProtected { get; set; }
    }
}
