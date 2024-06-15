namespace Catalog.API.Models;

public class Product
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required List<string> Category { get; init; } = [];
    public required string ImageFile { get; init; }
    public required decimal Price { get; init; }
    public required DateTimeOffset DateCreated { get; init; }
    public DateTimeOffset? DateLastUpdated { get; private set; }

    public void UpdateDateLastUpdated(DateTimeOffset now)
    {
        DateLastUpdated = now;
    }
}