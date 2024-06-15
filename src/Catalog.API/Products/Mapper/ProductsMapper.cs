using Catalog.API.Products.Dtos;

namespace Catalog.API.Products.Mapper;

public static class ProductsMapper
{
    public static IEnumerable<ProductDto> ToProductsDto(this IReadOnlyList<Product> products)
    {
        return products.Select(product => new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Category,
            product.ImageFile,
            product.Price,
            product.DateCreated,
            product.DateUpdated
            ));
    }
}