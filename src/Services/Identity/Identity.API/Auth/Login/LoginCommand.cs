namespace Identity.API.Auth.Login;

public sealed record LoginCommand(
    string Email,
    string Password) : ICommand<AuthResponse>;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(command => command.Password)
            .NotEmpty();
    }
}

internal sealed class LoginCommandHandler(
    IdentityDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenService tokenService,
    IEshopClock clock)
    : ICommandHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

        if (user is null || !passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            throw new BadRequestException("Login failed.", "Invalid email or password.");
        }

        var refreshToken = tokenService.CreateRefreshToken();
        dbContext.RefreshTokens.Add(UserRefreshToken.Create(
            user.Id,
            tokenService.HashRefreshToken(refreshToken),
            clock.DateTimeOffset,
            tokenService.GetRefreshTokenExpiresAt(clock.DateTimeOffset)));

        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            user.Id,
            user.UserName,
            user.Email,
            tokenService.CreateAccessToken(user),
            refreshToken);
    }
}
