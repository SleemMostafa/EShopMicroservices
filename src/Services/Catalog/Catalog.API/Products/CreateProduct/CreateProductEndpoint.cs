namespace Catalog.API.Products.CreateProduct;

public sealed record CreateProductRequest(
    string Name,
    string Description,
    List<string> Category,
    string ImageFile,
    decimal Price);

public sealed record CreateProductResponse(Guid Id);

public sealed class CreateProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", async (CreateProductRequest request, ISender sender, CancellationToken ct) =>
            {
                var command = ProductMapper.ToCommand(request);

                var result = await sender.Send(command, ct);

                var response = ProductMapper.ToResponse(result);
                
                return Results.Created($"/product{response.Id}", response);
            })
            .WithName("CreateProduct")
            .Produces<CreateProductResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create Product")
            .WithDescription("Create Product");
    }
}
