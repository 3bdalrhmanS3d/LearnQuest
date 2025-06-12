using LearnQuestV1.Api.Services.Implementations;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.EF.Repositories;
using LearnQuestV1.EF.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace LearnQuestV1.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProjectDependencies(this IServiceCollection services)
        {
            // سجلّ المستودعات و UnitOfWork
            services.AddScoped(typeof(IBaseRepo<>), typeof(BaseRepo<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // سجلّ AccountService
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<ITrackService, TrackService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<ISectionService, SectionService>();
            services.AddScoped<ILevelService, LevelService>();

            services.AddScoped<ActionLogService, ActionLogService > ();
            services.AddScoped<TrackService>();
            // سجلّ FailedLoginTracker كـScoped
            services.AddScoped<IFailedLoginTracker, FailedLoginTracker>();
            services.AddHttpContextAccessor();
            // هنا: سجّل EmailQueueService كـSingleton بدلاً من Scoped
            services.AddSingleton<IEmailQueueService, EmailQueueService>();


            // سجّل الخلفيّة، وهي HostedService (Singleton ضمن DI)
            services.AddHostedService<EmailQueueBackgroundService>();

            return services;
        }

        public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);
            return services;
        }
    }
}
