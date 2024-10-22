namespace Discount.Grpc.Models;

public class Coupon
{
    public int Id { get; init; }
    public required string ProductName { get; init; }
    public required string Description { get; init; }
    public required int Amount { get; init; }
}