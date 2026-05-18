using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Resilience;

public sealed class CorrelationIdDelegatingHandler(
    IHttpContextAccessor httpContextAccessor,
    IOptions<CorrelationIdOptions> options)
    : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var headerName = options.Value.HeaderName;
        var correlationId = httpContextAccessor.HttpContext?.TraceIdentifier;

        if (!string.IsNullOrWhiteSpace(correlationId) && !request.Headers.Contains(headerName))
        {
            request.Headers.TryAddWithoutValidation(headerName, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
