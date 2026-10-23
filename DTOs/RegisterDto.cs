
using System.ComponentModel.DataAnnotations;

namespace MotorInsuranceAPI.DTOs
{
    // Defines the data required to register a new user.
    public class RegisterDto
    {
        // User's full name.
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        // User's email address.
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        // Password provided during registration.
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must contain at least 8 characters.")]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;
    }
}
