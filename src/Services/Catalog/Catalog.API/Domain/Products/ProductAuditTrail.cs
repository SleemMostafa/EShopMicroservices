using BuildingBlocks.DDD;

namespace Catalog.API.Domain.Products;

public sealed class ProductAuditTrail : Aggregate<Guid>
{
    private ProductAuditTrail()
    {
    }

    private ProductAuditTrail(
        Guid id,
        Guid productId,
        ProductAuditAction action,
        string userId,
        DateTimeOffset occurredOn)
    {
        Id = id;
        ProductId = productId;
        Action = action;
        UserId = userId;
        OccurredOn = occurredOn;
    }

    public Guid ProductId { get; private set; }
    public ProductAuditAction Action { get; private set; }
    public string UserId { get; private set; } = default!;
    public DateTimeOffset OccurredOn { get; private set; }

    public static ProductAuditTrail Create(
        Guid productId,
        ProductAuditAction action,
        string userId,
        DateTimeOffset occurredOn)
    {
        return new ProductAuditTrail(
            Guid.NewGuid(),
            productId,
            action,
            userId,
            occurredOn);
    }
}
