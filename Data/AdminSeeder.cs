
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MotorInsuranceAPI.Models;

namespace MotorInsuranceAPI.Data
{
    public class AdminSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AdminSeeder(
            ApplicationDbContext context,
            IPasswordHasher<User> passwordHasher,
            IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        public async Task CreateAdminAsync()
        {
            var email = _configuration["Admin:Email"];
            var password = _configuration["Admin:Password"];
            var fullName = _configuration["Admin:FullName"];

            // Do not create an admin without configured credentials.
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fullName))
            {
                return;
            }

            email = email.Trim().ToLowerInvariant();

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
            {
                if (existingUser.Role != "Admin")
                {
                  existingUser.Role = "Admin";
                  await _context.SaveChangesAsync();
                }
                // Never silently promote an existing customer to Admin.
                return;
            }

            var admin = new User
            {
                FullName = fullName.Trim(),
                Email = email,
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };

            admin.PasswordHash = _passwordHasher.HashPassword(
                admin,
                password
            );

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();
        }
    }
}
