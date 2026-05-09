using BuildingBlocks.CQRS;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var validatorList = validators.ToList();

        if (validatorList.Count == 0)
        {
            logger.LogDebug("No validators registered for command {CommandName}", requestName);
            return await next();
        }

        logger.LogDebug(
            "Validating command {CommandName} with {ValidatorCount} validator(s)",
            requestName,
            validatorList.Count);

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validatorList.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count != 0)
        {
            logger.LogWarning(
                "Validation failed for command {CommandName} with {FailureCount} error(s): {ValidationErrors}",
                requestName,
                failures.Count,
                failures.Select(failure => new
                {
                    failure.PropertyName,
                    failure.ErrorMessage,
                    failure.ErrorCode
                }));

            throw new ValidationException(failures);
        }

        logger.LogDebug("Validation succeeded for command {CommandName}", requestName);

        return await next();
    }
}
