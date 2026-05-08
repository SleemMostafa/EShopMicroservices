namespace Identity.API.Security;

public interface IJwtTokenService
{
    string CreateAccessToken(ApplicationUser user);
    string CreateRefreshToken();
    string HashRefreshToken(string refreshToken);
    DateTimeOffset GetRefreshTokenExpiresAt(DateTimeOffset now);
}
