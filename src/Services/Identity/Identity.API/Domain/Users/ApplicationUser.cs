namespace Identity.API.Domain.Users;

public sealed class ApplicationUser : Aggregate<Guid>
{
    private ApplicationUser()
    {
    }

    private ApplicationUser(Guid id, string userName, string email, string passwordHash, DateTimeOffset createdAt)
    {
        Id = id;
        UserName = userName;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = createdAt;
    }

    public string UserName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }

    public static ApplicationUser Register(
        string userName,
        string email,
        string passwordHash,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new BadRequestException("Invalid user data.", "User name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new BadRequestException("Invalid user data.", "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new BadRequestException("Invalid user data.", "Password hash is required.");
        }

        return new ApplicationUser(
            Guid.NewGuid(),
            userName.Trim(),
            email.Trim().ToLowerInvariant(),
            passwordHash,
            createdAt);
    }
}
