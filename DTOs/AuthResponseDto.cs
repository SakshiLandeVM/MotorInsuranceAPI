
namespace MotorInsuranceAPI.DTOs
{
    // Defines the response returned after successful
    // registration or login.
    public class AuthResponseDto
    {
        // Unique ID of the logged-in user.
        public int UserId { get; set; }

        // Name of the user.
        public string FullName { get; set; } = string.Empty;

        // Email address of the user.
        public string Email { get; set; } = string.Empty;

        // Role assigned to the user.
        public string Role { get; set; } = string.Empty;

        // JWT token used to access protected API endpoints.
        public string Token { get; set; } = string.Empty;

        // UTC date and time when the token expires.
        public DateTime ExpiresAtUtc { get; set; }
    }
}
