
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MotorInsuranceAPI.Data;
using MotorInsuranceAPI.DTOs;
using MotorInsuranceAPI.Models;

namespace MotorInsuranceAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AuthService(
            ApplicationDbContext context,
            IPasswordHasher<User> passwordHasher,
            IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        // Register a new user.
        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
        {
            string email = dto.Email.Trim().ToLowerInvariant();

            // Check whether the email is already registered.
            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == email);

            if (emailExists)
            {
                return null;
            }

            // Create the user without storing a plain-text password.
            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = email,
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            // Hash the password before saving it.
            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                dto.Password
            );

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate a token after successful registration.
            return CreateAuthResponse(user);
        }

        // Log in an existing user.
        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            string email = dto.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return null;
            }

            // Verify the entered password against its stored hash.
            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.Password
            );

            if (result == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return CreateAuthResponse(user);
        }

        // Generate a JWT token and prepare the response.
        private AuthResponseDto CreateAuthResponse(User user)
        {
            string issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer is missing.");

            string audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("Jwt:Audience is missing.");

            string key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key is missing.");

            var expiresAt = DateTime.UtcNow.AddHours(2);

            var claims = new List<System.Security.Claims.Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key)
            );

            var credentials = new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            return new AuthResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAtUtc = expiresAt
            };
        }
    }
}
