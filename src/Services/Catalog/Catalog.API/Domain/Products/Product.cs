using BuildingBlocks.Exceptions;

namespace Catalog.API.Domain.Products;

public sealed class Product
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

    public Guid Id { get; private set; }
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
        ValidateProduct(name, description, category, imageFile, price);

        return new Product(
            Guid.NewGuid(),
            name,
            description,
            NormalizeCategory(category),
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
        ValidateProduct(name, description, category, imageFile, price);

        return new Product(
            id,
            name,
            description,
            NormalizeCategory(category),
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
        ValidateProduct(name, description, category, imageFile, price);

        Name = name;
        Description = description;
        Category = NormalizeCategory(category);
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

    private static void ValidateProduct(
        string name,
        string description,
        List<string> category,
        string imageFile,
        decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BadRequestException("Product name is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new BadRequestException("Product description is required.");
        }

        if (category is null || category.Count == 0 || NormalizeCategory(category).Count == 0)
        {
            throw new BadRequestException("Product category is required.");
        }

        if (string.IsNullOrWhiteSpace(imageFile))
        {
            throw new BadRequestException("Product image file is required.");
        }

        if (price <= 0)
        {
            throw new BadRequestException("Product price must be greater than zero.");
        }
    }
}
