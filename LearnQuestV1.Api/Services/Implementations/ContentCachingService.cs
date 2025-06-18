using LearnQuestV1.Api.DTOs.Contents;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using LearnQuestV1.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LearnQuestV1.Api.Services.Implementations
{
    /// <summary>
    /// Service for managing content-related caching with intelligent invalidation
    /// </summary>
    
    public class ContentCachingService : IContentCachingService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<ContentCachingService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        // Cache configuration
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(15);
        private readonly TimeSpan _shortExpiration = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _longExpiration = TimeSpan.FromHours(1);
        private readonly TimeSpan _statsExpiration = TimeSpan.FromMinutes(2);

        // Cache key patterns
        private const string SECTION_CONTENTS_KEY = "content:section:{0}:user:{1}";
        private const string CONTENT_DETAILS_KEY = "content:details:{0}";
        private const string CONTENT_STATS_KEY = "content:stats:{0}";
        private const string USER_CONTENT_KEY = "content:user:{0}";
        private const string INSTRUCTOR_CONTENT_KEY = "content:instructor:{0}";
        private const string COURSE_CONTENT_KEY = "content:course:{0}";
        private const string CACHE_STATS_KEY = "cache:stats";

        // Cache tags for easier invalidation
        private readonly Dictionary<string, HashSet<string>> _cacheTags = new();
        private readonly object _cacheTagsLock = new object();

        public ContentCachingService(
            IMemoryCache memoryCache,
            IDistributedCache distributedCache,
            ILogger<ContentCachingService> logger)
        {
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        // =====================================================
        // Basic Caching Operations
        // =====================================================

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                // Try memory cache first (fastest)
                if (_memoryCache.TryGetValue(key, out T? memoryValue))
                {
                    _logger.LogDebug("Cache hit in memory for key: {Key}", key);
                    return memoryValue;
                }

                // Try distributed cache
                var distributedValue = await _distributedCache.GetStringAsync(key);
                if (!string.IsNullOrEmpty(distributedValue))
                {
                    var deserializedValue = JsonSerializer.Deserialize<T>(distributedValue, _jsonOptions);

                    // Store in memory cache for faster access
                    _memoryCache.Set(key, deserializedValue, _shortExpiration);

                    _logger.LogDebug("Cache hit in distributed cache for key: {Key}", key);
                    return deserializedValue;
                }

                _logger.LogDebug("Cache miss for key: {Key}", key);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache value for key: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var exp = expiration ?? _defaultExpiration;
                var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);

                // Set in both memory and distributed cache
                _memoryCache.Set(key, value, exp);
                await _distributedCache.SetStringAsync(key, serializedValue, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = exp
                });

                // Track cache tags
                TrackCacheEntry(key, ExtractTagsFromKey(key));

                _logger.LogDebug("Cache set for key: {Key} with expiration: {Expiration}", key, exp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                await _distributedCache.RemoveAsync(key);

                RemoveCacheEntry(key);

                _logger.LogDebug("Cache removed for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
            }
        }

        public async Task RemovePatternAsync(string pattern)
        {
            try
            {
                var keysToRemove = GetCacheKeysByPattern(pattern);

                foreach (var key in keysToRemove)
                {
                    await RemoveAsync(key);
                }

                _logger.LogDebug("Cache removed for pattern: {Pattern}, keys removed: {Count}", pattern, keysToRemove.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache values for pattern: {Pattern}", pattern);
            }
        }

        // =====================================================
        // Content-Specific Caching Methods
        // =====================================================

        public async Task<IEnumerable<ContentSummaryDto>?> GetSectionContentsAsync(int sectionId, int userId)
        {
            var key = string.Format(SECTION_CONTENTS_KEY, sectionId, userId);
            return await GetAsync<IEnumerable<ContentSummaryDto>>(key);
        }

        public async Task SetSectionContentsAsync(int sectionId, int userId, IEnumerable<ContentSummaryDto> contents)
        {
            var key = string.Format(SECTION_CONTENTS_KEY, sectionId, userId);
            await SetAsync(key, contents, _defaultExpiration);
        }

        public async Task InvalidateSectionContentsAsync(int sectionId)
        {
            var pattern = string.Format(SECTION_CONTENTS_KEY, sectionId, "*");
            await RemovePatternAsync(pattern);

            _logger.LogInformation("Invalidated section contents cache for section: {SectionId}", sectionId);
        }

        public async Task<ContentDetailsDto?> GetContentDetailsAsync(int contentId)
        {
            var key = string.Format(CONTENT_DETAILS_KEY, contentId);
            return await GetAsync<ContentDetailsDto>(key);
        }

        public async Task SetContentDetailsAsync(int contentId, ContentDetailsDto details)
        {
            var key = string.Format(CONTENT_DETAILS_KEY, contentId);
            await SetAsync(key, details, _longExpiration);
        }

        public async Task InvalidateContentDetailsAsync(int contentId)
        {
            var key = string.Format(CONTENT_DETAILS_KEY, contentId);
            await RemoveAsync(key);

            _logger.LogInformation("Invalidated content details cache for content: {ContentId}", contentId);
        }

        public async Task<ContentStatsDto?> GetContentStatsAsync(int contentId)
        {
            var key = string.Format(CONTENT_STATS_KEY, contentId);
            return await GetAsync<ContentStatsDto>(key);
        }

        public async Task SetContentStatsAsync(int contentId, ContentStatsDto stats)
        {
            var key = string.Format(CONTENT_STATS_KEY, contentId);
            await SetAsync(key, stats, _statsExpiration);
        }

        public async Task InvalidateContentStatsAsync(int contentId)
        {
            var key = string.Format(CONTENT_STATS_KEY, contentId);
            await RemoveAsync(key);

            _logger.LogInformation("Invalidated content stats cache for content: {ContentId}", contentId);
        }

        // =====================================================
        // Bulk Invalidation Methods
        // =====================================================

        public async Task InvalidateUserContentCacheAsync(int userId)
        {
            var patterns = new[]
            {
                string.Format(USER_CONTENT_KEY, userId),
                string.Format(SECTION_CONTENTS_KEY, "*", userId)
            };

            foreach (var pattern in patterns)
            {
                await RemovePatternAsync(pattern);
            }

            _logger.LogInformation("Invalidated user content cache for user: {UserId}", userId);
        }

        public async Task InvalidateInstructorContentCacheAsync(int instructorId)
        {
            var pattern = string.Format(INSTRUCTOR_CONTENT_KEY, instructorId);
            await RemovePatternAsync(pattern);

            _logger.LogInformation("Invalidated instructor content cache for instructor: {InstructorId}", instructorId);
        }

        public async Task InvalidateCourseContentCacheAsync(int courseId)
        {
            var pattern = string.Format(COURSE_CONTENT_KEY, courseId);
            await RemovePatternAsync(pattern);

            _logger.LogInformation("Invalidated course content cache for course: {CourseId}", courseId);
        }

        // =====================================================
        // Cache Warming and Preloading
        // =====================================================

        public async Task WarmupSectionCacheAsync(int sectionId, int userId)
        {
            try
            {
                // This would typically involve calling the actual service to populate cache
                // For now, we'll just log the warming attempt
                _logger.LogInformation("Cache warmup requested for section: {SectionId}, user: {UserId}", sectionId, userId);

                // In a real implementation, you would:
                // 1. Check if cache is empty
                // 2. Call the content service to get data
                // 3. Store in cache

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error warming up section cache for section: {SectionId}, user: {UserId}", sectionId, userId);
            }
        }

        public async Task WarmupUserContentCacheAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Cache warmup requested for user content: {UserId}", userId);

                // Implementation would involve preloading commonly accessed user content
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error warming up user content cache for user: {UserId}", userId);
            }
        }

        public async Task WarmupInstructorContentCacheAsync(int instructorId)
        {
            try
            {
                _logger.LogInformation("Cache warmup requested for instructor content: {InstructorId}", instructorId);

                // Implementation would involve preloading instructor's content data
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error warming up instructor content cache for instructor: {InstructorId}", instructorId);
            }
        }

        // =====================================================
        // Cache Statistics and Monitoring
        // =====================================================

        public async Task<CacheStatisticsDto> GetCacheStatisticsAsync()
        {
            try
            {
                // Try to get cached statistics first
                var cachedStats = await GetAsync<CacheStatisticsDto>(CACHE_STATS_KEY);
                if (cachedStats != null)
                {
                    return cachedStats;
                }

                // Calculate fresh statistics
                var stats = new CacheStatisticsDto
                {
                    GeneratedAt = DateTime.UtcNow,
                    TotalEntries = GetTotalCacheEntries(),
                    MemoryCacheEntries = GetMemoryCacheEntries(),
                    EntryCategories = GetCacheEntryCategories(),
                    EstimatedMemoryUsage = EstimateMemoryUsage(),
                    CacheHitRate = CalculateCacheHitRate(),
                    AverageExpirationTime = CalculateAverageExpirationTime()
                };

                // Cache the statistics for a short time
                await SetAsync(CACHE_STATS_KEY, stats, TimeSpan.FromMinutes(1));

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating cache statistics");
                return new CacheStatisticsDto
                {
                    GeneratedAt = DateTime.UtcNow,
                    TotalEntries = 0
                };
            }
        }

        public async Task<IEnumerable<CacheEntryInfoDto>> GetCacheEntriesAsync(string? pattern = null)
        {
            try
            {
                var entries = new List<CacheEntryInfoDto>();

                lock (_cacheTagsLock)
                {
                    foreach (var kvp in _cacheTags)
                    {
                        var key = kvp.Key;
                        if (pattern == null || key.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                        {
                            entries.Add(new CacheEntryInfoDto
                            {
                                Key = key,
                                Tags = kvp.Value.ToList(),
                                Category = DetermineCacheCategory(key),
                                EstimatedSize = EstimateEntrySize(key),
                                LastAccessed = DateTime.UtcNow // This would be tracked in a real implementation
                            });
                        }
                    }
                }

                return entries.OrderBy(e => e.Category).ThenBy(e => e.Key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache entries");
                return Enumerable.Empty<CacheEntryInfoDto>();
            }
        }

        public async Task ClearAllContentCacheAsync()
        {
            try
            {
                var patterns = new[]
                {
                    "content:*",
                    "section:*",
                    "instructor:*",
                    "course:*"
                };

                foreach (var pattern in patterns)
                {
                    await RemovePatternAsync(pattern);
                }

                // Clear cache tracking
                lock (_cacheTagsLock)
                {
                    _cacheTags.Clear();
                }

                _logger.LogInformation("Cleared all content cache");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing all content cache");
            }
        }

        // =====================================================
        // Helper Methods
        // =====================================================

        private List<string> ExtractTagsFromKey(string key)
        {
            var tags = new List<string>();

            if (key.Contains("content:"))
                tags.Add("content");
            if (key.Contains("section:"))
                tags.Add("section");
            if (key.Contains("user:"))
                tags.Add("user");
            if (key.Contains("instructor:"))
                tags.Add("instructor");
            if (key.Contains("course:"))
                tags.Add("course");
            if (key.Contains("stats:"))
                tags.Add("stats");

            return tags;
        }

        private void TrackCacheEntry(string key, List<string> tags)
        {
            lock (_cacheTagsLock)
            {
                _cacheTags[key] = new HashSet<string>(tags);
            }
        }

        private void RemoveCacheEntry(string key)
        {
            lock (_cacheTagsLock)
            {
                _cacheTags.Remove(key);
            }
        }

        private List<string> GetCacheKeysByPattern(string pattern)
        {
            var keys = new List<string>();

            lock (_cacheTagsLock)
            {
                var regex = new System.Text.RegularExpressions.Regex(
                    pattern.Replace("*", ".*"),
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                foreach (var key in _cacheTags.Keys)
                {
                    if (regex.IsMatch(key))
                    {
                        keys.Add(key);
                    }
                }
            }

            return keys;
        }

        private int GetTotalCacheEntries()
        {
            lock (_cacheTagsLock)
            {
                return _cacheTags.Count;
            }
        }

        private int GetMemoryCacheEntries()
        {
            // This is a simplified implementation
            // In reality, you'd need to access internal memory cache statistics
            return GetTotalCacheEntries();
        }

        private Dictionary<string, int> GetCacheEntryCategories()
        {
            var categories = new Dictionary<string, int>();

            lock (_cacheTagsLock)
            {
                foreach (var kvp in _cacheTags)
                {
                    var category = DetermineCacheCategory(kvp.Key);
                    categories[category] = categories.GetValueOrDefault(category, 0) + 1;
                }
            }

            return categories;
        }

        private string DetermineCacheCategory(string key)
        {
            if (key.Contains("content:details"))
                return "Content Details";
            if (key.Contains("content:stats"))
                return "Content Statistics";
            if (key.Contains("section:"))
                return "Section Content";
            if (key.Contains("instructor:"))
                return "Instructor Data";
            if (key.Contains("course:"))
                return "Course Data";

            return "Other";
        }

        private long EstimateMemoryUsage()
        {
            // Simplified estimation - in reality you'd calculate based on actual object sizes
            return GetTotalCacheEntries() * 1024; // Assume 1KB per entry on average
        }

        private long EstimateEntrySize(string key)
        {
            // Simplified estimation based on key type
            if (key.Contains("stats"))
                return 512; // Stats are typically smaller
            if (key.Contains("details"))
                return 2048; // Details are typically larger

            return 1024; // Default size
        }

        private decimal CalculateCacheHitRate()
        {
            // This would require tracking hits and misses in a real implementation
            return 85.0m; // Placeholder value
        }

        private TimeSpan CalculateAverageExpirationTime()
        {
            // This would be calculated based on actual cache entry TTLs
            return _defaultExpiration;
        }
    }

    
}