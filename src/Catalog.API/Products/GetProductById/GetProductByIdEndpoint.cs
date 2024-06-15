namespace Catalog.API.Products.GetProductById;

public sealed record GetProductByIdResponse(ProductDto Product);
public sealed class GetProductByIdEndpoint:ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id}", async (Guid id,ISender sender,CancellationToken ct) =>
            {
                var result = await sender.Send(new GetProductByIdQuery(id), ct);

                var response = result.Adapt<GetProductByIdResponse>();

                return Results.Ok(response);
            }) 
            .WithName("GetProductById")
            .Produces<GetProductByIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get product by id")
            .WithDescription("Get product by id");
    }
}