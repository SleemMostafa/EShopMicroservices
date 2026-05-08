namespace Identity.API.Auth.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<AuthResponse>;

public sealed class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(command => command.RefreshToken).NotEmpty();
    }
}

internal sealed class RefreshTokenCommandHandler(
    IdentityDbContext dbContext,
    IJwtTokenService tokenService,
    IEshopClock clock)
    : ICommandHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var tokenHash = tokenService.HashRefreshToken(command.RefreshToken);
        var storedToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null || !storedToken.IsActive(clock.DateTimeOffset))
        {
            throw new BadRequestException("Refresh token failed.", "Refresh token is invalid or expired.");
        }

        var user = await dbContext.Users.FindAsync([storedToken.UserId], cancellationToken);
        if (user is null)
        {
            throw new BadRequestException("Refresh token failed.", "Refresh token user does not exist.");
        }

        storedToken.Revoke(clock.DateTimeOffset);

        var newRefreshToken = tokenService.CreateRefreshToken();
        dbContext.RefreshTokens.Add(UserRefreshToken.Create(
            user.Id,
            tokenService.HashRefreshToken(newRefreshToken),
            clock.DateTimeOffset,
            tokenService.GetRefreshTokenExpiresAt(clock.DateTimeOffset)));

        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            user.Id,
            user.UserName,
            user.Email,
            tokenService.CreateAccessToken(user),
            newRefreshToken);
    }
}
