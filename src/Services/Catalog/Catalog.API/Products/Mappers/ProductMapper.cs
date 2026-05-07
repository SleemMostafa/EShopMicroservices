using Catalog.API.Products.CreateProduct;
using Catalog.API.Products.DeleteProduct;
using Catalog.API.Products.GetProductAuditHistory;
using Catalog.API.Products.GetProductByCategory;
using Catalog.API.Products.GetProductById;
using Catalog.API.Products.GetProducts;
using Catalog.API.Products.UpdateProduct;

namespace Catalog.API.Products.Mappers;

public static class ProductMapper
{
    public static CreateProductCommand ToCommand(CreateProductRequest request)
    {
        return new CreateProductCommand(
            request.Name,
            request.Description,
            request.Category,
            request.ImageFile,
            request.Price);
    }

    public static CreateProductResponse ToResponse(CreateProductResult result)
    {
        return new CreateProductResponse(result.Id);
    }

    public static UpdateProductCommand ToCommand(UpdateProductRequest request)
    {
        return new UpdateProductCommand(
            request.Id,
            request.Name,
            request.Description,
            request.ImageFile,
            request.Price,
            request.Category);
    }

    public static UpdateProductResponse ToResponse(UpdateProductResult result)
    {
        return new UpdateProductResponse(result.IsSuccess);
    }

    public static DeleteProductResponse ToResponse(DeleteProductResponse result)
    {
        return new DeleteProductResponse(result.IsSuccess);
    }

    public static GetProductsResponse ToResponse(GetProductsResult result)
    {
        return new GetProductsResponse(result.Products.Select(ToDto).ToList());
    }

    public static GetProductsByCategoryResponse ToResponse(GetProductsByCategoryResult result)
    {
        return new GetProductsByCategoryResponse(result.Products.Select(ToDto).ToList());
    }

    public static GetProductByIdResponse ToResponse(GetProductByIdResult result)
    {
        return new GetProductByIdResponse(ToDto(result.Product));
    }

    public static GetProductAuditHistoryResponse ToResponse(GetProductAuditHistoryResult result)
    {
        return new GetProductAuditHistoryResponse(result.AuditHistory.Select(ToDto).ToList());
    }

    public static ProductDto ToDto(Product product)
    {
        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Category,
            product.ImageFile,
            product.Price,
            product.DateCreated,
            product.DateLastUpdated);
    }

    public static ProductAuditTrailDto ToDto(ProductAuditTrail auditTrail)
    {
        return new ProductAuditTrailDto(
            auditTrail.Id,
            auditTrail.ProductId,
            auditTrail.Action.ToString(),
            auditTrail.UserId,
            auditTrail.OccurredOn);
    }
}
