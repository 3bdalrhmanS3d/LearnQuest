using AutoMapper;
using LearnQuestV1.Api.DTOs.Admin;
using LearnQuestV1.Api.DTOs.Courses;
using LearnQuestV1.Api.DTOs.Contents;
using LearnQuestV1.Api.DTOs.Instructor;
using LearnQuestV1.Api.DTOs.Levels;
using LearnQuestV1.Api.DTOs.Payments;
using LearnQuestV1.Api.DTOs.Profile;
using LearnQuestV1.Api.DTOs.Progress;
using LearnQuestV1.Api.DTOs.Sections;
using LearnQuestV1.Api.DTOs.Track;
using LearnQuestV1.Api.DTOs.User.Response;
using LearnQuestV1.Api.DTOs.Users.Request;
using LearnQuestV1.Api.DTOs.Users.Response;
using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Models.UserManagement;
using LearnQuestV1.Core.Models.CourseStructure;
using LearnQuestV1.Core.Models.CourseOrganization;
using LearnQuestV1.Core.Models.Communication;
using LearnQuestV1.Core.Models.Administration;
using LearnQuestV1.Core.Models.LearningAndProgress;

namespace LearnQuestV1.Api.Profiles
{
    public class ApplicationProfiles : Profile
    {
        public ApplicationProfiles()
        {
            CreateUserMappings();
            CreateCourseMappings();
            CreateProgressMappings();
            CreateAdminMappings();
            CreateInstructorMappings();
        }

        private void CreateUserMappings()
        {
            // User Profile Mappings
            CreateMap<User, UserProfileDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.UserDetail != null ? src.UserDetail.BirthDate : (DateTime?)null))
                .ForMember(dest => dest.Edu, opt => opt.MapFrom(src => src.UserDetail != null ? src.UserDetail.EducationLevel : null))
                .ForMember(dest => dest.National, opt => opt.MapFrom(src => src.UserDetail != null ? src.UserDetail.Nationality : null))
                .ForMember(dest => dest.Progress, opt => opt.MapFrom(src => src.UserProgresses));

            CreateMap<UserProgress, UserProgressDto>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName));

            // User Statistics
            CreateMap<UserProgress, CourseProgressDto>()
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
                .ForMember(dest => dest.ProgressPercentage, opt => opt.Ignore()); // Calculated in service

            // Notifications
            CreateMap<Notification, NotificationDto>();

            // Payment Related
            CreateMap<CourseEnrollment, MyCourseDto>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Course.Description));
        }

        private void CreateCourseMappings()
        {
            // Course Mappings
            CreateMap<Course, CourseCDto>()
                .ForMember(dest => dest.CourseImage, opt => opt.MapFrom(src => src.CourseImage ?? string.Empty));

            CreateMap<Course, CourseDetailsDto>()
                .ForMember(dest => dest.CourseImage, opt => opt.MapFrom(src => src.CourseImage ?? string.Empty))
                .ForMember(dest => dest.AboutCourses, opt => opt.MapFrom(src => src.AboutCourses))
                .ForMember(dest => dest.CourseSkills, opt => opt.MapFrom(src => src.CourseSkills));

            CreateMap<AboutCourse, AboutCourseItem>()
                .ForMember(dest => dest.OutcomeType, opt => opt.MapFrom(src => src.OutcomeType.ToString()));

            CreateMap<CourseSkill, AvailableSkillsDto>();

            CreateMap<CreateCourseDto, Course>()
                .ForMember(dest => dest.CourseId, opt => opt.Ignore())
                .ForMember(dest => dest.InstructorId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CourseImage, opt => opt.MapFrom(_ => "/uploads/courses/default.jpg"));

            // Level Mappings
            CreateMap<Level, LevelSummaryDto>();
            CreateMap<CreateLevelDto, Level>()
                .ForMember(dest => dest.LevelId, opt => opt.Ignore())
                .ForMember(dest => dest.LevelOrder, opt => opt.Ignore())
                .ForMember(dest => dest.RequiresPreviousLevelCompletion, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

            // Section Mappings
            CreateMap<Section, SectionSummaryDto>();
            CreateMap<CreateSectionDto, Section>()
                .ForMember(dest => dest.SectionId, opt => opt.Ignore())
                .ForMember(dest => dest.SectionOrder, opt => opt.Ignore())
                .ForMember(dest => dest.RequiresPreviousSectionCompletion, opt => opt.Ignore())
                .ForMember(dest => dest.IsVisible, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

            // Content Mappings
            CreateMap<Content, ContentSummaryDto>();
            CreateMap<CreateContentDto, Content>()
                .ForMember(dest => dest.ContentId, opt => opt.Ignore())
                .ForMember(dest => dest.ContentOrder, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsVisible, opt => opt.MapFrom(src => true));

            // Track Mappings
            CreateMap<CourseTrack, DTOs.Track.TrackDto>()
                .ForMember(dest => dest.CourseCount, opt => opt.Ignore()); // Calculated in service

            CreateMap<CourseTrack, TrackDetailsDto>()
                .ForMember(dest => dest.Courses, opt => opt.Ignore()); // Mapped in service

            CreateMap<Course, DTOs.Track.CourseInTrackDto>();

            CreateMap<CreateTrackRequestDto, CourseTrack>()
                .ForMember(dest => dest.TrackId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        }

        private void CreateProgressMappings()
        {
            // Progress Tracking DTOs
            CreateMap<CourseTrack, DTOs.Progress.TrackDto>()
                .ForMember(dest => dest.CourseCount, opt => opt.Ignore()); // Calculated in service

            CreateMap<Course, DTOs.Progress.CourseDto>()
                .ForMember(dest => dest.CourseImage, opt => opt.MapFrom(src => src.CourseImage ?? string.Empty));

            CreateMap<Course, DTOs.Progress.CourseInTrackDto>()
                .ForMember(dest => dest.CourseImage, opt => opt.MapFrom(src => src.CourseImage ?? string.Empty))
                .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Instructor.FullName))
                .ForMember(dest => dest.LevelsCount, opt => opt.MapFrom(src => src.Levels.Count));

            CreateMap<Level, DTOs.Progress.LevelDto>();

            CreateMap<Section, DTOs.Progress.SectionDto>()
                .ForMember(dest => dest.IsCurrent, opt => opt.Ignore()) // Calculated in service
                .ForMember(dest => dest.IsCompleted, opt => opt.Ignore()); // Calculated in service

            CreateMap<Content, DTOs.Progress.ContentDto>()
                .ForMember(dest => dest.ContentType, opt => opt.MapFrom(src => src.ContentType.ToString()))
                .ForMember(dest => dest.ContentText, opt => opt.MapFrom(src => src.ContentText ?? string.Empty))
                .ForMember(dest => dest.ContentDoc, opt => opt.MapFrom(src => src.ContentDoc ?? string.Empty))
                .ForMember(dest => dest.ContentUrl, opt => opt.MapFrom(src => src.ContentUrl ?? string.Empty))
                .ForMember(dest => dest.ContentDescription, opt => opt.MapFrom(src => src.ContentDescription ?? string.Empty));

            // Complex response DTOs
            CreateMap<Course, LevelsResponseDto>()
                .ForMember(dest => dest.CourseImage, opt => opt.MapFrom(src => src.CourseImage ?? string.Empty))
                .ForMember(dest => dest.LevelsCount, opt => opt.MapFrom(src => src.Levels.Count))
                .ForMember(dest => dest.Levels, opt => opt.MapFrom(src => src.Levels.Where(l => !l.IsDeleted && l.IsVisible).OrderBy(l => l.LevelOrder)));

            CreateMap<Level, SectionsResponseDto>()
                .ForMember(dest => dest.Sections, opt => opt.MapFrom(src => src.Sections.Where(s => !s.IsDeleted && s.IsVisible).OrderBy(s => s.SectionOrder)));

            CreateMap<Section, ContentsResponseDto>()
                .ForMember(dest => dest.Contents, opt => opt.MapFrom(src => src.Contents.Where(c => c.IsVisible).OrderBy(c => c.ContentOrder)));
        }

        private void CreateAdminMappings()
        {
            // Admin User DTOs
            CreateMap<User, AdminUserDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.IsVerified, opt => opt.MapFrom(src =>
                    src.AccountVerifications.Any(av => av.CheckedOK)));

            CreateMap<User, BasicUserInfoDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.UserDetail));

            CreateMap<UserDetail, BasicUserInfoDto.UserDetailDto>();

            // Admin Action Logs
            CreateMap<AdminActionLog, AdminActionLogDto>()
                .ForMember(dest => dest.AdminName, opt => opt.MapFrom(src => src.Admin.FullName))
                .ForMember(dest => dest.AdminEmail, opt => opt.MapFrom(src => src.Admin.EmailAddress))
                .ForMember(dest => dest.TargetUserName, opt => opt.MapFrom(src => src.TargetUser != null ? src.TargetUser.FullName : null))
                .ForMember(dest => dest.TargetUserEmail, opt => opt.MapFrom(src => src.TargetUser != null ? src.TargetUser.EmailAddress : null));
        }

        private void CreateInstructorMappings()
        {
            // Instructor Dashboard
            CreateMap<Course, CourseStatDto>()
                .ForMember(dest => dest.StudentCount, opt => opt.Ignore()) // Calculated in service
                .ForMember(dest => dest.ProgressCount, opt => opt.Ignore()); // Calculated in service

            CreateMap<Course, MostEngagedCourseDto>()
                .ForMember(dest => dest.ProgressCount, opt => opt.Ignore()); // Calculated in service

            // Stats DTOs
            CreateMap<Level, LevelStatsDto>()
                .ForMember(dest => dest.UsersReachedCount, opt => opt.Ignore()); // Calculated in service

            CreateMap<Section, SectionStatsDto>()
                .ForMember(dest => dest.UsersReached, opt => opt.Ignore()); // Calculated in service

            CreateMap<Content, ContentStatsDto>()
                .ForMember(dest => dest.UsersReached, opt => opt.Ignore()); // Calculated in service
        }
    }
}