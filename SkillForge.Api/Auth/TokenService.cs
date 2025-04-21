using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace SkillForge.Api.Auth;

public class TokenService : ITokenService
{
    private readonly JwtSettings _settings;
    private readonly byte[] _key;

    public TokenService(IOptions<JwtSettings> opts)
    {
        _settings = opts.Value;
        _key = Encoding.UTF8.GetBytes(_settings.SecretKey);
    }

    public string GenerateToken(string userName, IEnumerable<Claim>? extraClaims = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, userName) };
        if (extraClaims is not null) claims.AddRange(extraClaims);

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(_key),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:  _settings.Issuer,
            audience:_settings.Audience,
            claims:  claims,
            expires: DateTime.UtcNow.AddDays(_settings.ExpirationDays),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
