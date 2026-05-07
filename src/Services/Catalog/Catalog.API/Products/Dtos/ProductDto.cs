namespace Catalog.API.Products.Dtos;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyList<string> Category,
    string ImageFile,
    decimal Price,
    DateTimeOffset DateCreated,
    DateTimeOffset? DateLastUpdated
);

public sealed record ProductAuditTrailDto(
    Guid Id,
    Guid ProductId,
    string Action,
    string UserId,
    DateTimeOffset OccurredOn
);
