using Discount.Grpc.Models;

namespace Discount.Grpc.Discounts.Mappers;

public static class CouponMapper
{
    public static Coupon? ToCoupon(CouponModel? coupon)
    {
        if (coupon is null)
        {
            return null;
        }

        return new Coupon
        {
            Id = coupon.Id,
            ProductName = coupon.ProductName,
            Description = coupon.Description,
            Amount = coupon.Amount
        };
    }

    public static CouponModel ToCouponModel(Coupon coupon)
    {
        return new CouponModel
        {
            Id = coupon.Id,
            ProductName = coupon.ProductName,
            Description = coupon.Description,
            Amount = coupon.Amount
        };
    }
}
