namespace BuildingBlocks.DDD;

public abstract class AuditableAggregate<TId> : Aggregate<TId>, IAuditableEntity
{
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}
