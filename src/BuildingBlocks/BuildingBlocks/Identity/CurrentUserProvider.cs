using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Identity;

public sealed class CurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    private const string UserIdHeaderName = "X-User-Id";
    private const string SystemUserId = "system";

    public string UserId
    {
        get
        {
            if (httpContextAccessor.HttpContext?.Request.Headers.TryGetValue(UserIdHeaderName, out var values) == true)
            {
                var userId = values.FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    return userId;
                }
            }

            return SystemUserId;
        }
    }
}
