namespace Basket.API.Basket.StoreBasket;

public sealed record StoreBasketRequest(ShoppingCartDto Cart);
public sealed record StoreBasketResponse(string UserName);

public sealed class StoreBasketEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/basket", async (StoreBasketRequest request, ISender sender, CancellationToken ct) =>
            {
                var command = BasketMapper.ToCommand(request);

                var result = await sender.Send(command, ct);

                var response = BasketMapper.ToResponse(result);

                return Results.Created($"/basket/{response.UserName}", response);
            })
            .WithName("StoreBasket")
            .Produces<StoreBasketResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Store basket")
            .WithDescription("Create or update a basket");
    }
}
