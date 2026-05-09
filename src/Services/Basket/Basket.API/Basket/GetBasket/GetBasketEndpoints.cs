namespace Basket.API.Basket.GetBasket;

public record GetBasketResponse(ShoppingCartDto Cart);

public sealed class GetBasketEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/basket/{userName}", async (string userName, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetBasketQuery(userName), ct);

                var response = BasketMapper.ToResponse(result);

                return Results.Ok(response);
            })
            .WithName("GetBasket")
            .Produces<GetBasketResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get basket")
            .WithDescription("Get basket by user name");
    }
}
