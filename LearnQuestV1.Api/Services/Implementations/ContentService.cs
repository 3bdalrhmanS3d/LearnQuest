// LearnQuestV1.Api/Services/Implementations/ContentService.cs
using LearnQuestV1.Api.DTOs.Contents;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using LearnQuestV1.Core.Models.CourseStructure;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class ContentService : IContentService
    {
        private readonly IUnitOfWork _uow;

        public ContentService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<int> CreateContentAsync(CreateContentDto input, int instructorId)
        {
            // 1) Verify that the section exists and belongs to this instructor
            var section = await _uow.Sections.Query()
                .Include(s => s.Level)
                    .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(s =>
                    s.SectionId == input.SectionId &&
                    s.Level.Course.InstructorId == instructorId);

            if (section == null)
                throw new KeyNotFoundException($"Section {input.SectionId} not found or not owned by this instructor.");

            // 2) Enforce type‐specific requirements
            switch (input.ContentType)
            {
                case Core.Enums.ContentType.Video:
                    if (string.IsNullOrWhiteSpace(input.ContentUrl))
                        throw new InvalidOperationException("Video URL is required when ContentType is Video.");
                    break;
                case Core.Enums.ContentType.Doc:
                    if (string.IsNullOrWhiteSpace(input.ContentDoc))
                        throw new InvalidOperationException("Document path is required when ContentType is Doc.");
                    break;
                case Core.Enums.ContentType.Text:
                    if (string.IsNullOrWhiteSpace(input.ContentText))
                        throw new InvalidOperationException("Text is required when ContentType is Text.");
                    break;
                default:
                    throw new InvalidOperationException("Unsupported ContentType.");
            }

            // 3) Determine next ContentOrder under this section
            var nextOrder = await _uow.Contents.Query()
                .CountAsync(c => c.SectionId == input.SectionId) + 1;

            // 4) Create and save
            var entity = new Content
            {
                SectionId = input.SectionId,
                Title = input.Title.Trim(),
                ContentType = input.ContentType,
                ContentUrl = input.ContentType == Core.Enums.ContentType.Video
                             ? input.ContentUrl
                             : null,
                ContentText = input.ContentType == Core.Enums.ContentType.Text
                              ? input.ContentText
                              : null,
                ContentDoc = input.ContentType == Core.Enums.ContentType.Doc
                             ? input.ContentDoc
                             : null,
                DurationInMinutes = input.DurationInMinutes,
                ContentDescription = input.ContentDescription?.Trim(),
                ContentOrder = nextOrder,
                CreatedAt = DateTime.UtcNow,
                IsVisible = true
            };

            await _uow.Contents.AddAsync(entity);
            await _uow.SaveAsync();

            return entity.ContentId;
        }

        public async Task<string> UploadContentFileAsync(IFormFile file, Core.Enums.ContentType type)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("No file uploaded.");

            // Decide folder: “videos” or “docs”
            var folderName = type == Core.Enums.ContentType.Video ? "videos" : "docs";
            var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(wwwroot, "uploads", folderName);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return a publicly accessible URL:
            return $"/uploads/{folderName}/{fileName}";
        }

        public async Task UpdateContentAsync(UpdateContentDto input, int instructorId)
        {
            // 1) Fetch the content and verify ownership
            var content = await _uow.Contents.Query()
                .Include(c => c.Section)
                    .ThenInclude(s => s.Level)
                        .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(c =>
                    c.ContentId == input.ContentId &&
                    c.Section.Level.Course.InstructorId == instructorId);

            if (content == null)
                throw new KeyNotFoundException($"Content {input.ContentId} not found or not owned by this instructor.");

            // 2) Only update fields if provided
            if (!string.IsNullOrWhiteSpace(input.Title))
                content.Title = input.Title.Trim();

            if (!string.IsNullOrWhiteSpace(input.ContentDescription))
                content.ContentDescription = input.ContentDescription.Trim();

            if (input.DurationInMinutes.HasValue)
                content.DurationInMinutes = input.DurationInMinutes.Value;

            // 3) Depending on content type, update the appropriate field
            switch (content.ContentType)
            {
                case Core.Enums.ContentType.Text:
                    if (!string.IsNullOrWhiteSpace(input.ContentText))
                        content.ContentText = input.ContentText;
                    break;

                case Core.Enums.ContentType.Video:
                    if (!string.IsNullOrWhiteSpace(input.ContentUrl))
                        content.ContentUrl = input.ContentUrl;
                    break;

                case Core.Enums.ContentType.Doc:
                    if (!string.IsNullOrWhiteSpace(input.ContentDoc))
                        content.ContentDoc = input.ContentDoc;
                    break;
            }

            _uow.Contents.Update(content);
            await _uow.SaveAsync();
        }

        public async Task DeleteContentAsync(int contentId, int instructorId)
        {
            // 1) Fetch and verify ownership
            var content = await _uow.Contents.Query()
                .Include(c => c.Section)
                    .ThenInclude(s => s.Level)
                        .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(c =>
                    c.ContentId == contentId &&
                    c.Section.Level.Course.InstructorId == instructorId);

            if (content == null)
                throw new KeyNotFoundException($"Content {contentId} not found or not owned by this instructor.");

            // 2) Remove it
            _uow.Contents.Remove(content);
            await _uow.SaveAsync();
        }

        public async Task ReorderContentsAsync(IEnumerable<ReorderContentDto> input, int instructorId)
        {
            foreach (var dto in input)
            {
                // Fetch each content and verify ownership
                var content = await _uow.Contents.Query()
                    .Include(c => c.Section)
                        .ThenInclude(s => s.Level)
                            .ThenInclude(l => l.Course)
                    .FirstOrDefaultAsync(c =>
                        c.ContentId == dto.ContentId &&
                        c.Section.Level.Course.InstructorId == instructorId);

                if (content != null)
                {
                    content.ContentOrder = dto.NewOrder;
                    _uow.Contents.Update(content);
                }
                // If content is null or not owned by this instructor, skip silently.
            }

            await _uow.SaveAsync();
        }

        public async Task<bool> ToggleContentVisibilityAsync(int contentId, int instructorId)
        {
            // 1) Fetch and verify ownership
            var content = await _uow.Contents.Query()
                .Include(c => c.Section)
                    .ThenInclude(s => s.Level)
                        .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(c =>
                    c.ContentId == contentId &&
                    c.Section.Level.Course.InstructorId == instructorId);

            if (content == null)
                throw new KeyNotFoundException($"Content {contentId} not found or not owned by this instructor.");

            content.IsVisible = !content.IsVisible;
            _uow.Contents.Update(content);
            await _uow.SaveAsync();

            return content.IsVisible;
        }

        public async Task<IEnumerable<ContentSummaryDto>> GetSectionContentsAsync(int sectionId, int instructorId)
        {
            // 1) Verify that the section belongs to this instructor
            var section = await _uow.Sections.Query()
                .Include(s => s.Level)
                    .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(s =>
                    s.SectionId == sectionId &&
                    s.Level.Course.InstructorId == instructorId &&
                    !s.IsDeleted);

            if (section == null)
                throw new KeyNotFoundException($"Section {sectionId} not found or not owned by this instructor.");

            // 2) Fetch all non‐deleted, visible contents under that section, ordered by ContentOrder
            var items = await _uow.Contents.Query()
                .Where(c => c.SectionId == sectionId && !c.Section.IsDeleted)
                .OrderBy(c => c.ContentOrder)
                .ToListAsync();

            return items.Select(c => new ContentSummaryDto
            {
                ContentId = c.ContentId,
                Title = c.Title,
                ContentType = c.ContentType,
                IsVisible = c.IsVisible,
                ContentOrder = c.ContentOrder
            });
        }

        public async Task<ContentStatsDto> GetContentStatsAsync(int contentId, int instructorId)
        {
            // 1) Verify ownership
            var content = await _uow.Contents.Query()
                .Include(c => c.Section)
                    .ThenInclude(s => s.Level)
                        .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(c =>
                    c.ContentId == contentId &&
                    c.Section.Level.Course.InstructorId == instructorId);

            if (content == null)
                throw new KeyNotFoundException($"Content {contentId} not found or not owned by this instructor.");

            // 2) Count how many users have an activity entry with EndTime != null for this content
            var usersReached = await _uow.UserContentActivities.Query()
                .CountAsync(a =>
                    a.ContentId == contentId &&
                    a.EndTime != null);

            return new ContentStatsDto
            {
                ContentId = contentId,
                Title = content.Title,
                UsersReached = usersReached
            };
        }
    }
}
