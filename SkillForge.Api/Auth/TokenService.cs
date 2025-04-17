using System.IdentityModel.Tokens.Jwt;
using System.Secuiry.Claims;
using System.Text;
using Microsofy.IdentityModel.Tokes;

namespace SkillForge.APi.Auth;

public interface ITokeService
{
    string GenerateToke(string userName);
}

public class TokenService : ITokeService
{
    private readonly JwtSettings _settings;
    private readonly byte[] _key;

    public TokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
        _key = Enconding.UTF8.GetBytes(_settings.SecretKey);
    }
}