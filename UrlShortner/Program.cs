using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Text;
using UrlShortner.Data;
using UrlShortner.Helpers;
using UrlShortner.Middleware;
using UrlShortner.Models;

var builder = WebApplication.CreateBuilder(args);


// ========================================
// Controllers
// ========================================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();


// ========================================
// Swagger + JWT Configuration
// ========================================

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",

        new OpenApiSecurityScheme
        {
            Name = "Authorization",

            Type = SecuritySchemeType.Http,

            Scheme = "bearer",

            BearerFormat = "JWT",

            In = ParameterLocation.Header,

            Description =
                "Enter token like: Bearer {token}"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,

                            Id = "Bearer"
                        }
                },

                Array.Empty<string>()
            }
        });
});


// ========================================
// PostgreSQL Database
// ========================================

builder.Services.AddDbContext<AppDbContext>(
    options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString(
                "DefaultConnection"
            )
        )
);


// ========================================
// Redis Cache
// ========================================

builder.Services.AddSingleton<IConnectionMultiplexer>(
    sp =>
    {
        var configuration =
            ConfigurationOptions.Parse(
                builder.Configuration[
                    "Redis:ConnectionString"
                ]!
            );

        configuration.AbortOnConnectFail = false;

        return ConnectionMultiplexer.Connect(
            configuration
        );
    }
);


// ========================================
// JWT Authentication
// ========================================

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme
    )
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration[
                        "Jwt:Issuer"
                    ],

                ValidAudience =
                    builder.Configuration[
                        "Jwt:Audience"
                    ],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration[
                                "Jwt:Key"
                            ]!
                        )
                    )
            };
    });

builder.Services.AddAuthorization();


// ========================================
// CORS
// ========================================

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowReact",

        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173"
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


// ========================================
// Dependency Injection
// ========================================

builder.Services.AddScoped<JwtTokenGenerator>();


var app = builder.Build();


// ========================================
// Database Migration + Seed Data
// ========================================

using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

    // Apply pending migrations
    context.Database.Migrate();

    // Seed initial users
    if (!context.Users.Any())
    {
        context.Users.AddRange(

            new User
            {
                Email =
                    "free@test.com",

                PasswordHash =
                    BCrypt.Net.BCrypt
                        .HashPassword(
                            "123456"
                        ),

                Plan =
                    UserPlan.Free
            },

            new User
            {
                Email =
                    "premium@test.com",

                PasswordHash =
                    BCrypt.Net.BCrypt
                        .HashPassword(
                            "123456"
                        ),

                Plan =
                    UserPlan.Premium
            }
        );

        context.SaveChanges();
    }
}


// ========================================
// Middleware Pipeline
// ========================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

// Enable in production later
// app.UseHttpsRedirection();

app.UseCors("AllowReact");

app.UseDeveloperExceptionPage();

app.UseAuthentication();

app.UseMiddleware<RateLimitMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();