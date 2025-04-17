using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace SkillForge.Api.Auth;

public class JwtSettings
{
    public string Issuer         { get; init; } = string.Empty;
    public string Audience       { get; init; } = string.Empty;
    public string SecretKey      { get; init; } = string.Empty;
    public int    ExpirationDays { get; init; } = 7;
}

public interface ITokenService
{
    string GenerateToken(string userName);
}

public class TokenService : ITokenService
{
    private readonly JwtSettings _settings;
    private readonly byte[] _key;

    public TokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
        _key = Encoding.UTF8.GetBytes(_settings.SecretKey);
    }

    public string GenerateToken(string userName)
    {
        var creds = new SigningCredentials(
            new SymmetricSecurityKey(_key),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:  _settings.Issuer,
            audience:_settings.Audience,
            claims:  new[] { new Claim(ClaimTypes.Name, userName) },
            expires: DateTime.UtcNow.AddDays(_settings.ExpirationDays),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
