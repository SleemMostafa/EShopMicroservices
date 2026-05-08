namespace Identity.API.Auth.RefreshToken;

public sealed class RefreshTokenEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/identity/refresh-token", async (RefreshTokenRequest request, ISender sender, CancellationToken ct) =>
            {
                var response = await sender.Send(new RefreshTokenCommand(request.RefreshToken), ct);
                return Results.Ok(response);
            })
            .AllowAnonymous()
            .WithName("RefreshToken")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Refresh token")
            .WithDescription("Refresh token");
    }
}
