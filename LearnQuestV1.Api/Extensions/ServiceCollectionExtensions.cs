using LearnQuestV1.Api.BackgroundServices;
using LearnQuestV1.Api.Configuration;
using LearnQuestV1.Api.HealthChecks;
using LearnQuestV1.Api.Services.Implementations;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.EF.Repositories;
using LearnQuestV1.EF.UnitOfWork;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

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
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IPointsService, PointsService>();
            services.AddScoped<INotificationService, NotificationService>();

            // === ENHANCED SECURITY SERVICES ===
            // Note: These are now registered in Program.cs for better control
            // but keeping here for backward compatibility if needed

            return services;
        }

        /// <summary>
        /// Add background services for notifications
        /// </summary>
        public static IServiceCollection AddNotificationBackgroundServices(this IServiceCollection services)
        {
            services.AddHostedService<NotificationCleanupService>();
            services.AddHostedService<NotificationReminderService>();

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
        public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure email settings
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            // Email Template Service
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();

            // Email Queue Service (Singleton for queue management)
            services.AddSingleton<IEmailQueueService, EmailQueueService>();

            // Background Services
            services.AddHostedService<EmailQueueBackgroundService>();

            return services;
        }
        // === HEALTH CHECKS ===
        public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck<EmailServiceHealthCheck>("email_service");

            return services;
        }

        // === RATE LIMITING SERVICES ===
        public static IServiceCollection AddRateLimitingServices(this IServiceCollection services)
        {
            services.AddMemoryCache();
            return services;
        }

        // === ALL ENHANCED SERVICES ===
        public static IServiceCollection AddAllEnhancedServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddProjectDependencies();
            services.AddQuizServices();
            services.AddEnhancedAuthServices();
            services.AddEmailServices(configuration);
            services.AddCustomHealthChecks();
            services.AddRateLimitingServices();
            services.AddAutoMapperProfiles();

            return services;
        }

        // ================================
        // Background Services for Notifications
        // ================================

        /// <summary>
        /// Background service to clean up old notifications
        /// </summary>
        public class NotificationCleanupService : BackgroundService
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly ILogger<NotificationCleanupService> _logger;
            private readonly TimeSpan _cleanupInterval = TimeSpan.FromDays(1); // Run daily

            public NotificationCleanupService(
                IServiceProvider serviceProvider,
                ILogger<NotificationCleanupService> logger)
            {
                _serviceProvider = serviceProvider;
                _logger = logger;
            }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                        var deletedCount = await notificationService.CleanupOldNotificationsAsync(30);
                        _logger.LogInformation("Cleaned up {DeletedCount} old notifications", deletedCount);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during notification cleanup");
                    }

                    await Task.Delay(_cleanupInterval, stoppingToken);
                }
            }
        }

        /// <summary>
        /// Background service to send reminder notifications
        /// </summary>
        public class NotificationReminderService : BackgroundService
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly ILogger<NotificationReminderService> _logger;
            private readonly TimeSpan _reminderInterval = TimeSpan.FromHours(6); // Run every 6 hours

            public NotificationReminderService(
                IServiceProvider serviceProvider,
                ILogger<NotificationReminderService> logger)
            {
                _serviceProvider = serviceProvider;
                _logger = logger;
            }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                        // Logic to send reminder notifications for inactive users
                        // This would be implemented based on business requirements

                        _logger.LogInformation("Reminder notifications processed");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during reminder notification processing");
                    }

                    await Task.Delay(_reminderInterval, stoppingToken);
                }
            }
        }

        /// <summary>
        /// Global exception handling middleware
        /// </summary>
        public class GlobalExceptionMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly ILogger<GlobalExceptionMiddleware> _logger;

            public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
            {
                _next = next;
                _logger = logger;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                try
                {
                    await _next(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unhandled exception occurred");
                    await HandleExceptionAsync(context, ex);
                }
            }

            private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
            {
                context.Response.ContentType = "application/json";

                var response = new
                {
                    Success = false,
                    Message = "An error occurred while processing your request",
                    Details = exception.Message,
                    Timestamp = DateTime.UtcNow
                };

                context.Response.StatusCode = exception switch
                {
                    KeyNotFoundException => 404,
                    UnauthorizedAccessException => 403,
                    ArgumentException => 400,
                    _ => 500
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}