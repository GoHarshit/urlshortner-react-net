using StackExchange.Redis;
using System.Security.Claims;

namespace UrlShortner.Middleware
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly IConnectionMultiplexer
            _redis;

        public RateLimitMiddleware(
            RequestDelegate next,
            IConnectionMultiplexer redis)
        {
            _next = next;

            _redis = redis;
        }

        public async Task InvokeAsync(
            HttpContext context)
        {
            // ==========================
            // APPLY ONLY TO SHORTEN API
            // ==========================

            if (
                context.Request.Path
                    .StartsWithSegments(
                        "/api/url/shorten"
                    )
                &&
                context.Request.Method
                    == "POST"
            )
            {
                var userId =
                    context.User
                        .FindFirst(
                            ClaimTypes
                                .NameIdentifier
                        )?.Value;

                var plan =
                    context.User
                        .FindFirst(
                            "Plan"
                        )?.Value
                    ?? "Free";

                // ==========================
                // GUEST SAFETY
                // ==========================

                if (string.IsNullOrEmpty(
                    userId))
                {
                    context.Response.StatusCode =
                        401;

                    await context.Response
                        .WriteAsync(
                            "Unauthorized"
                        );

                    return;
                }

                // ==========================
                // LIMITS
                // ==========================

                int limit =
                    plan == "Premium"
                    ? 100
                    : 10;

                // ==========================
                // REDIS KEY
                // ==========================

                var redisDb =
                    _redis.GetDatabase();

                var key =
                    $"rate_limit:{userId}";

                // ==========================
                // CURRENT COUNT
                // ==========================

                var currentCount =
                    await redisDb
                        .StringIncrementAsync(
                            key
                        );

                // ==========================
                // FIRST REQUEST
                // ==========================

                if (currentCount == 1)
                {
                    await redisDb
                        .KeyExpireAsync(
                            key,
                            TimeSpan.FromMinutes(
                                1
                            )
                        );
                }

                // ==========================
                // LIMIT EXCEEDED
                // ==========================

                if (currentCount > limit)
                {
                    context.Response
                        .StatusCode = 429;

                    await context.Response
                        .WriteAsync(
                            $"Rate limit exceeded. Max {limit} requests per minute."
                        );

                    return;
                }
            }

            // ==========================
            // CONTINUE PIPELINE
            // ==========================

            await _next(context);
        }
    }
}