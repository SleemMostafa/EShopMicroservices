namespace Identity.API.Auth.Register;

public sealed record RegisterCommand(
    string UserName,
    string Email,
    string Password) : ICommand<AuthResponse>;

public sealed class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(command => command.UserName)
            .NotEmpty()
            .Length(2, 100);

        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(command => command.Password)
            .NotEmpty()
            .MinimumLength(8);
    }
}

internal sealed class RegisterCommandHandler(
    IdentityDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenService tokenService,
    IEshopClock clock)
    : ICommandHandler<RegisterCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var userName = command.UserName.Trim();

        var userExists = await dbContext.Users
            .AnyAsync(user => user.Email == email || user.UserName == userName, cancellationToken);

        if (userExists)
        {
            throw new BadRequestException("Registration failed.", "User name or email is already registered.");
        }

        var user = ApplicationUser.Register(
            userName,
            email,
            passwordHasher.Hash(command.Password),
            clock.DateTimeOffset);

        var refreshToken = tokenService.CreateRefreshToken();
        var refreshTokenDocument = UserRefreshToken.Create(
            user.Id,
            tokenService.HashRefreshToken(refreshToken),
            clock.DateTimeOffset,
            tokenService.GetRefreshTokenExpiresAt(clock.DateTimeOffset));

        dbContext.Users.Add(user);
        dbContext.RefreshTokens.Add(refreshTokenDocument);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            user.Id,
            user.UserName,
            user.Email,
            tokenService.CreateAccessToken(user),
            refreshToken);
    }
}
