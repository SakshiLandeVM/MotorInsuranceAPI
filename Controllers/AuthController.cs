
using Microsoft.AspNetCore.Mvc;
using MotorInsuranceAPI.DTOs;
using MotorInsuranceAPI.Services;

namespace MotorInsuranceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        // Receive the authentication service through dependency injection.
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            // Return a conflict response if the email already exists.
            if (result == null)
            {
                return Conflict(new
                {
                    message = "An account with this email already exists."
                });
            }

            // Return the user information and JWT token.
            return Ok(result);
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            // Reject invalid email or password.
            if (result == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            // Return the user information and JWT token.
            return Ok(result);
        }
    }
}
