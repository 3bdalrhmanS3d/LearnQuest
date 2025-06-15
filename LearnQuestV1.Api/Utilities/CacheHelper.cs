using Microsoft.Extensions.Caching.Memory;

namespace LearnQuestV1.Api.Utilities
{
    public static class CacheHelper
    {
        public static void SetWithExpiration<T>(this IMemoryCache cache, string key, T value, TimeSpan expiration)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };
            cache.Set(key, value, options);
        }

        public static void SetWithOptions<T>(this IMemoryCache cache, string key, T value, MemoryCacheEntryOptions options)
        {
            cache.Set(key, value, options);
        }
    }
}
