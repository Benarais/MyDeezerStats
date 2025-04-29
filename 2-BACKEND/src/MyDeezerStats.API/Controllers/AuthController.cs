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

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] Microsoft.AspNetCore.Identity.Data.LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Signup attempt for {Email}", request.Email);

                var createUserResult = await _authService.CreateUser(request.Email, request.Password);

                _logger.LogInformation("Signup successful for {Email}", request.Email);

                return Ok(true);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Signup failed: user already exists ({Email})", request.Email);
                // 409 Conflict pour utilisateur existant
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during signup for {Email}", request?.Email);
                return StatusCode(500, new { message = "Une erreur interne est survenue" });
            }
        }



        //[HttpPost("signup")]
        //public async Task<IActionResult> SignUp([FromBody] Microsoft.AspNetCore.Identity.Data.LoginRequest request)
        //{
        //    try
        //    {
        //        _logger.LogInformation($"Signup attempt for {request.Email}");

        //        var createUserResult = await _authService.CreateUser(request.Email, request.Password);

        //        if (!createUserResult)
        //        {
        //            _logger.LogWarning($"Signup failed for {request.Email}, user creation failed.");
        //            return Ok(false);
        //        }

        //        _logger.LogInformation($"Signup successful for {request.Email}");

        //        return Ok(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error during signup for {request?.Email}");
        //        return StatusCode(500, new { message = "Une erreur interne est survenue" });
        //    }
        //}

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
