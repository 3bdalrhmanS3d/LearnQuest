using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Models.CourseOrganization;
using LearnQuestV1.Core.Models.CourseStructure;
using LearnQuestV1.Core.Models.UserManagement;
using LearnQuestV1.EF.Application;
using Microsoft.EntityFrameworkCore;

namespace LearnQuestV1.Api.Data
{
    public static class DatabaseSeeder
    {
        const string userImage = "/uploads/profile-pictures/default.png";
        const string courseImageDefault = "/uploads/courses/default-course.jpg";
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
                        PasswordHash = AuthHelpers.HashPassword("Yg1rb76y@Yg1rb76y"), // Strong default password
                        CreatedAt = DateTime.UtcNow,
                        Role = UserRole.Admin,
                        IsActive = true,
                        IsDeleted = false,
                        IsSystemProtected = true, // Prevent deletion of this admin
                        ProfilePhoto = "/uploads/profile-pictures/defult.png"
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
                    Console.WriteLine($"🔑 Password: Yg1rb76y@Yg1rb76y");
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
        /// Seeds sample instructors if none exist
        /// </summary>
        public static async Task SeedSampleInstructorsAsync(ApplicationDbContext context)
        {
            try
            {
                var instructorsExist = await context.Users
                    .AnyAsync(u => u.Role == UserRole.Instructor && !u.IsDeleted);

                if (!instructorsExist)
                {
                    var instructors = new List<User>
                    {
                        new User
                        {
                            FullName = "Ahmed Hassan",
                            EmailAddress = "ahmed.hassan@learnquest.com",
                            PasswordHash = AuthHelpers.HashPassword("Yg1rb76y@Yg1rb76y"),
                            CreatedAt = DateTime.UtcNow,
                            Role = UserRole.Instructor,
                            IsActive = true,
                            IsDeleted = false,
                            ProfilePhoto = userImage
                        },
                        new User
                        {
                            FullName = "Sara Mohamed",
                            EmailAddress = "sara.mohamed@learnquest.com",
                            PasswordHash = AuthHelpers.HashPassword("Yg1rb76y@Yg1rb76y"),
                            CreatedAt = DateTime.UtcNow,
                            Role = UserRole.Instructor,
                            IsActive = true,
                            IsDeleted = false,
                            ProfilePhoto =userImage 
                        },
                        new User
                        {
                            FullName = "Omar Ali",
                            EmailAddress = "omar.ali@learnquest.com",
                            PasswordHash = AuthHelpers.HashPassword("Yg1rb76y@Yg1rb76y"),
                            CreatedAt = DateTime.UtcNow,
                            Role = UserRole.Instructor,
                            IsActive = true,
                            IsDeleted = false,
                            ProfilePhoto = userImage
                        }
                    };

                    await context.Users.AddRangeAsync(instructors);
                    await context.SaveChangesAsync();

                    var verifications = instructors.Select(inst => new AccountVerification
                    {
                        UserId = inst.UserId,
                        Code = "000000",  
                        CheckedOK = true,         
                        Date = DateTime.UtcNow
                    }).ToList();

                    await context.AccountVerifications.AddRangeAsync(verifications);

                    // 3. لكل Instructor: إنشاء تفاصيل المستخدم (UserDetail)
                    var details = instructors.Select(inst => new UserDetail
                    {
                        UserId = inst.UserId,
                        BirthDate = DateTime.UtcNow.AddYears(-30), 
                        EducationLevel = "Not specified",
                        Nationality = "Unknown",
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    await context.UserDetails.AddRangeAsync(details);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✅ Created {instructors.Count} sample instructors!");
                }
                else
                {
                    Console.WriteLine("ℹ️  Instructors already exist. Skipping instructor seeding.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error seeding sample instructors: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Seeds up to 50 sample regular users, adding فقط ما ينقص للوصول إلى 50.
        /// Creates details only للمستخدمين المفعلين.
        /// </summary>
        public static async Task SeedSampleUsersAsync(ApplicationDbContext context, int targetCount = 50)
        {
            try
            {
                // 1. احسب عدد المستخدمين الحاليين
                var existingCount = await context.Users
                    .CountAsync(u => u.Role == UserRole.RegularUser && !u.IsDeleted);

                var toCreate = targetCount - existingCount;
                if (toCreate < 1)
                {
                    Console.WriteLine($"ℹ️  Already have {existingCount} regular users. No need to seed more.");
                    return;
                }

                Console.WriteLine($"ℹ️  Found {existingCount} regular users. Will create {toCreate} more to reach {targetCount}.");

                var rnd = new Random();
                var firstNames = new[] { "Ali", "Sara", "Omar", "Laila", "Khaled", "Mona", "Youssef", "Nour" };
                var lastNames = new[] { "Hassan", "Ahmed", "Saleh", "Mostafa", "Ibrahim", "Fahmy" };
                var educations = new[] { "High School", "Bachelor's Degree", "Master's Degree", "PhD" };
                var nationalities = new[] { "Egypt", "Jordan", "Lebanon", "Morocco", "Tunisia", "UAE" };

                var newUsers = new List<User>(capacity: toCreate);
                for (int i = 1; i <= toCreate; i++)
                {
                    var fullName = $"{firstNames[rnd.Next(firstNames.Length)]} {lastNames[rnd.Next(lastNames.Length)]}";
                    var email = $"{fullName.ToLower().Replace(" ", ".")}{existingCount + i}@learnquest.com";

                    var user = new User
                    {
                        FullName = fullName,
                        EmailAddress = email,
                        PasswordHash = AuthHelpers.HashPassword("Yg1rb76y@Yg1rb76y"),
                        CreatedAt = DateTime.UtcNow,
                        Role = UserRole.RegularUser,
                        IsActive = rnd.Next(2) == 0,
                        IsDeleted = false,
                        ProfilePhoto = userImage
                    };
                    newUsers.Add(user);
                }

                await context.Users.AddRangeAsync(newUsers);
                await context.SaveChangesAsync();

                var details = new List<UserDetail>();
                var verifications = new List<AccountVerification>();

                foreach (var u in newUsers)
                {
                    if (u.IsActive)
                    {
                        var start = new DateTime(1970, 1, 1);
                        var end = new DateTime(2005, 1, 1);
                        var span = end - start;
                        var birth = start + TimeSpan.FromTicks((long)(rnd.NextDouble() * span.Ticks));

                        details.Add(new UserDetail
                        {
                            UserId = u.UserId,
                            BirthDate = birth,
                            EducationLevel = educations[rnd.Next(educations.Length)],
                            Nationality = nationalities[rnd.Next(nationalities.Length)],
                            CreatedAt = DateTime.UtcNow
                        });

                        var code = ReferenceEquals(u, newUsers.First())
                            ? "000000"
                            : AuthHelpers.GenerateVerificationCode();

                        verifications.Add(new AccountVerification
                        {
                            UserId = u.UserId,
                            Code = code,
                            CheckedOK = true,
                            Date = DateTime.UtcNow
                        });
                    }
                }

                if (details.Any())
                    await context.UserDetails.AddRangeAsync(details);
                if (verifications.Any())
                    await context.AccountVerifications.AddRangeAsync(verifications);

                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Created {toCreate} regular users, with details for the active ones.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error seeding sample users: {ex.Message}");
                throw;
            }
        }


        /// <summary>
        /// Seeds sample courses with levels, sections, and content
        /// </summary>
        public static async Task SeedSampleCoursesAsync(ApplicationDbContext context)
        {
            try
            {
                var coursesExist = await context.Courses.AnyAsync();

                if (!coursesExist)
                {
                    // Get tracks and instructors
                    var tracks = await context.CourseTracks.ToListAsync();
                    var instructors = await context.Users.Where(u => u.Role == UserRole.Instructor).ToListAsync();

                    if (!tracks.Any() || !instructors.Any())
                    {
                        Console.WriteLine("❌ Tracks or Instructors not found. Please seed them first.");
                        return;
                    }

                    var courses = new List<Course>();
                    var courseTrackCourses = new List<CourseTrackCourse>();

                    // Web Development Track Courses
                    var webDevTrack = tracks.First(t => t.TrackName == "Web Development");
                    var webDevCourses = new[]
                    {
                        new Course
                        {
                            CourseName = "HTML & CSS Fundamentals",
                            Description = "Master the building blocks of web development with HTML5 and CSS3",
                            CourseImage = courseImageDefault,
                            CoursePrice = 199.99m,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            InstructorId = instructors[0].UserId
                        },
                        new Course
                        {
                            CourseName = "JavaScript Essentials",
                            Description = "Learn modern JavaScript programming from basics to advanced concepts",
                            CourseImage = courseImageDefault,
                            CoursePrice = 299.99m,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            InstructorId = instructors[0].UserId
                        },
                        new Course
                        {
                            CourseName = "React.js Complete Guide",
                            Description = "Build modern web applications with React.js and hooks",
                            CourseImage = courseImageDefault,
                            CoursePrice = 399.99m,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            InstructorId = instructors[1].UserId
                        },
                        new Course
                        {
                            CourseName = "Node.js Backend Development",
                            Description = "Create scalable backend applications with Node.js and Express",
                            CourseImage = courseImageDefault,
                            CoursePrice = 349.99m,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            InstructorId = instructors[1].UserId
                        },
                        new Course
                        {
                            CourseName = "Full-Stack Web Projects",
                            Description = "Build complete web applications from frontend to backend",
                            CourseImage = courseImageDefault,
                            CoursePrice = 499.99m,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            InstructorId = instructors[2].UserId
                        },
                        new Course
                        {
                            CourseName = "Web Development DevOps",
                            Description = "Deploy and maintain web applications with modern DevOps practices",
                            CourseImage = courseImageDefault,
                            CoursePrice = 299.99m,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            InstructorId = instructors[2].UserId
                        }
                    };

                    courses.AddRange(webDevCourses);

                    // Mobile Development Track Courses
                    var mobileTrack = tracks.First(t => t.TrackName == "Mobile App Development");
                    var mobileCourses = new[]
                    {
                        new Course
                        {
                            CourseName = "Android Development with Kotlin",
                            Description = "Build native Android apps using Kotlin programming language",
                            CourseImage = courseImageDefault,
                            CoursePrice = 349.99m,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            InstructorId = instructors[0].UserId
                        },
                        new Course
                        {
                            CourseName = "iOS Development with Swift",
                            Description = "Create beautiful iOS applications using Swift and SwiftUI",
                            CourseImage = courseImageDefault,
                            CoursePrice = 399.99m,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            InstructorId = instructors[1].UserId
                        },
                        new Course
                        {
                            CourseName = "React Native Cross-Platform",
                            Description = "Develop mobile apps for both iOS and Android with React Native",
                            CourseImage = courseImageDefault,
                            CoursePrice = 449.99m,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            InstructorId = instructors[2].UserId
                        },
                        new Course
                        {
                            CourseName = "Flutter Mobile Development",
                            Description = "Build cross-platform mobile apps with Flutter and Dart",
                            CourseImage = courseImageDefault,
                            CoursePrice = 399.99m,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            InstructorId = instructors[0].UserId
                        },
                        new Course
                        {
                            CourseName = "Mobile App UI/UX Design",
                            Description = "Design beautiful and user-friendly mobile interfaces",
                            CourseImage = courseImageDefault,
                            CoursePrice = 249.99m,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            InstructorId = instructors[1].UserId
                        },
                        new Course
                        {
                            CourseName = "Mobile App Publishing",
                            Description = "Deploy apps to Google Play Store and Apple App Store",
                            CourseImage = courseImageDefault,
                            CoursePrice = 199.99m,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            InstructorId = instructors[2].UserId
                        }
                    };

                    courses.AddRange(mobileCourses);

                    // Data Science Track Courses
                    var dataTrack = tracks.First(t => t.TrackName == "Data Science");
                    var dataCourses = new[]
                    {
                        new Course
                        {
                            CourseName = "Python for Data Science",
                            Description = "Learn Python programming specifically for data analysis and science",
                            CourseImage = courseImageDefault,
                            CoursePrice = 299.99m,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            InstructorId = instructors[0].UserId
                        },
                        new Course
                        {
                            CourseName = "Data Analysis with Pandas",
                            Description = "Master data manipulation and analysis using Pandas library",
                            CourseImage = courseImageDefault,
                            CoursePrice = 249.99m,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            InstructorId = instructors[1].UserId
                        },
                        new Course
                        {
                            CourseName = "Machine Learning Fundamentals",
                            Description = "Introduction to machine learning algorithms and applications",
                            CourseImage = courseImageDefault,
                            CoursePrice = 399.99m,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            InstructorId = instructors[2].UserId
                        },
                        new Course
                        {
                            CourseName = "Deep Learning with TensorFlow",
                            Description = "Build neural networks and deep learning models with TensorFlow",
                            CourseImage = "/uploads/course-images/tensorflow.jpg",
                            CoursePrice = 499.99m,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            InstructorId = instructors[0].UserId
                        },
                        new Course
                        {
                            CourseName = "Data Visualization",
                            Description = "Create compelling data visualizations and dashboards",
                            CourseImage = courseImageDefault,
                            CoursePrice = 199.99m,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            InstructorId = instructors[1].UserId
                        },
                        new Course
                        {
                            CourseName = "Big Data Analytics",
                            Description = "Process and analyze large datasets with modern tools",
                            CourseImage = courseImageDefault,
                            CoursePrice = 449.99m,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            InstructorId = instructors[2].UserId
                        }
                    };

                    courses.AddRange(dataCourses);

                    var devOpsTrack = tracks.First(t => t.TrackName == "DevOps & Cloud");
                    var devOpsCourses = new[]
                                        {
                        new Course
                        {
                            CourseName = "Introduction to Docker & Kubernetes",
                            Description = "Learn containerization and orchestration basics",
                            CourseImage = courseImageDefault,
                            CoursePrice = 299.99m,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            InstructorId = instructors[0].UserId
                        },
                        
                    };
                    courses.AddRange(devOpsCourses);

                    // Add all courses
                    await context.Courses.AddRangeAsync(courses);
                    await context.SaveChangesAsync();
                    
                    foreach (var course in webDevCourses)
                    {
                        courseTrackCourses.Add(new CourseTrackCourse
                        {
                            TrackId = webDevTrack.TrackId,
                            CourseId = course.CourseId
                        });
                    }

                    foreach (var course in mobileCourses)
                    {
                        courseTrackCourses.Add(new CourseTrackCourse
                        {
                            TrackId = mobileTrack.TrackId,
                            CourseId = course.CourseId
                        });
                    }

                    foreach (var course in dataCourses)
                    {
                        courseTrackCourses.Add(new CourseTrackCourse
                        {
                            TrackId = dataTrack.TrackId,
                            CourseId = course.CourseId
                        });
                    }

                    await context.CourseTrackCourses.AddRangeAsync(courseTrackCourses);
                    await context.SaveChangesAsync();

                    Console.WriteLine($"✅ Created {webDevCourses.Length + mobileCourses.Length + dataCourses.Length} sample courses with track associations!");


                    await context.CourseTrackCourses.AddRangeAsync(courseTrackCourses);
                    await context.SaveChangesAsync();

                    Console.WriteLine($"✅ Created {courses.Count} sample courses with track associations!");

                    // Now seed levels, sections, and content for each course
                    await SeedCourseLevelsAsync(context, courses);
                }
                else
                {
                    Console.WriteLine("ℹ️  Courses already exist. Skipping course seeding.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error seeding sample courses: {ex.Message}");
                throw;
            }
        }


        /// <summary>
        /// Seeds levels, sections, and content for courses
        /// </summary>
        private static async Task SeedCourseLevelsAsync(ApplicationDbContext context, List<Course> courses)
        {
            try
            {
                var levels = new List<Level>();
                var sections = new List<Section>();
                var contents = new List<Content>();

                foreach (var course in courses)
                {
                    // Create 3-5 levels per course
                    var levelCount = Random.Shared.Next(3, 6);

                    for (int levelOrder = 1; levelOrder <= levelCount; levelOrder++)
                    {
                        var level = new Level
                        {
                            CourseId = course.CourseId,
                            LevelName = $"Level {levelOrder}: {GetLevelName(course.CourseName, levelOrder)}",
                            LevelDetails = $"In this level, you'll learn {GetLevelDescription(course.CourseName, levelOrder)}",
                            LevelOrder = levelOrder,
                            IsVisible = true,
                            IsDeleted = false,
                            RequiresPreviousLevelCompletion = levelOrder > 1
                        };

                        levels.Add(level);

                        // Create 2-4 sections per level
                        var sectionCount = Random.Shared.Next(2, 5);

                        for (int sectionOrder = 1; sectionOrder <= sectionCount; sectionOrder++)
                        {
                            var section = new Section
                            {
                                Level = level, // Navigation property
                                SectionName = $"Section {sectionOrder}: {GetSectionName(course.CourseName, levelOrder, sectionOrder)}",
                                SectionOrder = sectionOrder,
                                IsVisible = true,
                                IsDeleted = false,
                                RequiresPreviousSectionCompletion = sectionOrder > 1
                            };

                            sections.Add(section);

                            // Create 3-6 content items per section
                            var contentCount = Random.Shared.Next(3, 7);

                            for (int contentOrder = 1; contentOrder <= contentCount; contentOrder++)
                            {
                                var contentType = GetRandomContentType();
                                var content = new Content
                                {
                                    Section = section, // Navigation property
                                    Title = $"{GetContentTitle(course.CourseName, levelOrder, sectionOrder, contentOrder)}",
                                    ContentDescription = $"Content description for {course.CourseName} - Level {levelOrder}, Section {sectionOrder}",
                                    ContentType = contentType,
                                    ContentOrder = contentOrder,
                                    DurationInMinutes = Random.Shared.Next(5, 45),
                                    IsDeleted = false,
                                    CreatedAt = DateTime.UtcNow
                                };

                                // Set content-specific properties based on type
                                switch (contentType)
                                {
                                    case ContentType.Video:
                                        content.ContentUrl = $"/uploads/videos/{course.CourseName.Replace(" ", "_").ToLower()}_l{levelOrder}_s{sectionOrder}_c{contentOrder}.mp4";
                                        break;
                                    case ContentType.Doc:
                                        content.ContentDoc = $"/uploads/documents/{course.CourseName.Replace(" ", "_").ToLower()}_l{levelOrder}_s{sectionOrder}_c{contentOrder}.pdf";
                                        break;
                                    case ContentType.Text:
                                        content.ContentText = GetSampleTextContent(course.CourseName, levelOrder, sectionOrder, contentOrder);
                                        break;
                                }

                                contents.Add(content);
                            }
                        }
                    }
                }

                // Add all entities to context
                await context.Levels.AddRangeAsync(levels);
                await context.SaveChangesAsync();

                // The sections and contents should be automatically added due to navigation properties
                // But let's explicitly add them to ensure consistency
                await context.Sections.AddRangeAsync(sections);
                await context.SaveChangesAsync();

                await context.Contents.AddRangeAsync(contents);
                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Created {levels.Count} levels, {sections.Count} sections, and {contents.Count} content items!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error seeding course levels: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Seeds sample course skills and about course information
        /// </summary>
        public static async Task SeedCourseDetailsAsync(ApplicationDbContext context)
        {
            try
            {
                var courses = await context.Courses.ToListAsync();
                if (!courses.Any()) return;

                var aboutCourses = new List<AboutCourse>();
                var courseSkills = new List<CourseSkill>();

                foreach (var course in courses)
                {
                    // Add about course items
                    var aboutItems = GetAboutCourseItems(course.CourseName);
                    foreach (var item in aboutItems)
                    {
                        aboutCourses.Add(new AboutCourse
                        {
                            CourseId = course.CourseId,
                            AboutCourseText = item
                        });
                    }

                    // Add course skills
                    var skills = GetCourseSkills(course.CourseName);
                    foreach (var skill in skills)
                    {
                        courseSkills.Add(new CourseSkill
                        {
                            CourseId = course.CourseId,
                            CourseSkillText = skill
                        });
                    }
                }

                await context.AboutCourses.AddRangeAsync(aboutCourses);
                await context.CourseSkills.AddRangeAsync(courseSkills);
                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Created {aboutCourses.Count} about course items and {courseSkills.Count} course skills!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error seeding course details: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Master seeding method that calls all individual seeders
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>Task representing the async operation</returns>
        /// 
        public static async Task SeedDatabaseAsync(ApplicationDbContext context)
        {
            Console.WriteLine("🌱 Starting comprehensive database seeding...");

            try
            {
                // Ensure database is created
                await context.Database.EnsureCreatedAsync();

                // Run all seeders in order
                await SeedDefaultAdminAsync(context);
                await SeedSampleUsersAsync(context, 50);
                await SeedDefaultTracksAsync(context);
                await SeedSampleInstructorsAsync(context);
                await SeedSampleCoursesAsync(context);

                if (!await context.Levels.AnyAsync())
                {
                    var allCourses = await context.Courses.ToListAsync();
                    Console.WriteLine("ℹ️  No levels found. Seeding levels, sections, and contents for existing courses...");
                    await SeedCourseLevelsAsync(context, allCourses);
                }

                await SeedCourseDetailsAsync(context);

                Console.WriteLine("✅ Database seeding completed successfully!");
                Console.WriteLine($"📊 Final Statistics:");

                var stats = await GetSeedingStatsAsync(context);
                Console.WriteLine($"   👥 Users: {stats.TotalUsers} (Admins: {stats.AdminCount}, Instructors: {stats.InstructorCount} )");
                Console.WriteLine($"   🎯 Tracks: {stats.TrackCount}");
                Console.WriteLine($"   📚 Courses: {stats.CourseCount}");
                Console.WriteLine($"   📈 Levels: {stats.LevelCount}");
                Console.WriteLine($"   📝 Sections: {stats.SectionCount}");
                Console.WriteLine($"   📄 Content Items: {stats.ContentCount}");
                Console.WriteLine($"   🔧 Skills: {stats.SkillCount}");
                Console.WriteLine($"   ℹ️  About Items: {stats.AboutItemCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Critical error during database seeding: {ex.Message}");
                throw;
            }
        }

        // Helper methods for generating content

        private static string GetLevelName(string courseName, int levelOrder)
        {
            var levelNames = courseName switch
            {
                "HTML & CSS Fundamentals" => new[] { "HTML Basics", "CSS Styling", "Responsive Design", "Advanced CSS", "CSS Frameworks" },
                "JavaScript Essentials" => new[] { "JS Basics", "DOM Manipulation", "ES6+ Features", "Async Programming", "Error Handling" },
                "React.js Complete Guide" => new[] { "React Basics", "Components & Props", "State & Lifecycle", "Hooks", "Advanced Patterns" },
                "Node.js Backend Development" => new[] { "Node Basics", "Express Framework", "Database Integration", "Authentication", "API Development" },
                "Python for Data Science" => new[] { "Python Basics", "NumPy & Arrays", "Data Structures", "File Operations", "Libraries" },
                "Machine Learning Fundamentals" => new[] { "ML Introduction", "Supervised Learning", "Unsupervised Learning", "Model Evaluation", "Deployment" },
                _ => new[] { "Introduction", "Fundamentals", "Intermediate", "Advanced", "Mastery" }
            };

            return levelOrder <= levelNames.Length ? levelNames[levelOrder - 1] : $"Advanced Topic {levelOrder}";
        }

        private static string GetLevelDescription(string courseName, int levelOrder)
        {
            return courseName switch
            {
                "HTML & CSS Fundamentals" when levelOrder == 1 => "the basic structure of HTML documents and semantic markup",
                "HTML & CSS Fundamentals" when levelOrder == 2 => "how to style HTML elements with CSS properties and selectors",
                "JavaScript Essentials" when levelOrder == 1 => "JavaScript syntax, variables, and basic programming concepts",
                "React.js Complete Guide" when levelOrder == 1 => "React components, JSX, and the virtual DOM",
                "Python for Data Science" when levelOrder == 1 => "Python syntax and basic programming for data analysis",
                _ => $"essential concepts and practical applications for level {levelOrder}"
            };
        }

        private static string GetSectionName(string courseName, int levelOrder, int sectionOrder)
        {
            var sectionNames = new[]
            {
                "Introduction", "Core Concepts", "Practical Examples", "Hands-on Practice", "Advanced Topics", "Project Work"
            };

            return sectionOrder <= sectionNames.Length ? sectionNames[sectionOrder - 1] : $"Topic {sectionOrder}";
        }

        private static string GetContentTitle(string courseName, int levelOrder, int sectionOrder, int contentOrder)
        {
            var contentTitles = new[]
            {
                "Overview and Objectives",
                "Key Concepts Explained",
                "Step-by-Step Tutorial",
                "Practical Exercise",
                "Common Mistakes to Avoid",
                "Best Practices",
                "Real-World Examples",
                "Summary and Review"
            };

            return contentOrder <= contentTitles.Length ? contentTitles[contentOrder - 1] : $"Topic {contentOrder}";
        }

        private static ContentType GetRandomContentType()
        {
            var types = new[] { ContentType.Video, ContentType.Doc, ContentType.Text };
            return types[Random.Shared.Next(types.Length)];
        }

        private static string GetSampleTextContent(string courseName, int levelOrder, int sectionOrder, int contentOrder)
        {
            return $@"
                    # {courseName} - Level {levelOrder}, Section {sectionOrder}

                    ## Learning Objectives
                    By the end of this content, you will be able to:
                    - Understand the key concepts presented
                    - Apply the knowledge in practical scenarios
                    - Solve common problems related to this topic

                    ## Content Overview
                    This content covers important aspects of {courseName} specifically focusing on level {levelOrder} concepts. 

                    ### Key Points:
                    1. **Foundation**: Understanding the basic principles
                    2. **Application**: How to apply these concepts practically
                    3. **Best Practices**: Industry-standard approaches
                    4. **Common Pitfalls**: What to avoid and how to troubleshoot

                    ## Practical Examples
                    Here are some examples to help you understand the concepts better:

                    ```
                    // Sample code or example content would go here
                    // This varies based on the course subject matter
                    ```

                    ## Summary
                    This content provides essential knowledge for progressing in {courseName}. Make sure to practice the concepts before moving to the next content item.

                    ## Next Steps
                    - Complete the practice exercises
                    - Review the key concepts
                    - Prepare for the next content item
                    ";
        }

        private static List<string> GetAboutCourseItems(string courseName)
        {
            return courseName switch
            {
                "HTML & CSS Fundamentals" => new List<string>
                {
                    "Learn semantic HTML5 markup",
                    "Master CSS3 styling techniques",
                    "Build responsive web layouts",
                    "Understand CSS Grid and Flexbox",
                    "Create modern web interfaces"
                },
                "JavaScript Essentials" => new List<string>
                {
                    "Master JavaScript fundamentals",
                    "Learn ES6+ modern features",
                    "Understand asynchronous programming",
                    "Work with APIs and JSON",
                    "Debug JavaScript effectively"
                },
                "React.js Complete Guide" => new List<string>
                {
                    "Build dynamic user interfaces",
                    "Master React hooks and state management",
                    "Create reusable components",
                    "Handle forms and user input",
                    "Deploy React applications"
                },
                "Python for Data Science" => new List<string>
                {
                    "Learn Python programming basics",
                    "Work with NumPy and Pandas",
                    "Perform data analysis and visualization",
                    "Handle different data formats",
                    "Apply statistical concepts"
                },
                _ => new List<string>
                {
                    "Comprehensive course content",
                    "Hands-on practical exercises",
                    "Real-world project examples",
                    "Expert instructor guidance",
                    "Certificate upon completion"
                }
            };
        }

        private static List<string> GetCourseSkills(string courseName)
        {
            return courseName switch
            {
                "HTML & CSS Fundamentals" => new List<string> { "HTML5", "CSS3", "Responsive Design", "Web Standards", "Browser Compatibility" },
                "JavaScript Essentials" => new List<string> { "JavaScript", "ES6+", "DOM Manipulation", "Async/Await", "JSON" },
                "React.js Complete Guide" => new List<string> { "React", "JSX", "Hooks", "State Management", "Component Design" },
                "Node.js Backend Development" => new List<string> { "Node.js", "Express", "REST API", "Database", "Authentication" },
                "Python for Data Science" => new List<string> { "Python", "NumPy", "Pandas", "Data Analysis", "Statistics" },
                "Machine Learning Fundamentals" => new List<string> { "Machine Learning", "Scikit-learn", "Data Preprocessing", "Model Training", "Evaluation" },
                "Android Development with Kotlin" => new List<string> { "Kotlin", "Android Studio", "Mobile UI", "APIs", "App Store" },
                "iOS Development with Swift" => new List<string> { "Swift", "Xcode", "SwiftUI", "iOS SDK", "App Store" },
                "React Native Cross-Platform" => new List<string> { "React Native", "Cross-Platform", "Mobile Development", "JavaScript", "Native APIs" },
                "Flutter Mobile Development" => new List<string> { "Flutter", "Dart", "Cross-Platform", "Material Design", "iOS Design" },
                "Data Visualization" => new List<string> { "Data Visualization", "Charts", "Dashboards", "Matplotlib", "Seaborn" },
                "Deep Learning with TensorFlow" => new List<string> { "TensorFlow", "Neural Networks", "Deep Learning", "CNN", "RNN" },
                _ => new List<string> { "Problem Solving", "Critical Thinking", "Technical Skills", "Best Practices", "Project Management" }
            };
        }

        private static async Task<SeedingStats> GetSeedingStatsAsync(ApplicationDbContext context)
        {
            return new SeedingStats
            {
                TotalUsers = await context.Users.CountAsync(),
                AdminCount = await context.Users.CountAsync(u => u.Role == UserRole.Admin),
                InstructorCount = await context.Users.CountAsync(u => u.Role == UserRole.Instructor),
                TrackCount = await context.CourseTracks.CountAsync(),
                CourseCount = await context.Courses.CountAsync(),
                LevelCount = await context.Levels.CountAsync(),
                SectionCount = await context.Sections.CountAsync(),
                ContentCount = await context.Contents.CountAsync(),
                SkillCount = await context.CourseSkills.CountAsync(),
                AboutItemCount = await context.AboutCourses.CountAsync()
            };
        }

        private class SeedingStats
        {
            public int TotalUsers { get; set; }
            public int AdminCount { get; set; }
            public int InstructorCount { get; set; }
            public int TrackCount { get; set; }
            public int CourseCount { get; set; }
            public int LevelCount { get; set; }
            public int SectionCount { get; set; }
            public int ContentCount { get; set; }
            public int SkillCount { get; set; }
            public int AboutItemCount { get; set; }
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

    }
}