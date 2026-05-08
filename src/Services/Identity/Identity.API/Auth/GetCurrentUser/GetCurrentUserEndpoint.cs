using System.Security.Claims;

namespace Identity.API.Auth.GetCurrentUser;

public sealed class GetCurrentUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/identity/me", (ClaimsPrincipal user) =>
            {
                var response = new CurrentUserResponse(
                    user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? string.Empty,
                    user.FindFirstValue(ClaimTypes.Name),
                    user.FindFirstValue(ClaimTypes.Email));

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithName("GetCurrentUser")
            .Produces<CurrentUserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Get current user")
            .WithDescription("Get current user");
    }
}
