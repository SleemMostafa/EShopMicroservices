﻿namespace Catalog.API.Products.GetProductByCategory;

public sealed record GetProductsByCategoryResponse(IEnumerable<ProductDto> Products);

public sealed class GetProductsByCategoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/category/{category}", async (string category, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetProductsByCategoryQuery(category), ct);

                var response = result.Adapt<GetProductsByCategoryResponse>();

                return Results.Ok(response);
            })
            .WithName("GetProductsByCategory")
            .Produces<GetProductsByCategoryResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get products by category")
            .WithDescription("Get products by category");
    }
}