using BuildingBlocks.Authentication;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Options;

namespace Basket.API.Infrastructure.Grpc;

public sealed class InternalServiceAuthInterceptor(
    IInternalServiceTokenProvider tokenProvider,
    IOptions<InternalServiceAuthOptions> options)
    : Interceptor
{
    private readonly InternalServiceAuthOptions _options = options.Value;

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var headers = context.Options.Headers ?? new Metadata();
        headers.Add("Authorization", $"Bearer {tokenProvider.CreateToken(_options.Scopes)}");

        var callOptions = context.Options.WithHeaders(headers);
        var securedContext = new ClientInterceptorContext<TRequest, TResponse>(
            context.Method,
            context.Host,
            callOptions);

        return continuation(request, securedContext);
    }
}
