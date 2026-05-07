namespace Catalog.API.Products.GetProducts;

public sealed record GetProductsResponse(IEnumerable<ProductDto> Products);

public sealed class GetProductsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetProductsQuery(), ct);

                var response = ProductMapper.ToResponse(result);

                return Results.Ok(response);
            })
            .WithName("GetProducts")
            .Produces<GetProductsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get all products")
            .WithDescription("Get all products");
    }
}
