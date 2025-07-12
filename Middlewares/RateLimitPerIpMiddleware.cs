using Microsoft.Extensions.Caching.Memory;

namespace project_graduation.Middlewares
{
    public class RateLimitPerIpMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        // Rate limiting settings
        private static readonly TimeSpan TimeWindow = TimeSpan.FromSeconds(3);
        private static readonly TimeSpan PerRequestDelay = TimeSpan.FromSeconds(3);
        private const int MaxRequestsPerWindow = 100;

        // Excluded paths from rate limiting
        private readonly List<string> _excludedFromDelayPaths = new()
        {
            "/api/auth/login",
            "/api/auth/register",
            "/api/passwordcheck"
        };

        // Constructor to initialize dependencies
        public RateLimitPerIpMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        // Middleware logic to handle rate limiting
        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var path = context.Request.Path.Value?.ToLower().TrimEnd('/');

            // Skip processing if IP or path is missing
            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(path))
            {
                await _next(context);
                return;
            }

            // 1. Apply 60-second delay if not in excluded paths
            if (!_excludedFromDelayPaths.Contains(path))
            {
                var delayKey = $"RateLimit:{ip}:{path}";
                if (_cache.TryGetValue(delayKey, out _))
                {
                    context.Response.StatusCode = 429;
                    await context.Response.WriteAsync("Please wait 60 seconds and try again.");
                    return;
                }

                _cache.Set(delayKey, true, PerRequestDelay);
            }

            // 2. General rate limit: max 10 requests per minute per IP
            var countKey = $"RateCount:{ip}";
            int currentCount = _cache.Get<int>(countKey);

            if (currentCount >= MaxRequestsPerWindow)
            {
                context.Response.StatusCode = 429;
                // Maximum 10 requests per minute
                await context.Response.WriteAsync("API Rate limit exceeded.");
                return;
            }

            _cache.Set(countKey, currentCount + 1, TimeWindow);
            await _next(context);
        }
    }

    // Extension method to add middleware to pipeline
    public static class RateLimitPerIpMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimitPerIp(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitPerIpMiddleware>();
        }
    }
}
