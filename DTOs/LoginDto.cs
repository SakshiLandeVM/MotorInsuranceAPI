
using System.ComponentModel.DataAnnotations;

namespace MotorInsuranceAPI.DTOs
{
    // Defines the data required when a user logs in.
    public class LoginDto
    {
        // Email address used during registration.
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        // Password provided by the user.
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }
}
