using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using MyDeezerStats.Application.Interfaces;



namespace MyDeezerStats.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Microsoft.AspNetCore.Identity.Data.LoginRequest request)
        {
            try
            {
                _logger.LogInformation($"Login attempt for {request.Email}");

                var token = await _authService.Authenticate(request.Email, request.Password);

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning($"Login failed for {request.Email}");
                    return Unauthorized(new { message = "Email ou mot de passe incorrect" });
                }

                _logger.LogInformation($"Login successful for {request.Email}");

                return Ok(new
                {
                    token = token,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during login for {request?.Email}");
                return StatusCode(500, new { message = "Une erreur interne est survenue" });
            }
        }
    }
}
