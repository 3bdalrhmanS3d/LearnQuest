using LearnQuestV1.Api.DTOs.Contents;

namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IContentCachingService
    {
        // Basic caching operations
        Task<T?> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
        Task RemoveAsync(string key);
        Task RemovePatternAsync(string pattern);

        // Content-specific caching methods
        Task<IEnumerable<ContentSummaryDto>?> GetSectionContentsAsync(int sectionId, int userId);
        Task SetSectionContentsAsync(int sectionId, int userId, IEnumerable<ContentSummaryDto> contents);
        Task InvalidateSectionContentsAsync(int sectionId);

        Task<ContentDetailsDto?> GetContentDetailsAsync(int contentId);
        Task SetContentDetailsAsync(int contentId, ContentDetailsDto details);
        Task InvalidateContentDetailsAsync(int contentId);

        Task<ContentStatsDto?> GetContentStatsAsync(int contentId);
        Task SetContentStatsAsync(int contentId, ContentStatsDto stats);
        Task InvalidateContentStatsAsync(int contentId);

        // Bulk invalidation methods
        Task InvalidateUserContentCacheAsync(int userId);
        Task InvalidateInstructorContentCacheAsync(int instructorId);
        Task InvalidateCourseContentCacheAsync(int courseId);

        // Cache warming and preloading
        Task WarmupSectionCacheAsync(int sectionId, int userId);
        Task WarmupUserContentCacheAsync(int userId);
        Task WarmupInstructorContentCacheAsync(int instructorId);

        // Cache statistics and monitoring
        Task<CacheStatisticsDto> GetCacheStatisticsAsync();
        Task<IEnumerable<CacheEntryInfoDto>> GetCacheEntriesAsync(string? pattern = null);
        Task ClearAllContentCacheAsync();
    }
}
