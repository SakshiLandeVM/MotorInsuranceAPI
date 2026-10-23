
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MotorInsuranceAPI.Data;
using MotorInsuranceAPI.Mapping;
using MotorInsuranceAPI.Models;
using MotorInsuranceAPI.Services;


DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Map .env variables to the configuration keys used by AdminSeeder.

builder.Configuration.AddInMemoryCollection(
    new Dictionary<string, string?>
    {
        // Admin account settings
        ["Admin:FullName"] = Environment.GetEnvironmentVariable("ADMIN_FULLNAME"),
        ["Admin:Email"] = Environment.GetEnvironmentVariable("ADMIN_EMAIL"),
        ["Admin:Password"] = Environment.GetEnvironmentVariable("ADMIN_PASSWORD"),

        // Email SMTP settings
        ["Email:SmtpHost"] = Environment.GetEnvironmentVariable("EMAIL_SMTP_HOST"),
        ["Email:SmtpPort"] = Environment.GetEnvironmentVariable("EMAIL_SMTP_PORT"),
        ["Email:Username"] = Environment.GetEnvironmentVariable("EMAIL_USERNAME"),
        ["Email:Password"] = Environment.GetEnvironmentVariable("EMAIL_PASSWORD"),
        ["Email:FromAddress"] = Environment.GetEnvironmentVariable("EMAIL_FROM_ADDRESS"),
        ["Email:FromName"] = Environment.GetEnvironmentVariable("EMAIL_FROM_NAME")
    }
);


// Register API controllers.
builder.Services.AddControllers();

// Register Swagger with JWT Bearer authentication support.
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT access token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register database connection.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Register AutoMapper.
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

// Register application services.
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IImageStorageService, ImageStorageService>();
builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();
// Register password hashing.
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Read JWT configuration.
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key is missing.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT issuer is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT audience is missing.");

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "JWT key must be at least 32 bytes long."
    );
}

// Configure JWT authentication.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

// Register authorization.
builder.Services.AddAuthorization();

builder.Services.AddScoped<AdminSeeder>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var adminSeeder = scope.ServiceProvider
        .GetRequiredService<AdminSeeder>();

    await adminSeeder.CreateAdminAsync();
}


// Enable Swagger in development.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Authentication must run before authorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
