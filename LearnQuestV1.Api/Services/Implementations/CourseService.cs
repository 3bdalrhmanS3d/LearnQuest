using LearnQuestV1.Api.DTOs.Courses;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models.CourseStructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;


namespace LearnQuestV1.Api.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _uow;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CourseService(IUnitOfWork uow, IHttpContextAccessor httpContextAccessor)
        {
            _uow = uow;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Returns all courses belonging to the current instructor.
        /// </summary>
        public async Task<IEnumerable<CourseCDto>> GetAllCoursesForInstructorAsync()
        {
            var user = (_httpContextAccessor.HttpContext?.User) ?? throw new InvalidOperationException("Unable to determine current user.");
            var instructorId = user.GetCurrentUserId();
            if (instructorId == null)
                throw new InvalidOperationException("Unable to determine current user ID.");

            // Verify role = Instructor (optional)  
            var found = await _uow.Users.Query()
                .AnyAsync(u => u.UserId == instructorId.Value
                               && u.Role == UserRole.Instructor
                               && !u.IsDeleted);

            if (!found)
                throw new KeyNotFoundException("Instructor not found or is deleted.");

            var courses = await _uow.Courses.Query()
                .Where(c => c.InstructorId == instructorId.Value && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            if (courses == null || !courses.Any())
                return Enumerable.Empty<CourseCDto>();

            return courses.Select(c => new CourseCDto
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                CourseImage = c.CourseImage ?? string.Empty,
                CoursePrice = c.CoursePrice,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            });
        }

        /// <summary>
        /// Returns full details for a single course (AboutCourse + CourseSkill included).
        /// </summary>
        public async Task<CourseDetailsDto> GetCourseDetailsAsync(int courseId)
        {
            var user = (_httpContextAccessor.HttpContext?.User) ?? throw new InvalidOperationException("Unable to determine current user.");
            var instructorId = user.GetCurrentUserId();
            if (instructorId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            // Load course and verify ownership
            var course = await _uow.Courses.Query()
                .Include(c => c.AboutCourses)
                .Include(c => c.CourseSkills)
                .FirstOrDefaultAsync(c => c.CourseId == courseId
                                          && c.InstructorId == instructorId.Value
                                          && !c.IsDeleted);
            if (course == null)
                throw new KeyNotFoundException($"Course {courseId} not found or not owned by you.");

            var dto = new CourseDetailsDto
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                Description = course.Description,
                CourseImage = course.CourseImage ?? string.Empty,
                CoursePrice = course.CoursePrice,
                IsActive = course.IsActive,
                CreatedAt = course.CreatedAt,
                AboutCourses = course.AboutCourses.Select(a => new AboutCourseItem
                {
                    AboutCourseId = a.AboutCourseId,
                    AboutCourseText = a.AboutCourseText,
                    OutcomeType = a.OutcomeType.ToString()
                }),
                CourseSkills = course.CourseSkills.Select(s => new CourseSkillItem
                {
                    CourseSkillId = s.CourseSkillId,
                    CourseSkillText = s.CourseSkillText
                })
            };

            return dto;
        }

        /// <summary>
        /// Creates a new course under the current instructor, including AboutCourse and CourseSkill entries.
        /// Returns the new CourseId.
        /// </summary>
        public async Task<int> CreateCourseAsync(CreateCourseDto input)
        {
            var user = (_httpContextAccessor.HttpContext?.User) ?? throw new InvalidOperationException("Unable to determine current user.");
            var instructorId = user.GetCurrentUserId();
            if (instructorId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            // Verify instructor exists
            var exists = await _uow.Users.Query()
                .AnyAsync(u => u.UserId == instructorId.Value
                               && u.Role == UserRole.Instructor
                               && !u.IsDeleted);
            if (!exists)
                throw new KeyNotFoundException("Instructor not found or is deleted.");

            // Create course entity
            var newCourse = new Course
            {
                CourseName = input.CourseName.Trim(),
                Description = input.Description.Trim(),
                CoursePrice = input.CoursePrice,
                CourseImage = string.IsNullOrWhiteSpace(input.CourseImage)
                              ? "/uploads/courses/default.jpg"
                              : input.CourseImage.Trim(),
                InstructorId = instructorId.Value,
                CreatedAt = DateTime.UtcNow,
                IsActive = input.IsActive
            };

            await _uow.Courses.AddAsync(newCourse);
            await _uow.SaveAsync();

            // If AboutCourse inputs provided
            if (input.AboutCourseInputs != null && input.AboutCourseInputs.Any())
            {
                foreach (var aboutInput in input.AboutCourseInputs)
                {
                    var about = new AboutCourse
                    {
                        CourseId = newCourse.CourseId,
                        AboutCourseText = FormatText(aboutInput.AboutCourseText),
                        OutcomeType = Enum.TryParse<Core.Enums.CourseOutcomeType>(aboutInput.OutcomeType, true, out var o)
                                      ? o
                                      : Core.Enums.CourseOutcomeType.Learn
                    };

                    await _uow.AboutCourses.AddAsync(about);
                }
            }

            // If CourseSkill inputs provided
            if (input.CourseSkillInputs != null && input.CourseSkillInputs.Any())
            {
                foreach (var skillInput in input.CourseSkillInputs)
                {
                    var skill = new CourseSkill
                    {
                        CourseId = newCourse.CourseId,
                        CourseSkillText = FormatText(skillInput.CourseSkillText)
                    };
                    await _uow.CourseSkills.AddAsync(skill);
                }
            }

            await _uow.SaveAsync();
            return newCourse.CourseId;
        }

        /// <summary>
        /// Updates an existing course (and its AboutCourse/Skills).
        /// </summary>
        public async Task UpdateCourseAsync(int courseId, UpdateCourseDto input)
        {
            var user = (_httpContextAccessor.HttpContext?.User) ?? throw new InvalidOperationException("Unable to determine current user.");
            var instructorId = user.GetCurrentUserId();
            if (instructorId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            // Load course with related collections
            var course = await _uow.Courses.Query()
                .Include(c => c.AboutCourses)
                .Include(c => c.CourseSkills)
                .FirstOrDefaultAsync(c => c.CourseId == courseId
                                          && c.InstructorId == instructorId.Value
                                          && !c.IsDeleted);
            if (course == null)
                throw new KeyNotFoundException($"Course {courseId} not found or not owned by you.");

            // Update basic fields
            if (!string.IsNullOrWhiteSpace(input.CourseName))
                course.CourseName = input.CourseName.Trim();
            if (!string.IsNullOrWhiteSpace(input.Description))
                course.Description = input.Description.Trim();
            if (input.CoursePrice.HasValue && input.CoursePrice.Value >= 0)
                course.CoursePrice = input.CoursePrice.Value;
            if (!string.IsNullOrWhiteSpace(input.CourseImage))
                course.CourseImage = input.CourseImage.Trim();
            if (input.IsActive.HasValue)
                course.IsActive = input.IsActive.Value;

            // Handle AboutCourse updates/deletes/adds
            if (input.AboutCourseInputs != null)
            {
                var existingAbout = course.AboutCourses ?? new List<AboutCourse>();
                var updatedIds = input.AboutCourseInputs.Select(ai => ai.AboutCourseId).ToList();

                // Delete any removed items
                foreach (var a in existingAbout.Where(a => !updatedIds.Contains(a.AboutCourseId)).ToList())
                {
                    _uow.AboutCourses.Remove(a);
                }

                // Add or update
                foreach (var aboutInput in input.AboutCourseInputs)
                {
                    if (aboutInput.AboutCourseId == 0)
                    {
                        // new item
                        var newAbout = new AboutCourse
                        {
                            CourseId = course.CourseId,
                            AboutCourseText = FormatText(aboutInput.AboutCourseText),
                            OutcomeType = Enum.TryParse<Core.Enums.CourseOutcomeType>(aboutInput.OutcomeType, true, out var o)
                                          ? o
                                          : Core.Enums.CourseOutcomeType.Learn
                        };
                        await _uow.AboutCourses.AddAsync(newAbout);
                    }
                    else
                    {
                        // existing item
                        var existing = existingAbout.FirstOrDefault(a => a.AboutCourseId == aboutInput.AboutCourseId);
                        if (existing != null)
                        {
                            existing.AboutCourseText = FormatText(aboutInput.AboutCourseText);
                            existing.OutcomeType = Enum.TryParse<Core.Enums.CourseOutcomeType>(aboutInput.OutcomeType, true, out var o2)
                                                   ? o2
                                                   : Core.Enums.CourseOutcomeType.Learn;
                            _uow.AboutCourses.Update(existing);
                        }
                    }
                }
            }

            // Handle CourseSkill updates/deletes/adds
            if (input.CourseSkillInputs != null)
            {
                var existingSkills = course.CourseSkills ?? new List<CourseSkill>();
                var updatedSkillIds = input.CourseSkillInputs.Select(si => si.CourseSkillId).ToList();

                // Delete any removed items
                foreach (var s in existingSkills.Where(s => !updatedSkillIds.Contains(s.CourseSkillId)).ToList())
                {
                    _uow.CourseSkills.Remove(s);
                }

                // Add or update
                foreach (var skillInput in input.CourseSkillInputs)
                {
                    if (skillInput.CourseSkillId == 0)
                    {
                        var newSkill = new CourseSkill
                        {
                            CourseId = course.CourseId,
                            CourseSkillText = FormatText(skillInput.CourseSkillText)
                        };
                        await _uow.CourseSkills.AddAsync(newSkill);
                    }
                    else
                    {
                        var existingSkill = existingSkills.FirstOrDefault(s => s.CourseSkillId == skillInput.CourseSkillId);
                        if (existingSkill != null)
                        {
                            existingSkill.CourseSkillText = FormatText(skillInput.CourseSkillText);
                            _uow.CourseSkills.Update(existingSkill);
                        }
                    }
                }
            }

            _uow.Courses.Update(course);
            await _uow.SaveAsync();
        }

        /// <summary>
        /// Soft‐deletes the course (sets IsDeleted = true).
        /// </summary>
        public async Task DeleteCourseAsync(int courseId)
        {
            var user = (_httpContextAccessor.HttpContext?.User) ?? throw new InvalidOperationException("Unable to determine current user.");
            var instructorId = user.GetCurrentUserId();
            if (instructorId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            var course = await _uow.Courses.Query()
                .FirstOrDefaultAsync(c => c.CourseId == courseId
                                          && c.InstructorId == instructorId.Value
                                          && !c.IsDeleted);
            if (course == null)
                throw new KeyNotFoundException($"Course {courseId} not found or not owned by you.");

            course.IsDeleted = true;
            _uow.Courses.Update(course);
            await _uow.SaveAsync();
        }

        /// <summary>
        /// Toggles the course’s IsActive flag (active/inactive).
        /// </summary>
        public async Task ToggleCourseStatusAsync(int courseId)
        {
            var user = (_httpContextAccessor.HttpContext?.User) ?? throw new InvalidOperationException("Unable to determine current user.");
            var instructorId = user.GetCurrentUserId();
            if (instructorId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            var course = await _uow.Courses.Query()
                .FirstOrDefaultAsync(c => c.CourseId == courseId
                                          && c.InstructorId == instructorId.Value
                                          && !c.IsDeleted);
            if (course == null)
                throw new KeyNotFoundException($"Course {courseId} not found or not owned by you.");

            course.IsActive = !course.IsActive;
            _uow.Courses.Update(course);
            await _uow.SaveAsync();
        }

        /// <summary>
        /// Saves an uploaded image to wwwroot/uploads/courses/ and returns its relative URL.
        /// </summary>
        public async Task<string> UploadCourseImageAsync(int courseId, IFormFile file)
        {
            var user = (_httpContextAccessor.HttpContext?.User) ?? throw new InvalidOperationException("Unable to determine current user.");
            var instructorId = user.GetCurrentUserId();
            if (instructorId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            var course = await _uow.Courses.Query()
                .FirstOrDefaultAsync(c => c.CourseId == courseId
                                          && c.InstructorId == instructorId.Value
                                          && !c.IsDeleted);
            if (course == null)
                throw new KeyNotFoundException($"Course {courseId} not found or not owned by you.");

            if (file == null || file.Length == 0)
                throw new InvalidOperationException("No file uploaded.");

            // Save to disk under wwwroot/uploads/courses/
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "courses");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"course_{courseId}_{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Set relative URL
            var relativeUrl = $"/uploads/courses/{fileName}";
            course.CourseImage = relativeUrl;
            _uow.Courses.Update(course);
            await _uow.SaveAsync();

            return relativeUrl;
        }

        // Helper: Trim + collapse whitespace + capitalize first letter
        private static string FormatText(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            input = input.Trim();
            input = System.Text.RegularExpressions.Regex.Replace(input, @"\s+", " ");
            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }
    }
}
