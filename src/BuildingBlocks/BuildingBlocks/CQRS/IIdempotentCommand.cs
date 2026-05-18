namespace BuildingBlocks.CQRS;

public interface IIdempotentCommand<out TResponse> : ICommand<TResponse>
{
    string? IdempotencyKey { get; }
}
