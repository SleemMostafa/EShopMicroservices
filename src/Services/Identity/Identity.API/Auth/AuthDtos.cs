namespace Identity.API.Auth;

public sealed record AuthResponse(
    Guid UserId,
    string UserName,
    string Email,
    string AccessToken,
    string RefreshToken);

public sealed record RegisterRequest(
    string UserName,
    string Email,
    string Password);

public sealed record LoginRequest(
    string Email,
    string Password);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record RevokeRefreshTokenRequest(string RefreshToken);

public sealed record RevokeRefreshTokenResponse(bool IsSuccess);

public sealed record CurrentUserResponse(
    string UserId,
    string? UserName,
    string? Email);
