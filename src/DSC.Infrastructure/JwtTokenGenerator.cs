using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DSC.Application.DTOs;
using DSC.Application.Interfaces;
using DSC.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DSC.Infrastructure;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private const string RolesClaim = "roles";
    private const string PersonIdClaim = "personId";
    private const string UsernameClaim = "username";

    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration) => _configuration = configuration;

    public JwtTokenResult GenerateToken(User user)
    {
        var key = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
            throw new InvalidOperationException("Jwt:Key debe tener al menos 32 bytes.");

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(GetExpirationMinutes());
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
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256
        );
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials
        );

        return new JwtTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private int GetExpirationMinutes() =>
        int.TryParse(_configuration["Jwt:ExpiresMinutes"], out var configuredMinutes)
        && configuredMinutes > 0
            ? configuredMinutes
            : 60;
}
