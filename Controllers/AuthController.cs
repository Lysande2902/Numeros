using Microsoft.AspNetCore.Mvc;
using ParImparAPI.Domain.Entities;
using ParImparAPI.Domain.Services;

namespace ParImparAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Username y Password son requeridos" });
            }

            if (_authService.ValidateCredentials(request.Username, request.Password))
            {
                var token = _authService.GenerateJwtToken(request.Username);

                var response = new LoginResponse
                {
                    Token = token,
                    Username = request.Username,
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                };

                return Ok(response);
            }

            return Unauthorized(new { message = "Credenciales inválidas" });
        }
    }
}
