using Microsoft.Extensions.Options;
using MyDeezerStats.Application.Interfaces;
using MyDeezerStats.Domain.Entities;
using MyDeezerStats.Domain.Repositories;
using MyDeezerStats.Infrastructure.Settings;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;
using DnsClient.Internal;


namespace MyDeezerStats.Application.MongoDbServices
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly JwtSettings _jwtSettings;

        public AuthService(IUserRepository userRepository, IOptions<JwtSettings> jwtSettings, ILogger<AuthService> logger)
        {
            _logger = logger;
            _userRepository = userRepository;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<string> Authenticate(string username, string password)
        {
            _logger.LogInformation("Tentative de connexion pour l'utilisateur: {Email}", username);

            var user = await _userRepository.GetByUsername(username);
            if (user == null) {
                _logger.LogWarning("Utilisateur non trouvé pour l'email: {Email}", username);
                throw new InvalidOperationException("Invalid credentials");
            }
            if (!VerifyPassword(user, password))
            {
                _logger.LogWarning("Mot de passe incorrect de {email}", username);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            return GenerateJwtToken(user);
        }

        private bool VerifyPassword(User user, string password)
        {
            //// Utilisation de bcrypt pour vérifier le mot de passe
            //_logger.LogInformation(user.Email);
            //_logger.LogInformation("mot de passe {password}", user.PasswordHash);
            //var test = BCrypt.Net.BCrypt.Verify(password, "$2a$11$aCGMoEm35/h4kogbbgl3wuLj0fMsOTc6tiP2gcYJkJ693KUlwI4KS");
            //_logger.LogInformation(BCrypt.Net.BCrypt.HashPassword(password));
            //_logger.LogInformation("hash is valide {}", test

            return password == user.PasswordHash;

            //return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _jwtSettings.Issuer,
                _jwtSettings.Audience,
                claims,
                expires: DateTime.Now.AddMinutes(_jwtSettings.ExpirationInMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
