using LearnQuestV1.Api.DTOs.Sections;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Models.CourseStructure;
using Microsoft.EntityFrameworkCore;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class SectionService : ISectionService
    {
        private readonly IUnitOfWork _uow;

        public SectionService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<int> CreateSectionAsync(CreateSectionDto dto, int instructorId)
        {
            // 1) Verify that the level exists and is owned by this instructor
            var level = await _uow.Levels.Query()
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LevelId == dto.LevelId
                                          && l.Course.InstructorId == instructorId
                                          && !l.Course.IsDeleted);
            if (level == null)
                throw new KeyNotFoundException($"Level {dto.LevelId} not found or not owned by instructor.");

            // 2) Determine the next SectionOrder (only count non-deleted)
            var existingCount = await _uow.Sections.Query()
                .CountAsync(s => s.LevelId == dto.LevelId && !s.IsDeleted);
            int nextOrder = existingCount + 1;

            // 3) Compute RequiresPreviousSectionCompletion
            bool requiresPrevious = nextOrder != 1;

            // 4) Create new Section
            var section = new Section
            {
                LevelId = dto.LevelId,
                SectionName = dto.SectionName.Trim(),
                SectionOrder = nextOrder,
                RequiresPreviousSectionCompletion = requiresPrevious,
                IsVisible = true,
                IsDeleted = false
            };

            await _uow.Sections.AddAsync(section);
            await _uow.SaveAsync();

            return section.SectionId;
        }

        public async Task UpdateSectionAsync(UpdateSectionDto dto, int instructorId)
        {
            // 1) Find section and verify ownership
            var section = await _uow.Sections.Query()
                .Include(s => s.Level)
                    .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(s => s.SectionId == dto.SectionId
                                          && s.Level.Course.InstructorId == instructorId
                                          && !s.Level.Course.IsDeleted);
            if (section == null)
                throw new KeyNotFoundException($"Section {dto.SectionId} not found or not owned by instructor.");

            // 2) Update name if provided
            if (!string.IsNullOrWhiteSpace(dto.SectionName))
            {
                section.SectionName = dto.SectionName.Trim();
            }

            _uow.Sections.Update(section);
            await _uow.SaveAsync();
        }

        public async Task DeleteSectionAsync(int sectionId, int instructorId)
        {
            // 1) Find section and verify ownership
            var section = await _uow.Sections.Query()
                .Include(s => s.Level)
                    .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(s => s.SectionId == sectionId
                                          && s.Level.Course.InstructorId == instructorId
                                          && !s.Level.Course.IsDeleted);
            if (section == null)
                throw new KeyNotFoundException($"Section {sectionId} not found or not owned by instructor.");

            // 2) Soft‐delete
            section.IsDeleted = true;
            _uow.Sections.Update(section);
            await _uow.SaveAsync();
        }

        public async Task ReorderSectionsAsync(IEnumerable<ReorderSectionDto> dtos, int instructorId)
        {
            // We will iterate through each reordering request
            foreach (var item in dtos)
            {
                var section = await _uow.Sections.Query()
                    .Include(s => s.Level)
                        .ThenInclude(l => l.Course)
                    .FirstOrDefaultAsync(s => s.SectionId == item.SectionId
                                              && s.Level.Course.InstructorId == instructorId
                                              && !s.Level.Course.IsDeleted);
                if (section == null)
                    throw new KeyNotFoundException($"Section {item.SectionId} not found or not owned by instructor.");

                section.SectionOrder = item.NewOrder;
                _uow.Sections.Update(section);
            }

            await _uow.SaveAsync();
        }

        public async Task<bool> ToggleSectionVisibilityAsync(int sectionId, int instructorId)
        {
            // 1) Find section and verify ownership
            var section = await _uow.Sections.Query()
                .Include(s => s.Level)
                    .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(s => s.SectionId == sectionId
                                          && s.Level.Course.InstructorId == instructorId
                                          && !s.Level.Course.IsDeleted);
            if (section == null)
                throw new KeyNotFoundException($"Section {sectionId} not found or not owned by instructor.");

            // 2) Flip IsVisible
            section.IsVisible = !section.IsVisible;
            _uow.Sections.Update(section);
            await _uow.SaveAsync();

            return section.IsVisible;
        }

        public async Task<IList<SectionSummaryDto>> GetCourseSectionsAsync(int levelId, int instructorId)
        {
            // 1) Verify that level exists and is owned by instructor
            var level = await _uow.Levels.Query()
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LevelId == levelId
                                          && l.Course.InstructorId == instructorId
                                          && !l.Course.IsDeleted);
            if (level == null)
                throw new KeyNotFoundException($"Level {levelId} not found or not owned by instructor.");

            // 2) Fetch all non-deleted sections for that level, in SectionOrder
            var sections = await _uow.Sections.Query()
                .Where(s => s.LevelId == levelId && !s.IsDeleted)
                .OrderBy(s => s.SectionOrder)
                .ToListAsync();

            // 3) Map to DTO
            return sections.Select(s => new SectionSummaryDto
            {
                SectionId = s.SectionId,
                SectionName = s.SectionName,
                SectionOrder = s.SectionOrder,
                IsVisible = s.IsVisible
            }).ToList();
        }

        public async Task<SectionStatsDto> GetSectionStatsAsync(int sectionId, int instructorId)
        {
            // 1) Verify that section exists and is owned by instructor
            var section = await _uow.Sections.Query()
                .Include(s => s.Level)
                    .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(s => s.SectionId == sectionId
                                          && s.Level.Course.InstructorId == instructorId
                                          && !s.Level.Course.IsDeleted);
            if (section == null)
                throw new KeyNotFoundException($"Section {sectionId} not found or not owned by instructor.");

            // 2) Count how many users have progressed to this section
            var usersReached = await _uow.UserProgresses.Query()
                .CountAsync(p => p.CurrentSectionId == sectionId);

            return new SectionStatsDto
            {
                SectionId = section.SectionId,
                SectionName = section.SectionName,
                UsersReached = usersReached
            };
        }
    }
}
