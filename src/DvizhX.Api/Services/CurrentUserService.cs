using DvizhX.Application.Common.Interfaces;
using System.Security.Claims;

namespace DvizhX.Api.Services
{
    public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
    {
        public Guid? UserId
        {
            get
            {
                var id = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(id))
                {
                    return null;
                }

                return Guid.TryParse(id, out var userId) ? userId : null;
            }
        }
    }
}
