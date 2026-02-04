using DvizhX.Application.Common.Interfaces.Authentication;
using DvizhX.Application.Features.Auth.Common;
using Google.Apis.Auth;

namespace DvizhX.Infrastructure.Authentication
{
    public class GoogleTokenValidator : IGoogleTokenValidator
    {
        public async Task<GoogleUserPayload> ValidateAsync(string idToken)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
                return new GoogleUserPayload(payload.Email, payload.Name, payload.Subject, payload.Picture);
            }
            catch
            {
                throw new Exception("Invalid Google Token");
            }
        }
    }
}
