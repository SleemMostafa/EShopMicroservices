using Ordering.Application.Orders.Commands.CreateOrder;

namespace Ordering.API.Endpoints;

public record CreateOrderRequest(OrderDto Order);
public record CreateOrderResponse(Guid Id);

public sealed class CreateOrder : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", async (CreateOrderRequest request, HttpContext context, ISender sender, CancellationToken ct) =>
        {
            var command = request.Adapt<CreateOrderCommand>() with
            {
                IdempotencyKey = context.Request.Headers["Idempotency-Key"].FirstOrDefault()
            };

            var result = await sender.Send(command, ct);

            var response = result.Adapt<CreateOrderResponse>();

            return Results.Created($"/orders/{response.Id}", response);
        })
        .WithName("CreateOrder")
        .Produces<CreateOrderResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Create Order")
        .WithDescription("Create Order");
    }
}
