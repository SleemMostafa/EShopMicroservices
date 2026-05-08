using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BuildingBlocks.Identity;

public sealed class CurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    private const string UserIdHeaderName = "X-User-Id";
    private const string SystemUserId = "system";

    public string UserId
    {
        get
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContextAccessor.HttpContext?.User.FindFirstValue("sub");

            if (!string.IsNullOrWhiteSpace(userId))
            {
                return userId;
            }

            if (httpContextAccessor.HttpContext?.Request.Headers.TryGetValue(UserIdHeaderName, out var values) == true)
            {
                userId = values.FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    return userId;
                }
            }

            return SystemUserId;
        }
    }
}
