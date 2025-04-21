using System.Security.Claims;

namespace SkillForge.Api.Auth;

public interface ITokenService
{
    string GenerateToken(string userName, IEnumerable<Claim>? extraClaims = null);
}
