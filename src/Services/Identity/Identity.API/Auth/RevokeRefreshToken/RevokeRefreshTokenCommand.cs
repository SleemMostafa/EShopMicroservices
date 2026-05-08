namespace Identity.API.Auth.RevokeRefreshToken;

public sealed record RevokeRefreshTokenCommand(string RefreshToken) : ICommand<RevokeRefreshTokenResponse>;

public sealed class RevokeRefreshTokenValidator : AbstractValidator<RevokeRefreshTokenCommand>
{
    public RevokeRefreshTokenValidator()
    {
        RuleFor(command => command.RefreshToken).NotEmpty();
    }
}

internal sealed class RevokeRefreshTokenCommandHandler(
    IdentityDbContext dbContext,
    IJwtTokenService tokenService,
    IEshopClock clock)
    : ICommandHandler<RevokeRefreshTokenCommand, RevokeRefreshTokenResponse>
{
    public async Task<RevokeRefreshTokenResponse> Handle(
        RevokeRefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var tokenHash = tokenService.HashRefreshToken(command.RefreshToken);
        var storedToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null)
        {
            return new RevokeRefreshTokenResponse(true);
        }

        storedToken.Revoke(clock.DateTimeOffset);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new RevokeRefreshTokenResponse(true);
    }
}
