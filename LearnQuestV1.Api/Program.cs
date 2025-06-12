using LearnQuestV1.Api.Extensions;
using LearnQuestV1.EF.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using LearnQuestV1.Api.Data;
using LearnQuestV1.Api.Middlewares; // Add this using for the DatabaseSeeder

namespace LearnQuestV1.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // add the ApplicationDbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddProjectDependencies();
            builder.Services.AddAutoMapperProfiles();

            builder.Services.AddControllers();
            builder.Services.AddDistributedMemoryCache();

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
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer",
                    Description = "Enter 'Bearer {token}'"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id   = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // ============================================
            // DATABASE SEEDING - USING SYNCHRONOUS APPROACH
            // ============================================
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();

                    // Apply any pending migrations
                    if (context.Database.GetPendingMigrations().Any())
                    {
                        Console.WriteLine("🔄 Applying pending migrations...");
                        context.Database.Migrate();
                        Console.WriteLine("✅ Migrations applied successfully!");
                    }

                    // Seed the database - call the method synchronously
                    DatabaseSeeder.SeedDatabaseAsync(context).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ An error occurred while seeding the database: {ex.Message}");
                    // Optionally log the exception or handle it as needed
                    // Don't throw here to prevent the app from crashing on startup
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseAuthentication(); // Add this line - it was missing
            app.UseAuthorization();

            app.UseCors("AllowReactApp");
            app.MapControllers();

            app.Run();
        }
    }
}