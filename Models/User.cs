
using System.ComponentModel.DataAnnotations;

namespace MotorInsuranceAPI.Models
{
    public class User
    {
        // Unique ID for every user.
        public int Id { get; set; }

        // Full name of the user.
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        // Email address used for login.
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        // Stores the hashed password, never the plain-text password.
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // Normal users receive the User role by default.
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "User";

        // Date and time when the user registers.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
