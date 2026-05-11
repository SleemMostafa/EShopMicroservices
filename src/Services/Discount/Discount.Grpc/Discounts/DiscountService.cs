using Discount.Grpc.Models;
using Grpc.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Discounts;

public sealed class DiscountService(DiscountContext context, ILogger<DiscountService> logger)
    : DiscountProtoService.DiscountProtoServiceBase
{
    public override async Task<CouponModel> CreateDiscount(
        CreateDiscountRequest request,
        ServerCallContext serverCallContext)
    {
        var coupon = request.Coupon.Adapt<Coupon>();
        if (coupon is null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request object."));
        }

        context.Coupons.Add(coupon);
        await context.SaveChangesAsync(serverCallContext.CancellationToken);

        logger.LogInformation("Discount is successfully created. ProductName : {ProductName}", coupon.ProductName);

        return coupon.Adapt<CouponModel>();
    }

    public override async Task<CouponModel> GetDiscount(
        GetDiscountRequest request,
        ServerCallContext serverCallContext)
    {
        var coupon = await context
            .Coupons
            .FirstOrDefaultAsync(
                coupon => coupon.ProductName == request.ProductName,
                serverCallContext.CancellationToken);

        coupon ??= new Coupon
        {
            ProductName = "No Discount",
            Amount = 0,
            Description = "No Discount Desc"
        };

        logger.LogInformation(
            "Discount is retrieved for ProductName : {ProductName}, Amount : {Amount}",
            coupon.ProductName,
            coupon.Amount);

        return coupon.Adapt<CouponModel>();
    }

    public override async Task<CouponModel> UpdateDiscount(
        UpdateDiscountRequest request,
        ServerCallContext serverCallContext)
    {
        var coupon = request.Coupon.Adapt<Coupon>();
        if (coupon is null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request object."));
        }

        context.Coupons.Update(coupon);
        await context.SaveChangesAsync(serverCallContext.CancellationToken);

        logger.LogInformation("Discount is successfully updated. ProductName : {ProductName}", coupon.ProductName);

        return coupon.Adapt<CouponModel>();
    }

    public override async Task<DeleteDiscountResponse> DeleteDiscount(
        DeleteDiscountRequest request,
        ServerCallContext serverCallContext)
    {
        var coupon = await context
            .Coupons
            .FirstOrDefaultAsync(
                coupon => coupon.ProductName == request.ProductName,
                serverCallContext.CancellationToken);

        if (coupon is null)
        {
            throw new RpcException(
                new Status(StatusCode.NotFound, $"Discount with ProductName={request.ProductName} is not found."));
        }

        context.Coupons.Remove(coupon);
        await context.SaveChangesAsync(serverCallContext.CancellationToken);

        logger.LogInformation("Discount is successfully deleted. ProductName : {ProductName}", request.ProductName);

        return new DeleteDiscountResponse { Success = true };
    }
}
