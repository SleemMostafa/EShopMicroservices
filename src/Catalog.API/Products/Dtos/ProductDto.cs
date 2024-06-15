namespace Catalog.API.Products.Dtos;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Description,
    List<string> Category,
    string ImageFile,
    decimal Price,
    DateTimeOffset DateCreated,
    DateTimeOffset? DateUpdated
);
