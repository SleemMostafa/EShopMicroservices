namespace Identity.API.Auth.Register;

public sealed class RegisterEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/identity/register", async (RegisterRequest request, ISender sender, CancellationToken ct) =>
            {
                var response = await sender.Send(
                    new RegisterCommand(request.UserName, request.Email, request.Password),
                    ct);

                return Results.Ok(response);
            })
            .AllowAnonymous()
            .WithName("Register")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Register user")
            .WithDescription("Register user");
    }
}
