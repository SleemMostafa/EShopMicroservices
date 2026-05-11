namespace Basket.API.Basket.CheckoutBasket;

public sealed record CheckoutBasketRequest(BasketCheckoutDto BasketCheckoutDto);
public sealed record CheckoutBasketResponse(bool IsSuccess);

public sealed class CheckoutBasketEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/basket/checkout", async (CheckoutBasketRequest request, ISender sender, CancellationToken ct) =>
            {
                var command = BasketMapper.ToCommand(request);

                var result = await sender.Send(command, ct);

                var response = BasketMapper.ToResponse(result);

                return Results.Ok(response);
            })
            .WithName("CheckoutBasket")
            .Produces<CheckoutBasketResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Checkout basket")
            .WithDescription("Checkout basket");
    }
}
