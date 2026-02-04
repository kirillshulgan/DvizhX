using System.Security.Claims;

namespace DvizhX.Application.Common.Interfaces.Authentication
{
    public interface IJwtTokenValidator
    {
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
