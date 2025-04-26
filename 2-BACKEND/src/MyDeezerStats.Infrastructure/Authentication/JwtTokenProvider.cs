using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyDeezerStats.Domain.Entities;
using MyDeezerStats.Infrastructure.Settings;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MyDeezerStats.Infrastructure.Authentication
{
    public class JwtTokenProvider
    {
        private readonly JwtSettings _jwtSettings;

        public JwtTokenProvider(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        // Méthode pour générer un token JWT
        public string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtSettings.ExpirationInMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Méthode pour valider un token (optionnelle, selon tes besoins)
        //public ClaimsPrincipal ValidateToken(string token)
        //{
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        //    try
        //    {
        //        var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
        //        {
        //            ValidateIssuer = true,
        //            ValidateAudience = true,
        //            ValidateLifetime = true,
        //            ValidateIssuerSigningKey = true,
        //            ValidIssuer = _jwtSettings.Issuer,
        //            ValidAudience = _jwtSettings.Audience,
        //            IssuerSigningKey = new SymmetricSecurityKey(key)
        //        }, out var validatedToken);

        //        return principal; // Retourne les informations du token validé
        //    }
        //    catch (Exception)
        //    {
        //        return null; // Si la validation échoue, retourne null (ou une autre gestion d'erreur)
        //    }
        //}
    }
}
