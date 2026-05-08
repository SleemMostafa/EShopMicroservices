namespace Identity.API.Auth.RevokeRefreshToken;

public sealed class RevokeRefreshTokenEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/identity/revoke-refresh-token", async (
                RevokeRefreshTokenRequest request,
                ISender sender,
                CancellationToken ct) =>
            {
                var response = await sender.Send(new RevokeRefreshTokenCommand(request.RefreshToken), ct);
                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithName("RevokeRefreshToken")
            .Produces<RevokeRefreshTokenResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Revoke refresh token")
            .WithDescription("Revoke refresh token");
    }
}
