using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UrlShortner.Models;

namespace UrlShortner.Helpers
{
    public class JwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(
            User user)
        {
            var claims =
                new List<Claim>
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        user.Id.ToString()
                    ),

                    new Claim(
                        ClaimTypes.Email,
                        user.Email
                    ),

                    new Claim(
                        "Plan",
                        user.Plan.ToString()
                    )
                };

            var key =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        _configuration[
                            "Jwt:Key"
                        ]!
                    )
                );

            var credentials =
                new SigningCredentials(
                    key,
                    SecurityAlgorithms
                        .HmacSha256
                );

            var token =
                new JwtSecurityToken(
                    issuer:
                        _configuration[
                            "Jwt:Issuer"
                        ],

                    audience:
                        _configuration[
                            "Jwt:Audience"
                        ],

                    claims: claims,

                    expires:
                        DateTime.UtcNow
                            .AddDays(7),

                    signingCredentials:
                        credentials
                );

            return new
                JwtSecurityTokenHandler()
                .WriteToken(
                    token
                );
        }
    }
}