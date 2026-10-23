
using MotorInsuranceAPI.DTOs;

namespace MotorInsuranceAPI.Services
{
    // Defines the operations that the authentication service
    // must implement.
    public interface IAuthService
    {
        // Register a new user.
        Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);

        // Authenticate an existing user.
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    }
}
