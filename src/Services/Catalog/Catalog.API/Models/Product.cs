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

    public Product Update(DateTimeOffset now,string name,string description,List<string> category,string imageFile, decimal price)
    {
        return new Product
        {
            Id = Id,
            Category = category,
            Description = description,
            Name = name,
            Price = price,
            DateCreated = DateCreated,
            ImageFile = imageFile,
            DateLastUpdated = now,
        };
    }
}