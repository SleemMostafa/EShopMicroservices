namespace Identity.API.Auth.Login;

public sealed class LoginEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/identity/login", async (LoginRequest request, ISender sender, CancellationToken ct) =>
            {
                var response = await sender.Send(new LoginCommand(request.Email, request.Password), ct);
                return Results.Ok(response);
            })
            .AllowAnonymous()
            .WithName("Login")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Login")
            .WithDescription("Login");
    }
}
