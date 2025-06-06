using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.DTOs.Levels;
using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class LevelService : ILevelService
    {
        //private readonly ILevelRepository _levelRepository;
        //private readonly ICourseRepository _courseRepository;
        private readonly IUnitOfWork _uow;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LevelService(IUnitOfWork uow, IHttpContextAccessor httpContextAccessor)
        {
            _uow = uow;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Creates a new level under the specified course. Returns the new LevelId.
        /// </summary>
        public async Task<int> CreateLevelAsync(CreateLevelDto input)
        {
            // 1) Ensure current user is an instructor and not deleted
            var user = _httpContextAccessor.HttpContext?.User;
            var instructorId = user?.GetCurrentUserId();
            if (instructorId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            var isInstructor = await _uow.Users.Query()
                .AnyAsync(u => u.UserId == instructorId.Value
                               && u.Role == UserRole.Instructor
                               && !u.IsDeleted);
            if (!isInstructor)
                throw new KeyNotFoundException("Instructor not found or is deleted.");

            // 2) Verify that the course exists and belongs to this instructor
            var course = await _uow.Courses.Query()
                .FirstOrDefaultAsync(c => c.CourseId == input.CourseId
                                          && c.InstructorId == instructorId.Value
                                          && !c.IsDeleted);
            if (course == null)
                throw new KeyNotFoundException("Course not found or not owned by you.");

            // 3) Determine next LevelOrder for this course
            var existingCount = await _uow.Levels.Query()
                .CountAsync(l => l.CourseId == input.CourseId && !l.IsDeleted);
            int nextOrder = existingCount + 1;

            bool requiresPrevious = nextOrder > 1;

            // 4) Create and save the new Level
            var newLevel = new Level
            {
                CourseId = input.CourseId,
                LevelOrder = nextOrder,
                LevelName = input.LevelName.Trim(),
                LevelDetails = input.LevelDetails?.Trim() ?? string.Empty,
                IsVisible = input.IsVisible,
                RequiresPreviousLevelCompletion = requiresPrevious,
                IsDeleted = false
            };

            await _uow.Levels.AddAsync(newLevel);
            await _uow.SaveAsync();

            return newLevel.LevelId;
        }

        /// <summary>
        /// Updates an existing level’s name/details (if owned by this instructor).
        /// </summary>
        public async Task UpdateLevelAsync(UpdateLevelDto input)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var instructorId = user?.GetCurrentUserId();
            if (instructorId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            // Fetch the Level, including its Course → check ownership
            var level = await _uow.Levels.Query()
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LevelId == input.LevelId
                                          && l.Course.InstructorId == instructorId.Value
                                          && !l.IsDeleted);
            if (level == null)
                throw new KeyNotFoundException("Level not found or not owned by you.");

            if (!string.IsNullOrWhiteSpace(input.LevelName))
                level.LevelName = input.LevelName.Trim();

            if (!string.IsNullOrWhiteSpace(input.LevelDetails))
                level.LevelDetails = input.LevelDetails.Trim();

            _uow.Levels.Update(level);
            await _uow.SaveAsync();
        }

        /// <summary>
        /// Soft-deletes a level by setting IsDeleted=true (if it belongs to the instructor).
        /// </summary>
        public async Task DeleteLevelAsync(int levelId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var instructorId = user?.GetCurrentUserId();
            if (instructorId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            var level = await _uow.Levels.Query()
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LevelId == levelId
                                          && l.Course.InstructorId == instructorId.Value
                                          && !l.IsDeleted);
            if (level == null)
                throw new KeyNotFoundException("Level not found or not owned by you.");

            level.IsDeleted = true;
            _uow.Levels.Update(level);
            await _uow.SaveAsync();
        }

        /// <summary>
        /// Returns all non-deleted levels for a given course (ordered by LevelOrder),
        /// only if the current instructor owns that course.
        /// </summary>
        public async Task<IEnumerable<LevelSummaryDto>> GetCourseLevelsAsync(int courseId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var instructorId = user?.GetCurrentUserId();
            if (instructorId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            // Verify that the course belongs to this instructor
            var course = await _uow.Courses.Query()
                .FirstOrDefaultAsync(c => c.CourseId == courseId
                                          && c.InstructorId == instructorId.Value
                                          && !c.IsDeleted);
            if (course == null)
                throw new KeyNotFoundException("Course not found or not owned by you.");

            var levels = await _uow.Levels.Query()
                .Where(l => l.CourseId == courseId && !l.IsDeleted)
                .OrderBy(l => l.LevelOrder)
                .ToListAsync();

            return levels.Select(l => new LevelSummaryDto
            {
                LevelId = l.LevelId,
                CourseId = l.CourseId,
                LevelOrder = l.LevelOrder,
                LevelName = l.LevelName,
                IsVisible = l.IsVisible,
                RequiresPreviousLevelCompletion = l.RequiresPreviousLevelCompletion
            });
        }

        /// <summary>
        /// Toggles IsVisible on a level (if it belongs to the current instructor) and returns the new state.
        /// </summary>
        public async Task<VisibilityToggleResultDto> ToggleLevelVisibilityAsync(int levelId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var instructorId = user?.GetCurrentUserId();
            if (instructorId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            var level = await _uow.Levels.Query()
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LevelId == levelId
                                          && l.Course.InstructorId == instructorId.Value
                                          && !l.IsDeleted);
            if (level == null)
                throw new KeyNotFoundException("Level not found or not owned by you.");

            level.IsVisible = !level.IsVisible;
            _uow.Levels.Update(level);
            await _uow.SaveAsync();

            return new VisibilityToggleResultDto
            {
                LevelId = level.LevelId,
                IsNowVisible = level.IsVisible,
                Message = level.IsVisible ? "Level is now visible." : "Level is now hidden."
            };
        }

        /// <summary>
        /// Reorders multiple levels (each tuple has LevelId + NewOrder). Ignores any mismatched items.
        /// </summary>
        public async Task ReorderLevelsAsync(IEnumerable<ReorderLevelDto> reorderItems)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var instructorId = user?.GetCurrentUserId();
            if (instructorId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            foreach (var item in reorderItems)
            {
                var level = await _uow.Levels.Query()
                    .Include(l => l.Course)
                    .FirstOrDefaultAsync(l => l.LevelId == item.LevelId
                                              && l.Course.InstructorId == instructorId.Value
                                              && !l.IsDeleted);
                if (level != null)
                {
                    level.LevelOrder = item.NewOrder;
                    _uow.Levels.Update(level);
                }
            }

            await _uow.SaveAsync();
        }

        /// <summary>
        /// Returns how many distinct users have reached this level.
        /// </summary>
        public async Task<LevelStatsDto> GetLevelStatsAsync(int levelId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var instructorId = user?.GetCurrentUserId();
            if (instructorId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            // Verify ownership
            var level = await _uow.Levels.Query()
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LevelId == levelId
                                          && l.Course.InstructorId == instructorId.Value
                                          && !l.IsDeleted);
            if (level == null)
                throw new KeyNotFoundException("Level not found or not owned by you.");

            // Count distinct users who have a UserProgress with CurrentLevelId = this level
            var usersReached = await _uow.UserProgresses.Query()
                .Where(p => p.CurrentLevelId == levelId)
                .Select(p => p.UserId)
                .Distinct()
                .CountAsync();

            return new LevelStatsDto
            {
                LevelId = level.LevelId,
                LevelName = level.LevelName,
                UsersReachedCount = usersReached
            };
        }
    }
    
}
