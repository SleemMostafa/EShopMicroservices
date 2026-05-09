using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Basket.API.Basket.GetBasket;

public record GetBasketQuery(string UserName) : IQuery<GetBasketResult>;
public record GetBasketResult(ShoppingCart Cart);

public sealed class GetBasketQueryValidator : AbstractValidator<GetBasketQuery>
{
    public GetBasketQueryValidator()
    {
        RuleFor(query => query.UserName).NotEmpty().WithMessage("UserName is required");
    }
}

public sealed class GetBasketQueryHandler(IDocumentSession session, IDistributedCache cache, ILogger<GetBasketQueryHandler> logger)
    : IQueryHandler<GetBasketQuery, GetBasketResult>
{
    public async Task<GetBasketResult> Handle(GetBasketQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting basket for {UserName}", query.UserName);

        var cachedBasket = await cache.GetStringAsync(query.UserName, cancellationToken);
        if (!string.IsNullOrWhiteSpace(cachedBasket))
        {
            var cachedDto = JsonSerializer.Deserialize<ShoppingCartDto>(cachedBasket)!;
            return new GetBasketResult(BasketMapper.ToDomain(cachedDto));
        }

        var basket = await session.LoadAsync<ShoppingCart>(query.UserName, cancellationToken)
                     ?? throw new BasketNotFoundException("Basket not found");

        await cache.SetStringAsync(
            query.UserName,
            JsonSerializer.Serialize(BasketMapper.ToDto(basket)),
            cancellationToken);

        return new GetBasketResult(basket);
    }
}
