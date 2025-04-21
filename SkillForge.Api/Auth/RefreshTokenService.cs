using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace SkillForge.Api.Auth;

public interface IRefreshTokenService
{
    (string accessToken, string refreshToken) GeneratePair(string user, IEnumerable<Claim> roles);
    string Rotate(string refreshToken);
}

public class RefreshTokenService : IRefreshTokenService
{
    private readonly ConcurrentDictionary<string, string> _store = new();
    private readonly ITokenService _tokenService;

    public RefreshTokenService(ITokenService ts) => _tokenService = ts;

    public (string, string) GeneratePair(string user, IEnumerable<Claim> roles)
    {
        var access  = _tokenService.GenerateToken(user, roles);
        var refresh = Guid.NewGuid().ToString("N");
        _store[refresh] = user;
        return (access, refresh);
    }

    public string Rotate(string refreshToken)
    {
        if (_store.TryRemove(refreshToken, out var user))
        {
            var newAccess  = _tokenService.GenerateToken(user, null);
            var newRefresh = Guid.NewGuid().ToString("N");
            _store[newRefresh] = user;
            return newAccess;
        }
        throw new SecurityTokenException("Refresh token inválido.");
    }
}
