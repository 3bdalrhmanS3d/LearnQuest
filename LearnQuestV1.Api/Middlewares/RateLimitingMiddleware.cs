using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace LearnQuestV1.Api.Middlewares
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RateLimitingMiddleware> _logger;

        // Rate limiting rules
        private readonly Dictionary<string, RateLimitRule> _rateLimitRules = new()
        {
            { "/api/auth/signin", new RateLimitRule(5, TimeSpan.FromMinutes(15)) }, // 5 attempts per 15 min
            { "/api/auth/signup", new RateLimitRule(3, TimeSpan.FromMinutes(10)) }, // 3 attempts per 10 min
            { "/api/auth/verify-account", new RateLimitRule(5, TimeSpan.FromMinutes(5)) }, // 5 attempts per 5 min
            { "/api/auth/resend-verification-code", new RateLimitRule(3, TimeSpan.FromMinutes(5)) }, // 3 per 5 min
            { "/api/auth/forget-password", new RateLimitRule(3, TimeSpan.FromMinutes(15)) }, // 3 per 15 min
            { "/api/auth/reset-password", new RateLimitRule(3, TimeSpan.FromMinutes(15)) } // 3 per 15 min
        };

        public RateLimitingMiddleware(
            RequestDelegate next,
            IMemoryCache cache,
            ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.Request.Path.Value?.ToLower();
            var method = context.Request.Method;

            // Only apply rate limiting to POST requests on auth endpoints
            if (method == "POST" && endpoint != null && _rateLimitRules.ContainsKey(endpoint))
            {
                var clientIp = GetClientIpAddress(context);
                var rule = _rateLimitRules[endpoint];

                if (!await IsRequestAllowedAsync(clientIp, endpoint, rule))
                {
                    await HandleRateLimitExceededAsync(context, rule);
                    return;
                }
            }

            await _next(context);
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
                _logger.LogWarning("Rate limit exceeded for IP {ClientIp} on endpoint {Endpoint}. Attempts: {Attempts}",
                    clientIp, endpoint, attempts.Count);
                return false;
            }

            // Add current attempt
            attempts.Add(DateTime.UtcNow);
            _cache.Set(key, attempts, rule.TimeWindow);

            return true;
        }

        private async Task HandleRateLimitExceededAsync(HttpContext context, RateLimitRule rule)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Rate limit exceeded",
                message = $"Too many requests. Try again in {rule.TimeWindow.TotalMinutes} minutes.",
                retryAfter = rule.TimeWindow.TotalSeconds
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }

        private string GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded IP first (for load balancers/proxies)
            var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                return xForwardedFor.Split(',')[0].Trim();
            }

            var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xRealIp))
            {
                return xRealIp;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
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