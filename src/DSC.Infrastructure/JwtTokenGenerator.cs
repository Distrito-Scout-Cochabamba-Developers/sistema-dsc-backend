using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DSC.Application.DTOs;
using DSC.Application.Interfaces;
using DSC.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DSC.Infrastructure;

/// <summary>Emite tokens HS256 con la misma configuración validada que utiliza la API.</summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private const string RolesClaim = "roles";
    private const string PersonIdClaim = "personId";
    private const string UsernameClaim = "username";

    private readonly JwtSettings _settings;

    /// <summary>Valida la configuración externa de autenticación al construir el emisor.</summary>
    /// <param name="configuration">Configuración con el secreto, emisor y audiencia JWT.</param>
    public JwtTokenGenerator(IConfiguration configuration) =>
        _settings = JwtSettings.Load(configuration);

    public JwtTokenResult GenerateToken(User user)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_settings.ExpiresMinutes);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(PersonIdClaim, user.PersonId.ToString(CultureInfo.InvariantCulture)),
            new(UsernameClaim, user.Username),
        };

        claims.AddRange(
            user.GetActiveRoleCodes(today).Select(roleCode => new Claim(RolesClaim, roleCode))
        );

        var credentials = new SigningCredentials(
            _settings.CreateSigningKey(),
            SecurityAlgorithms.HmacSha256
        );
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials
        );

        return new JwtTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
