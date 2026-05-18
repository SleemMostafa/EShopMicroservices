using FluentValidation;

namespace Ordering.Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand(OrderDto Order, string? IdempotencyKey = null)
    : IIdempotentCommand<CreateOrderResult>;

public record CreateOrderResult(Guid Id);

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Order.OrderName).NotEmpty();
        RuleFor(x => x.Order.CustomerId).NotNull();
        RuleFor(x => x.Order.OrderItems).NotEmpty();
    }
}
