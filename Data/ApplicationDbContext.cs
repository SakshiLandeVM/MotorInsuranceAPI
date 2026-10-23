
using Microsoft.EntityFrameworkCore;
using MotorInsuranceAPI.Models;

namespace MotorInsuranceAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Receives database configuration from Program.cs.
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options
        ) : base(options)
        {
        }

        // Represents the Claims table.
        public DbSet<Claim> Claims { get; set; } = null!;

        // Represents the Users table.
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Store claim amounts with two decimal places.
            modelBuilder.Entity<Claim>()
                .Property(c => c.ClaimAmount)
                .HasPrecision(18, 2);

            // Each email address must be unique.
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
