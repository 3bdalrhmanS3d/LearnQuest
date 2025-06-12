LearnQuestV1.sln
│
├── 📦 LearnQuestV1.Core/ (طبقة النطاق - Domain Layer)
│   ├── 📁 Enums/
│   │   ├── UserRole.cs (RegularUser, Instructor, Admin)
│   │   ├── ContentType.cs (Text, Video, Doc)
│   │   ├── CourseOutcomeType.cs (Learn, Expertise)
│   │   ├── NotificationTemplateType.cs
│   │   └── PaymentStatus.cs (Pending, Completed, Failed)
│   │
│   ├── 📁 Models/
│   │   ├── 👤 User Management
│   │   │   ├── User.cs
│   │   │   ├── UserDetail.cs
│   │   │   ├── AccountVerification.cs
│   │   │   ├── RefreshToken.cs
│   │   │   ├── BlacklistToken.cs
│   │   │   └── UserVisitHistory.cs
│   │   │
│   │   ├── 📚 Course Structure
│   │   │   ├── Course.cs
│   │   │   ├── AboutCourse.cs
│   │   │   ├── CourseSkill.cs
│   │   │   ├── Level.cs
│   │   │   ├── Section.cs
│   │   │   └── Content.cs
│   │   │
│   │   ├── 🎯 Learning & Progress
│   │   │   ├── CourseEnrollment.cs
│   │   │   ├── UserProgress.cs
│   │   │   ├── UserContentActivity.cs
│   │   │   └── UserCoursePoint.cs
│   │   │
│   │   ├── 💰 Financial
│   │   │   └── Payment.cs
│   │   │
│   │   ├── 🗂️ Course Organization
│   │   │   ├── CourseTrack.cs
│   │   │   ├── CourseTrackCourse.cs
│   │   │   └── FavoriteCourse.cs
│   │   │
│   │   ├── 📢 Communication
│   │   │   └── Notification.cs
│   │   │
│   │   ├── 📝 Feedback & Reviews
│   │   │   ├── CourseFeedback.cs
│   │   │   └── CourseReview.cs
│   │   │
│   │   └── 🛡️ Administration
│   │       └── AdminActionLog.cs
│   │
│   └── 📁 Interfaces/
│       ├── IBaseRepo<T>.cs
│       └── IUnitOfWork.cs
│
├── 📦 LearnQuestV1.EF/ (طبقة الوصول للبيانات - Data Access Layer)
│   ├── 📁 Application/
│   │   └── ApplicationDbContext.cs
│   │
│   ├── 📁 Repositories/
│   │   └── BaseRepo<T>.cs
│   │
│   ├── 📁 UnitOfWork/
│   │   └── UnitOfWork.cs
│   │
│   └── 📁 Migrations/
│       ├── 20250603142209_init.cs
│       ├── 20250604174924_updateModels.cs
│       ├── 20250604194443_updateModels2.cs
│       └── 20250606021433_AddAdminActionLogTable.cs
│
└── 📦 LearnQuestV1.Api/ (طبقة العرض - Presentation Layer)
    ├── 🎮 Controllers/ (10 وحدات تحكم)
    │   ├── 🔐 Authentication & User
    │   │   ├── AuthController.cs (Signup, Signin, Verify, Logout)
    │   │   ├── ProfileController.cs (User Profile Management)
    │   │   └── ProgressController.cs (Learning Progress)
    │   │
    │   ├── 👨‍🏫 Instructor Management
    │   │   ├── DashboardController.cs (Instructor Dashboard)
    │   │   ├── TrackController.cs (Course Tracks)
    │   │   ├── CourseController.cs (Course CRUD)
    │   │   ├── LevelController.cs (Course Levels)
    │   │   ├── SectionController.cs (Course Sections)
    │   │   └── ContentController.cs (Course Content)
    │   │
    │   └── 👮 Administration
    │       └── AdminController.cs (Admin Operations)
    │
    ├── 📋 DTOs/ (نماذج نقل البيانات)
    │   ├── 📁 Users/
    │   │   ├── Request/
    │   │   │   ├── SignupRequestDto.cs
    │   │   │   ├── SigninRequestDto.cs
    │   │   │   ├── VerifyAccountRequestDto.cs
    │   │   │   ├── RefreshTokenRequestDto.cs
    │   │   │   ├── ForgetPasswordRequestDto.cs
    │   │   │   ├── ResetPasswordRequestDto.cs
    │   │   │   ├── UserProfileUpdateDto.cs
    │   │   │   └── PaymentRequestDto.cs
    │   │   └── Response/
    │   │       ├── SigninResponseDto.cs
    │   │       ├── RefreshTokenResponseDto.cs
    │   │       ├── AutoLoginResponseDto.cs
    │   │       ├── CompleteSectionResultDto.cs
    │   │       ├── CourseCompletionDto.cs
    │   │       ├── CourseProgressDto.cs
    │   │       ├── NextSectionDto.cs
    │   │       ├── NotificationDto.cs
    │   │       └── UserStatsDto.cs
    │   │
    │   ├── 📁 Profile/
    │   │   ├── UserProfileDto.cs
    │   │   └── UserProgressDto.cs
    │   │
    │   ├── 📁 Progress/
    │   │   ├── TrackDto.cs
    │   │   ├── TrackCoursesDto.cs
    │   │   ├── CourseDto.cs
    │   │   ├── CourseInTrackDto.cs
    │   │   ├── LevelDto.cs
    │   │   ├── LevelsResponseDto.cs
    │   │   ├── SectionDto.cs
    │   │   ├── SectionsResponseDto.cs
    │   │   ├── ContentDto.cs
    │   │   └── ContentsResponseDto.cs
    │   │
    │   ├── 📁 Payments/
    │   │   └── MyCourseDto.cs
    │   │
    │   ├── 📁 Admin/
    │   │   ├── AdminUserDto.cs
    │   │   ├── BasicUserInfoDto.cs
    │   │   ├── AdminActionLogDto.cs
    │   │   ├── SystemStatsDto.cs
    │   │   └── AdminSendNotificationInput.cs
    │   │
    │   ├── 📁 Instructor/
    │   │   ├── DashboardDto.cs
    │   │   ├── CourseStatDto.cs
    │   │   └── MostEngagedCourseDto.cs
    │   │
    │   ├── 📁 Track/
    │   │   ├── CreateTrackRequestDto.cs
    │   │   ├── UpdateTrackRequestDto.cs
    │   │   ├── AddCourseToTrackRequestDto.cs
    │   │   ├── TrackDto.cs
    │   │   ├── TrackDetailsDto.cs
    │   │   └── CourseInTrackDto.cs
    │   │
    │   ├── 📁 Courses/
    │   │   ├── CreateCourseDto.cs
    │   │   ├── UpdateCourseDto.cs
    │   │   ├── CourseCDto.cs
    │   │   └── CourseDetailsDto.cs
    │   │
    │   ├── 📁 Levels/
    │   │   ├── CreateLevelDto.cs
    │   │   ├── UpdateLevelDto.cs
    │   │   ├── ReorderLevelDto.cs
    │   │   ├── LevelSummaryDto.cs
    │   │   ├── LevelStatsDto.cs
    │   │   └── VisibilityToggleResultDto.cs
    │   │
    │   ├── 📁 Sections/
    │   │   ├── CreateSectionDto.cs
    │   │   ├── UpdateSectionDto.cs
    │   │   ├── ReorderSectionDto.cs
    │   │   ├── SectionSummaryDto.cs
    │   │   └── SectionStatsDto.cs
    │   │
    │   └── 📁 Contents/
    │       ├── CreateContentDto.cs
    │       ├── UpdateContentDto.cs
    │       ├── ReorderContentDto.cs
    │       ├── ContentSummaryDto.cs
    │
    ├── 🔧 Services/ (طبقة الخدمات)
    │   ├── 📁 Interfaces/
    │   │   ├── IAccountService.cs
    │   │   ├── IUserService.cs
    │   │   ├── IAdminService.cs
    │   │   ├── IDashboardService.cs
    │   │   ├── ITrackService.cs
    │   │   ├── ICourseService.cs
    │   │   ├── ILevelService.cs
    │   │   ├── ISectionService.cs
    │   │   ├── IContentService.cs
    │   │   ├── IActionLogService.cs
    │   │   ├── IEmailQueueService.cs
    │   │   └── IFailedLoginTracker.cs
    │   │
    │   └── 📁 Implementations/
    │       ├── 🔐 Authentication Services
    │       │   ├── AccountService.cs
    │       │   └── FailedLoginTracker.cs
    │       │
    │       ├── 👤 User Services
    │       │   ├── UserService.cs
    │       │   └── AdminService.cs
    │       │
    │       ├── 📚 Course Management Services
    │       │   ├── CourseService.cs
    │       │   ├── LevelService.cs
    │       │   ├── SectionService.cs
    │       │   ├── ContentService.cs
    │       │   └── TrackService.cs
    │       │
    │       ├── 📊 Analytics Services
    │       │   └── DashboardService.cs
    │       │
    │       ├── 📧 Communication Services
    │       │   ├── EmailQueueService.cs
    │       │   └── EmailQueueBackgroundService.cs
    │       │
    │       └── 📝 Logging Services
    │           └── ActionLogService.cs
    │
    ├── 🛠️ Utilities/
    │   ├── AuthHelpers.cs (JWT, Password Hashing)
    │   └── ClaimsPrincipalExtensions.cs
    │
    ├── 🔌 Extensions/
    │   └── ServiceCollectionExtensions.cs (DI Registration)
    │
    ├── 🗺️ Profiles/
    │   └── ApplicationProfiles.cs (AutoMapper Mappings)
    │
    ├── ⚡ Middlewares/
    │   └── ExceptionHandlingMiddleware.cs
    │
    ├── 🌱 Data/
    │   └── DatabaseSeeder.cs (Default Admin & Tracks)
    │
    ├── ⚙️ Configuration Files
    │   ├── appsettings.json
    │   ├── appsettings.Development.json
    │   └── Program.cs
    │
    └── 📦 NuGet Packages
        ├── AutoMapper (14.0.0)
        ├── MailKit (4.12.1)
        ├── Microsoft.AspNetCore.Authentication.JwtBearer (8.0.16)
        ├── Microsoft.EntityFrameworkCore (9.0.5)
        ├── Microsoft.EntityFrameworkCore.SqlServer (9.0.5)
        └── Swashbuckle.AspNetCore (6.6.2)