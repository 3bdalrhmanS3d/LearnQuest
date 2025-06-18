using LearnQuestV1.Api.Services.Implementations;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.EF.Repositories;
using LearnQuestV1.EF.UnitOfWork;

namespace LearnQuestV1.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProjectDependencies(this IServiceCollection services)
        {
            // === CORE REPOSITORIES & UNIT OF WORK ===
            services.AddScoped(typeof(IBaseRepo<>), typeof(BaseRepo<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // === BUSINESS SERVICES ===
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<ITrackService, TrackService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<ISectionService, SectionService>();
            services.AddScoped<ILevelService, LevelService>();
            services.AddScoped<IActionLogService, ActionLogService>();
            services.AddScoped<IContentService, ContentService>();
            services.AddScoped<IContentCachingService, ContentCachingService>();
            services.AddScoped<Services.Implementations.IContentAnalyticsService, ContentAnalyticsService>();
            services.AddHttpClient<IContentValidationService, ContentValidationService>();
            services.AddScoped<IReviewService, ReviewService>();
            // === ENHANCED SECURITY SERVICES ===
            // Note: These are now registered in Program.cs for better control
            // but keeping here for backward compatibility if needed

            return services;
        }

        public static IServiceCollection AddQuizServices(this IServiceCollection services)
        {
            // === QUIZ REPOSITORIES ===
            services.AddScoped<IQuizRepository, QuizRepository>();
            services.AddScoped<IQuestionRepository, QuestionRepository>();
            services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();

            // === QUIZ SERVICES ===
            services.AddScoped<IQuizService, QuizService>();

            return services;
        }

        public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
        {
            // Register AutoMapper with all profiles in the current assembly
            services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);
            return services;
        }

        // === ENHANCED AUTHENTICATION SERVICES ===
        public static IServiceCollection AddEnhancedAuthServices(this IServiceCollection services)
        {
            // Security Services
            services.AddScoped<IAutoLoginService, AutoLoginService>();
            services.AddScoped<ISecurityAuditLogger, SecurityAuditLogger>();
            services.AddScoped<IAdminActionLogger, AdminActionLogger>();

            // Failed Login Tracker (Singleton for cross-request tracking)
            services.AddSingleton<IFailedLoginTracker, FailedLoginTracker>();

            return services;
        }

        // === EMAIL SERVICES ===
        public static IServiceCollection AddEmailServices(this IServiceCollection services)
        {
            // Email Template Service
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();

            // Email Queue Service (Singleton for queue management)
            services.AddSingleton<IEmailQueueService, EmailQueueService>();

            // Background Services
            services.AddHostedService<EmailQueueBackgroundService>();
            services.AddHostedService<FailedLoginMaintenanceService>();

            return services;
        }

        // === RATE LIMITING SERVICES ===
        public static IServiceCollection AddRateLimitingServices(this IServiceCollection services)
        {
            services.AddMemoryCache();
            return services;
        }

        // === ALL ENHANCED SERVICES ===
        public static IServiceCollection AddAllEnhancedServices(this IServiceCollection services)
        {
            services.AddProjectDependencies();
            services.AddQuizServices();
            services.AddEnhancedAuthServices();
            services.AddEmailServices();
            services.AddRateLimitingServices();
            services.AddAutoMapperProfiles();

            return services;
        }
    }
}