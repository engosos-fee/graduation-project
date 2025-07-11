using Microsoft.Extensions.Caching.Memory;

namespace project_graduation.Middlewares
{
    public class RateLimitPerIpMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        // Time window for counting requests (e.g., 1 minute)
        private static readonly TimeSpan TimeWindow = TimeSpan.FromMinutes(1);

        // Delay between requests to the same endpoint (e.g., 60 seconds)
        private static readonly TimeSpan PerRequestDelay = TimeSpan.FromSeconds(60);

        // Max number of requests allowed in TimeWindow
        private const int MaxRequestsPerWindow = 10;

        // Paths excluded from the 60-second delay (but still counted in rate limit)
        private readonly List<string> _excludedFromDelayPaths = new()
        {
            "/api/auth/login",
            "/api/auth/register",
            "/api/auth/forgot-password"
        };

        public RateLimitPerIpMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString();
            var path = context.Request.Path.Value?.ToLower();

            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(path))
            {
                await _next(context);
                return;
            }

            // 1. Apply 60-second delay per endpoint (unless excluded)
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

            // 2. Apply general rate limit: max 10 requests per minute
            var countKey = $"RateCount:{ip}";
            int requestCount = _cache.Get<int>(countKey);

            if (requestCount >= MaxRequestsPerWindow)
            {
                context.Response.StatusCode = 429;
                //--> Maximum 10 requests per minute.
                await context.Response.WriteAsync("Rate limit exceeded");
                return;
            }

            _cache.Set(countKey, requestCount + 1, TimeWindow);

            await _next(context);
        }
    }

    // Extension method for cleaner registration in Program.cs
    public static class RateLimitPerIpMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimitPerIp(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitPerIpMiddleware>();
        }
    }
}
