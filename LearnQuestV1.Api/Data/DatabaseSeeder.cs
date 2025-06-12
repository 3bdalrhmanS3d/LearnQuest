using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Models;
using LearnQuestV1.EF.Application;
using Microsoft.EntityFrameworkCore;

namespace LearnQuestV1.Api.Data
{
    public static class DatabaseSeeder
    {
        /// <summary>
        /// Seeds the database with default admin user if no admin exists
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>Task representing the async operation</returns>
        public static async Task SeedDefaultAdminAsync(ApplicationDbContext context)
        {
            try
            {
                // Check if any admin user already exists
                var adminExists = await context.Users
                    .AnyAsync(u => u.Role == UserRole.Admin && !u.IsDeleted);

                if (!adminExists)
                {
                    // Create default admin user
                    var defaultAdmin = new User
                    {
                        FullName = "System Administrator",
                        EmailAddress = "admin@learnquest.com",
                        PasswordHash = AuthHelpers.HashPassword("Admin@123"), // Strong default password
                        CreatedAt = DateTime.UtcNow,
                        Role = UserRole.Admin,
                        IsActive = true,
                        IsDeleted = false,
                        IsSystemProtected = true, // Prevent deletion of this admin
                        ProfilePhoto = "/uploads/profile-pictures/default_admin.webp"
                    };

                    await context.Users.AddAsync(defaultAdmin);
                    await context.SaveChangesAsync();

                    // Create a verified account verification record for the admin
                    var adminVerification = new AccountVerification
                    {
                        UserId = defaultAdmin.UserId,
                        Code = "000000", // Dummy code since admin is auto-verified
                        CheckedOK = true, // Pre-verified
                        Date = DateTime.UtcNow
                    };

                    await context.AccountVerifications.AddAsync(adminVerification);

                    // Create default user details for admin
                    var adminDetails = new UserDetail
                    {
                        UserId = defaultAdmin.UserId,
                        BirthDate = new DateTime(1990, 1, 1), // Default birth date
                        EducationLevel = "Master's Degree",
                        Nationality = "Egypt",
                        CreatedAt = DateTime.UtcNow
                    };

                    await context.UserDetails.AddAsync(adminDetails);
                    await context.SaveChangesAsync();

                    Console.WriteLine("✅ Default admin user created successfully!");
                    Console.WriteLine($"📧 Email: {defaultAdmin.EmailAddress}");
                    Console.WriteLine($"🔑 Password: Admin@123");
                    Console.WriteLine("⚠️  Please change the default password after first login!");
                    Console.WriteLine("🛡️  This admin account is system-protected and cannot be deleted.");
                }
                else
                {
                    Console.WriteLine("ℹ️  Admin user already exists. Skipping seeding.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error seeding default admin: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Seeds default course tracks if none exist
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>Task representing the async operation</returns>
        public static async Task SeedDefaultTracksAsync(ApplicationDbContext context)
        {
            try
            {
                var tracksExist = await context.CourseTracks.AnyAsync();

                if (!tracksExist)
                {
                    var defaultTracks = new List<CourseTrack>
                    {
                        new CourseTrack
                        {
                            TrackName = "Web Development",
                            TrackDescription = "Learn modern web development with HTML, CSS, JavaScript, and popular frameworks",
                            CreatedAt = DateTime.UtcNow,
                            TrackImage = "/uploads/TrackImages/web-development.jpg"
                        },
                        new CourseTrack
                        {
                            TrackName = "Mobile App Development",
                            TrackDescription = "Build mobile applications for iOS and Android platforms",
                            CreatedAt = DateTime.UtcNow,
                            TrackImage = "/uploads/TrackImages/mobile-development.jpg"
                        },
                        new CourseTrack
                        {
                            TrackName = "Data Science",
                            TrackDescription = "Master data analysis, machine learning, and artificial intelligence",
                            CreatedAt = DateTime.UtcNow,
                            TrackImage = "/uploads/TrackImages/data-science.jpg"
                        },
                        new CourseTrack
                        {
                            TrackName = "DevOps & Cloud",
                            TrackDescription = "Learn deployment, containerization, and cloud technologies",
                            CreatedAt = DateTime.UtcNow,
                            TrackImage = "/uploads/TrackImages/devops.jpg"
                        }
                    };

                    await context.CourseTracks.AddRangeAsync(defaultTracks);
                    await context.SaveChangesAsync();

                    Console.WriteLine($"✅ Created {defaultTracks.Count} default course tracks!");
                }
                else
                {
                    Console.WriteLine("ℹ️  Course tracks already exist. Skipping track seeding.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error seeding default tracks: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Seeds additional default data if needed
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>Task representing the async operation</returns>
        public static async Task SeedAdditionalDataAsync(ApplicationDbContext context)
        {
            try
            {
                // Seed default course tracks
                await SeedDefaultTracksAsync(context);

                // You can add more seeding logic here for:
                // - Sample courses
                // - Default notification templates
                // - System settings
                // etc.

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error seeding additional data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Master seeding method that calls all individual seeders
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>Task representing the async operation</returns>
        public static async Task SeedDatabaseAsync(ApplicationDbContext context)
        {
            Console.WriteLine("🌱 Starting database seeding...");

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Run all seeders
            await SeedDefaultAdminAsync(context);
            await SeedAdditionalDataAsync(context);

            Console.WriteLine("✅ Database seeding completed!");
        }
    }
}