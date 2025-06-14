using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace LearnQuestV1.Api.Middlewares
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly IConfiguration _configuration;

        // Rate limiting rules (fallback if config is not available)
        private readonly Dictionary<string, RateLimitRule> _defaultRateLimitRules = new()
        {
            { "/api/auth/signin", new RateLimitRule(5, TimeSpan.FromMinutes(15)) },
            { "/api/auth/signup", new RateLimitRule(3, TimeSpan.FromMinutes(10)) },
            { "/api/auth/verify-account", new RateLimitRule(5, TimeSpan.FromMinutes(5)) },
            { "/api/auth/resend-verification-code", new RateLimitRule(3, TimeSpan.FromMinutes(5)) },
            { "/api/auth/forget-password", new RateLimitRule(3, TimeSpan.FromMinutes(15)) },
            { "/api/auth/reset-password", new RateLimitRule(3, TimeSpan.FromMinutes(15)) }
        };

        public RateLimitingMiddleware(
            RequestDelegate next,
            IMemoryCache cache,
            ILogger<RateLimitingMiddleware> logger,
            IConfiguration configuration)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.Request.Path.Value?.ToLower();
            var method = context.Request.Method;

            // Only apply rate limiting to POST requests on auth endpoints
            if (method == "POST" && endpoint != null && ShouldApplyRateLimit(endpoint))
            {
                var clientIp = GetClientIpAddress(context);
                var rule = GetRateLimitRule(endpoint);

                if (!await IsRequestAllowedAsync(clientIp, endpoint, rule))
                {
                    await HandleRateLimitExceededAsync(context, rule);
                    return;
                }
            }

            await _next(context);
        }

        private bool ShouldApplyRateLimit(string endpoint)
        {
            // Check both default rules and config-based rules
            if (_defaultRateLimitRules.ContainsKey(endpoint))
                return true;

            // Check configuration for additional endpoints
            var configRules = _configuration.GetSection("RateLimiting:AuthEndpoints").GetChildren();
            foreach (var configRule in configRules)
            {
                var configEndpoint = $"/api/auth/{configRule.Key.ToLower()}";
                if (configEndpoint == endpoint)
                    return true;
            }

            return false;
        }

        private RateLimitRule GetRateLimitRule(string endpoint)
        {
            // Try to get from configuration first
            var endpointKey = endpoint.Replace("/api/auth/", "").Replace("-", "");
            var configSection = _configuration.GetSection($"RateLimiting:AuthEndpoints:{endpointKey}");

            if (configSection.Exists())
            {
                var maxRequests = configSection.GetValue<int>("MaxRequests", 5);
                var windowInMinutes = configSection.GetValue<int>("WindowInMinutes", 15);
                return new RateLimitRule(maxRequests, TimeSpan.FromMinutes(windowInMinutes));
            }

            // Fallback to default rules
            return _defaultRateLimitRules.TryGetValue(endpoint, out var rule)
                ? rule
                : new RateLimitRule(5, TimeSpan.FromMinutes(15)); // Default fallback
        }

        private async Task<bool> IsRequestAllowedAsync(string clientIp, string endpoint, RateLimitRule rule)
        {
            var key = $"rate_limit:{clientIp}:{endpoint}";
            var attempts = _cache.Get<List<DateTime>>(key) ?? new List<DateTime>();

            // Remove expired attempts
            var cutoff = DateTime.UtcNow.Subtract(rule.TimeWindow);
            attempts.RemoveAll(attempt => attempt < cutoff);

            // Check if limit exceeded
            if (attempts.Count >= rule.MaxRequests)
            {
                _logger.LogWarning("Rate limit exceeded for IP {ClientIp} on endpoint {Endpoint}. Attempts: {Attempts}/{MaxRequests}",
                    clientIp, endpoint, attempts.Count, rule.MaxRequests);
                return false;
            }

            // Add current attempt
            attempts.Add(DateTime.UtcNow);

            // Set cache with sliding expiration
            var cacheOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = rule.TimeWindow,
                Priority = CacheItemPriority.Low
            };

            _cache.Set(key, attempts, cacheOptions);

            return true;
        }

        private async Task HandleRateLimitExceededAsync(HttpContext context, RateLimitRule rule)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";

            // Add rate limit headers
            context.Response.Headers.Add("Retry-After", ((int)rule.TimeWindow.TotalSeconds).ToString());
            context.Response.Headers.Add("X-RateLimit-Limit", rule.MaxRequests.ToString());
            context.Response.Headers.Add("X-RateLimit-Window", rule.TimeWindow.TotalMinutes.ToString());

            var response = new
            {
                success = false,
                code = "RATE_LIMIT_EXCEEDED",
                message = $"Too many requests. Try again in {rule.TimeWindow.TotalMinutes} minutes.",
                retryAfter = rule.TimeWindow.TotalSeconds,
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }

        private string GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded IP first (for load balancers/proxies)
            var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                // Take the first IP if there are multiple
                var firstIp = xForwardedFor.Split(',')[0].Trim();
                if (IsValidIpAddress(firstIp))
                    return firstIp;
            }

            var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xRealIp) && IsValidIpAddress(xRealIp))
            {
                return xRealIp;
            }

            var cfConnectingIp = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(cfConnectingIp) && IsValidIpAddress(cfConnectingIp))
            {
                return cfConnectingIp;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private static bool IsValidIpAddress(string ip)
        {
            return System.Net.IPAddress.TryParse(ip, out _);
        }
    }

    public class RateLimitRule
    {
        public int MaxRequests { get; }
        public TimeSpan TimeWindow { get; }

        public RateLimitRule(int maxRequests, TimeSpan timeWindow)
        {
            MaxRequests = maxRequests;
            TimeWindow = timeWindow;
        }
    }

    // Extension method for easy registration
    public static class RateLimitingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}