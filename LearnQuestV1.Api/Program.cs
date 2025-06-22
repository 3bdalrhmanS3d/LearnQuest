using LearnQuestV1.Api.Configuration;
using LearnQuestV1.Api.Data;
using LearnQuestV1.Api.Extensions;
using LearnQuestV1.Api.HealthChecks;
using LearnQuestV1.Api.Middlewares;
using LearnQuestV1.Api.Services.Implementations;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.EF.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.DataProtection.Repositories;
using LearnQuestV1.Api.BackgroundServices;
using LearnQuestV1.Api.Hubs;

namespace LearnQuestV1.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // === DATABASE CONFIGURATION ===
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // === SECURITY CONFIGURATION ===
            builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection("Security"));
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

            // === CORE SERVICES ===
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddProjectDependencies();
            builder.Services.AddAutoMapperProfiles();
            builder.Services.AddQuizServices();
            builder.Services.AddEnhancedAuthServices();

            // === NOTIFICATION SERVICES === 
            builder.Services.AddAllEnhancedServices(builder.Configuration);
            builder.Services.AddNotificationServices();
            builder.Services.AddNotificationCors("http://localhost:3000");
            
            // === ENHANCED AUTHENTICATION SERVICES ===
            builder.Services.AddSingleton<IFailedLoginTracker, FailedLoginTracker>();
            builder.Services.AddScoped<ISecurityAuditLogger, SecurityAuditLogger>();
            builder.Services.AddScoped<IAutoLoginService, AutoLoginService>();
            builder.Services.AddSingleton<IEmailQueueService, EmailQueueService>();
            builder.Services.AddSingleton<IEmailTemplateService, EmailTemplateService>();


            // === BACKGROUND SERVICES ===
            builder.Services.AddHostedService<EmailQueueBackgroundService>();
            builder.Services.AddHostedService<FailedLoginMaintenanceService>();

            // === CACHING & SESSION ===
            builder.Services.AddMemoryCache();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

            // === JWT AUTHENTICATION ===
            var jwtSettings = builder.Configuration.GetSection("JWT");
            var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["ValidIss"],
                    ValidAudience = jwtSettings["ValidAud"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            // === CONTROLLERS ===
            builder.Services.AddControllers();

            // ===== CORS & API EXPLORER =====
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                    policy
                      .WithOrigins("http://localhost:3000")  // عنوان الواجهة
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials()
                );
            });

            //builder.Services.AddCors(options =>
            //{
            //    options.AddPolicy("AllowReactApp", policy =>
            //    {
            //        policy.WithOrigins(
            //                "http://localhost:3000",
            //                "http://localhost:5173",
            //                "https://yourfrontend.com")
            //              .AllowAnyMethod()
            //              .AllowAnyHeader()
            //              .AllowCredentials()
            //              .WithExposedHeaders("X-Pagination");
            //    });
            //});

            // === SWAGGER CONFIGURATION ===
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "LearnQuest API",
                    Version = "v1",
                    Description = "Enhanced Learning Platform API with Security Features"
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer",
                    Description = "Enter 'Bearer {your JWT token}'"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // === SECURITY HEADERS ===
            builder.Services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });

            // === HEALTH CHECKS ===
            //builder.Services.AddHealthChecks()
            //    .AddCheck<EmailServiceHealthCheck>("email_service");

            // === BUILD ===
            var app = builder.Build();

            var env = app.Services.GetRequiredService<IHostEnvironment>();
            Console.WriteLine($"Logs folder: {env.ContentRootPath}\\Logs");

            // === DATABASE SEEDING ===
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    if (context.Database.GetPendingMigrations().Any())
                    {
                        Console.WriteLine("🔄 Applying pending migrations...");
                        context.Database.Migrate();
                        Console.WriteLine("✅ Migrations applied successfully!");
                    }
                    DatabaseSeeder.SeedDatabaseAsync(context).GetAwaiter().GetResult();
                    Console.WriteLine("✅ Database seeding completed!");
                }
                catch (Exception ex)
                {
                    services.GetRequiredService<ILogger<Program>>()
                        .LogError(ex, "❌ An error occurred while seeding the database");
                }
            }

            // === MIDDLEWARE PIPELINE ===
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "LearnQuest API v1");
                    c.RoutePrefix = "swagger"; // serve at /swagger
                });
            }

            app.UseHttpsRedirection();

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
                app.UseHsts();
            }

            // 1. Enable routing
            app.UseRouting();

            // 2. CORS / Session / Static / Logging middleware
            app.UseCors("AllowReactApp");
            app.UseRateLimiting();

            app.UseSession();
            app.UseStaticFiles();
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // 3. AuthN & AuthZ
            app.UseAuthentication();
            app.UseAuthorization();

            
            app.MapHealthChecks("/health");
            app.MapControllers();
            app.MapHub<NotificationHub>("/hubs/notifications")
                .RequireCors("NotificationPolicy");

            app.Lifetime.ApplicationStarted.Register(() =>
            {
                Console.WriteLine("🚀 LearnQuest API is running!");
                Console.WriteLine($"📍 Environment: {app.Environment.EnvironmentName}");
                Console.WriteLine($"📱 Swagger UI: {(app.Environment.IsDevelopment() ? "Available at /swagger" : "Disabled in production")} ");
                Console.WriteLine("🏥 Health Check: Available at /health");
            });
            app.UseCors("NotificationPolicy");
            app.Run();
        }
    }
}
