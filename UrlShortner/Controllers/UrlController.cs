using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Security.Claims;
using UrlShortner.Data;
using UrlShortner.DTOs;
using UrlShortner.Helpers;
using UrlShortner.Models;

namespace UrlShortner.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConnectionMultiplexer _redis;

        public UrlController(
            AppDbContext context,
            IConnectionMultiplexer redis)
        {
            _context = context;
            _redis = redis;
        }

        // =====================================
        // CREATE SHORT URL
        // =====================================

        [Authorize]
        [HttpPost("shorten")]
        public async Task<IActionResult> ShortenUrl(
            [FromBody] CreateUrlRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.OriginalUrl))
                {
                    return BadRequest("URL is required");
                }

                var userPlan =
                    User.FindFirstValue("Plan");

                var userId =
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier);

                string shortCode;

                // =====================================
                // CUSTOM ALIAS (Premium)
                // =====================================

                if (!string.IsNullOrWhiteSpace(
                    request.CustomAlias))
                {
                    if (userPlan !=
                        UserPlan.Premium.ToString())
                    {
                        return StatusCode(
                            403,
                            "Custom aliases are premium only");
                    }

                    if (request.CustomAlias.Length < 3)
                    {
                        return BadRequest(
                            "Alias must be at least 3 characters");
                    }

                    if (request.CustomAlias.Length > 30)
                    {
                        return BadRequest(
                            "Alias too long");
                    }

                    if (!System.Text.RegularExpressions.Regex
                        .IsMatch(
                            request.CustomAlias,
                            "^[a-zA-Z0-9_-]+$"))
                    {
                        return BadRequest(
                            "Alias contains invalid characters");
                    }

                    var alias =
                        request.CustomAlias
                            .Trim()
                            .ToLower();

                    var aliasExists =
                        await _context.Urls
                            .AnyAsync(x =>
                                x.ShortCode == alias);

                    if (aliasExists)
                    {
                        return BadRequest(
                            "Alias already exists");
                    }

                    shortCode = alias;
                }
                else
                {
                    shortCode = "temp";
                }

                // =====================================
                // CREATE ENTITY
                // =====================================

                var url = new Url
                {
                    OriginalUrl = request.OriginalUrl,
                    ShortCode = shortCode,
                    CreatedAt = DateTime.UtcNow,
                    ClickCount = 0,
                    ExpiresAt = request.ExpiresAt.HasValue
                        ? DateTime.SpecifyKind(
                            request.ExpiresAt.Value,
                            DateTimeKind.Utc)
                        : null,
                    UserId = int.Parse(userId!)
                };

                _context.Urls.Add(url);

                await _context.SaveChangesAsync();

                // =====================================
                // GENERATE BASE62
                // =====================================

                if (shortCode == "temp")
                {
                    url.ShortCode =
                        Base62Encoder.Encode(url.Id);

                    await _context.SaveChangesAsync();
                }

                // =====================================
                // RESPONSE
                // =====================================

                var response =
                    new CreateUrlResponse
                    {
                        OriginalUrl = url.OriginalUrl,
                        ShortCode = url.ShortCode,
                        ShortUrl =
                            $"{Request.Scheme}://" +
                            $"{Request.Host}/" +
                            $"{url.ShortCode}"
                    };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Message =
                            "An error occurred while creating URL",

                        Error = ex.Message,

                        InnerError =
                            ex.InnerException?.Message,

                        StackTrace =
                            ex.StackTrace
                    });
            }
        }

        // =====================================
        // REDIRECT
        // =====================================

        [AllowAnonymous]
        [HttpGet("{shortCode}")]
        public async Task<IActionResult>
            RedirectToOriginal(string shortCode)
        {
            try
            {
                var redisDb =
                    _redis.GetDatabase();

                var cacheKey =
                    $"url:{shortCode}";

                string? cachedUrl =
                    await redisDb.StringGetAsync(
                        cacheKey);

                Url? url;

                // =====================================
                // CACHE HIT
                // =====================================

                if (!string.IsNullOrWhiteSpace(cachedUrl))
                {
                    url =
                        await _context.Urls
                            .FirstOrDefaultAsync(
                                x =>
                                    x.ShortCode ==
                                    shortCode);
                }
                else
                {
                    // =====================================
                    // CACHE MISS
                    // =====================================

                    url =
                        await _context.Urls
                            .FirstOrDefaultAsync(
                                x =>
                                    x.ShortCode ==
                                    shortCode);

                    if (url == null)
                    {
                        return NotFound(
                            "Short URL not found");
                    }

                    await redisDb.StringSetAsync(
                        cacheKey,
                        url.OriginalUrl,
                        TimeSpan.FromHours(1));
                }

                if (url == null)
                {
                    return NotFound(
                        "Short URL not found");
                }

                // =====================================
                // EXPIRY CHECK
                // =====================================

                if (url.ExpiresAt.HasValue &&
                    url.ExpiresAt < DateTime.UtcNow)
                {
                    return BadRequest(
                        "URL expired");
                }

                // =====================================
                // ANALYTICS
                // =====================================

                url.ClickCount++;

                await _context.SaveChangesAsync();

                return Redirect(url.OriginalUrl);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Message =
                            "An error occurred during redirection",

                        Error = ex.Message,

                        StackTrace = ex.StackTrace
                    });
            }
        }

        // =====================================
        // ANALYTICS
        // =====================================

        [Authorize]
        [HttpGet("analytics/{shortCode}")]
        public async Task<IActionResult>
            GetAnalytics(string shortCode)
        {
            try
            {
                var url =
                    await _context.Urls
                        .FirstOrDefaultAsync(
                            x =>
                                x.ShortCode ==
                                shortCode);

                if (url == null)
                {
                    return NotFound(
                        "Short URL not found");
                }

                var response =
                    new UrlAnalyticsResponse
                    {
                        OriginalUrl = url.OriginalUrl,
                        ShortCode = url.ShortCode,
                        ClickCount = url.ClickCount,
                        CreatedAt = url.CreatedAt,
                        ExpiresAt = url.ExpiresAt
                    };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Message =
                            "An error occurred while fetching analytics",

                        Error = ex.Message,

                        StackTrace = ex.StackTrace
                    });
            }
        }

        // =====================================
        // GET MY URLS
        // =====================================

        [Authorize]
        [HttpGet("myurls")]
        public async Task<IActionResult>
            GetMyUrls(
                int page = 1,
                int pageSize = 10)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (page < 1)
                page = 1;

            if (pageSize < 1)
                pageSize = 10;

            var userIdInt =
                int.Parse(userId);

            var totalCount =
                await _context.Urls
                    .CountAsync(
                        x =>
                            x.UserId == userIdInt);

            var urls =
                await _context.Urls
                    .Where(
                        x =>
                            x.UserId == userIdInt)
                    .OrderByDescending(
                        x => x.CreatedAt)
                    .Skip(
                        (page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new
                    {
                        x.Id,
                        x.ShortCode,
                        x.OriginalUrl,
                        x.ClickCount,
                        x.CreatedAt,
                        x.ExpiresAt
                    })
                    .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,

                TotalPages =
                    (int)Math.Ceiling(
                        totalCount /
                        (double)pageSize),

                CurrentPage = page,

                PageSize = pageSize,

                Data = urls
            });
        }

        // =====================================
        // DELETE URL
        // =====================================

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult>
            DeleteUrl(long id)
        {
            var userId =
                int.Parse(
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier)!);

            var url =
                await _context.Urls
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id == id &&
                            x.UserId == userId);

            if (url == null)
            {
                return NotFound();
            }

            _context.Urls.Remove(url);

            await _context.SaveChangesAsync();

            return Ok(
                "URL deleted successfully");
        }

        // =====================================
        // UPDATE URL
        // =====================================

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult>
            UpdateUrl(
                long id,
                [FromBody]
                UpdateUrlRequest request)
        {
            var userId =
                int.Parse(
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier)!);

            var userPlan =
                User.FindFirstValue("Plan");

            var url =
                await _context.Urls
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id == id &&
                            x.UserId == userId);

            if (url == null)
            {
                return NotFound();
            }

            url.OriginalUrl =
                request.OriginalUrl;

            url.ExpiresAt =
                request.ExpiresAt.HasValue
                    ? DateTime.SpecifyKind(
                        request.ExpiresAt.Value,
                        DateTimeKind.Utc)
                    : null;

            if (!string.IsNullOrWhiteSpace(
                request.CustomAlias))
            {
                if (userPlan !=
                    UserPlan.Premium.ToString())
                {
                    return StatusCode(
                        403,
                        "Alias editing is premium only");
                }

                var alias =
                    request.CustomAlias
                        .Trim()
                        .ToLower();

                var exists =
                    await _context.Urls
                        .AnyAsync(
                            x =>
                                x.ShortCode ==
                                    alias &&
                                x.Id != id);

                if (exists)
                {
                    return BadRequest(
                        "Alias already exists");
                }

                url.ShortCode = alias;
            }

            await _context.SaveChangesAsync();

            return Ok(
                "URL updated successfully");
        }
    }
}