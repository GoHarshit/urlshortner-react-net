using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UrlShortner.Data;
using UrlShortner.DTOs;
using UrlShortner.Helpers;
using UrlShortner.Models;

namespace UrlShortner.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtTokenGenerator _jwt;

        public AuthController(
            AppDbContext context,
            JwtTokenGenerator jwt)
        {
            _context = context;
            _jwt = jwt;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            RegisterRequest request)
        {
            // Check if user already exists
            var exists = await _context.Users
                .AnyAsync(
                    x => x.Email == request.Email);

            if (exists)
            {
                return BadRequest(
                    "User already exists");
            }

            // Create user
            var user = new User
            {
                Email = request.Email,

                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        request.Password),

                Plan = UserPlan.Free
            };

            // Save user
            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return Ok(
                "User created successfully");
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            LoginRequest request)
        {
            // Find user
            var user = await _context.Users
                .FirstOrDefaultAsync(
                    x => x.Email == request.Email);

            if (user == null)
            {
                return Unauthorized(
                    "Invalid credentials");
            }

            // Verify password
            var isValid =
                BCrypt.Net.BCrypt.Verify(
                    request.Password,
                    user.PasswordHash);

            if (!isValid)
            {
                return Unauthorized(
                    "Invalid credentials");
            }

            // Generate JWT token
            var token =
                _jwt.GenerateToken(user);

            // Return token
            return Ok(new
            {
                Token = token
            });
        }

        // POST: api/auth/upgrade

        [Authorize]
        [HttpPost("upgrade")]
        public async Task<IActionResult>
        UpgradePlan()
        {
            var userId =
                int.Parse(
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier)!);

            var user =
                await _context.Users
                    .FirstOrDefaultAsync(
                        x => x.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            user.Plan =
                UserPlan.Premium;

            await _context.SaveChangesAsync();

            return Ok(
                "Upgraded to Premium");
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult>
        GetProfile()
        {
            var userId =
                int.Parse(
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier
                    )!
                );

            var user =
                await _context.Users
                    .Include(x => x.Urls)
                    .FirstOrDefaultAsync(
                        x => x.Id == userId
                    );

            if (user == null)
            {
                return NotFound();
            }

            var response =
                new ProfileResponse
                {
                    Email =
                        user.Email,

                    Plan =
                        user.Plan.ToString(),

                    CreatedAt =
                        user.CreatedAt,

                    TotalUrls =
                        user.Urls.Count,

                    TotalClicks =
                        user.Urls.Sum(
                            x => x.ClickCount
                        )
                };

            return Ok(response);
        }
    }
}