namespace Catalog.API.Products.UpdateProduct;

public sealed record UpdateProductRequest(
    Guid Id,
    string Name,
    string Description,
    string ImageFile,
    decimal Price,
    List<string> Category);

public sealed record UpdateProductResponse(bool IsSuccess);

public sealed class UpdateProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/products", async (UpdateProductRequest request, ISender sender, CancellationToken ct) =>
            {
                var command = ProductMapper.ToCommand(request);

                var result = await sender.Send(command, ct);

                var response = ProductMapper.ToResponse(result);

                return Results.Ok(response);
            })
            .WithName("UpdateProduct")
            .Produces<UpdateProductResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update Product")
            .WithDescription("Update Product");
    }
}
