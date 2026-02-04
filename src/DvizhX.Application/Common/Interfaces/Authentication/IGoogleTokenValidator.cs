using DvizhX.Application.Features.Auth.Common;

namespace DvizhX.Application.Common.Interfaces.Authentication
{
    public interface IGoogleTokenValidator
    {
        Task<GoogleUserPayload> ValidateAsync(string idToken);
    }
}
