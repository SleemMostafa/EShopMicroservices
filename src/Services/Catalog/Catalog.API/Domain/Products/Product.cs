using ArgumentValidator;
using BuildingBlocks.DDD;
using BuildingBlocks.Exceptions;

namespace Catalog.API.Domain.Products;

public sealed class Product : Aggregate<Guid>
{
    private Product()
    {
    }

    private Product(
        Guid id,
        string name,
        string description,
        List<string> category,
        string imageFile,
        decimal price,
        DateTimeOffset dateCreated)
    {
        Id = id;
        Name = name;
        Description = description;
        Category = category;
        ImageFile = imageFile;
        Price = price;
        DateCreated = dateCreated;
    }

    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public List<string> Category { get; private set; } = [];
    public string ImageFile { get; private set; } = default!;
    public decimal Price { get; private set; }
    public DateTimeOffset DateCreated { get; private set; }
    public DateTimeOffset? DateLastUpdated { get; private set; }

    public static Product Create(
        string name,
        string description,
        List<string> category,
        string imageFile,
        decimal price,
        DateTimeOffset now)
    {
        var normalizedCategory = ValidateProduct(name, description, category, imageFile, price);

        return new Product(
            Guid.NewGuid(),
            name,
            description,
            normalizedCategory,
            imageFile,
            price,
            now);
    }

    public static Product Seed(
        Guid id,
        string name,
        string description,
        List<string> category,
        string imageFile,
        decimal price,
        DateTimeOffset dateCreated)
    {
        var normalizedCategory = ValidateProduct(name, description, category, imageFile, price);

        return new Product(
            id,
            name,
            description,
            normalizedCategory,
            imageFile,
            price,
            dateCreated);
    }

    public void Update(
        DateTimeOffset now,
        string name,
        string description,
        List<string> category,
        string imageFile,
        decimal price)
    {
        var normalizedCategory = ValidateProduct(name, description, category, imageFile, price);

        Name = name;
        Description = description;
        Category = normalizedCategory;
        ImageFile = imageFile;
        Price = price;
        DateLastUpdated = now;
    }

    private static List<string> NormalizeCategory(IEnumerable<string> categories)
    {
        return categories
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Select(category => category.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<string> ValidateProduct(
        string name,
        string description,
        List<string> category,
        string imageFile,
        decimal price)
    {
        try
        {
            Throw.IfNullOrEmpty(name, nameof(name));
            Throw.IfNullOrEmpty(description, nameof(description));
            Throw.IfNullOrEmpty(category, nameof(category));
            Throw.IfNullOrEmpty(imageFile, nameof(imageFile));
            Throw.IfNot(() => price > 0);

            var normalizedCategory = NormalizeCategory(category);
            Throw.IfNullOrEmpty(normalizedCategory, nameof(category));

            return normalizedCategory;
        }
        catch (ArgumentException exception)
        {
            throw new BadRequestException("Invalid product data.", exception.Message);
        }
    }
}
