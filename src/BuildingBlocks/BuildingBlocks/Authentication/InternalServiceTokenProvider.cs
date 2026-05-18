using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BuildingBlocks.Authentication;

public sealed class InternalServiceTokenProvider(
    IOptions<JwtOptions> jwtOptions,
    IOptions<InternalServiceAuthOptions> internalServiceOptions,
    IEshopClock clock)
    : IInternalServiceTokenProvider
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private readonly InternalServiceAuthOptions _internalServiceOptions = internalServiceOptions.Value;

    public string CreateToken(IEnumerable<string> scopes)
    {
        if (string.IsNullOrWhiteSpace(_internalServiceOptions.ServiceName))
        {
            throw new InvalidOperationException("Internal service name is not configured.");
        }

        var now = clock.DateTimeOffset;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, _internalServiceOptions.ServiceName),
            new(InternalServiceClaims.ClientId, _internalServiceOptions.ServiceName),
            new(InternalServiceClaims.Service, _internalServiceOptions.ServiceName),
            new(InternalServiceClaims.TokenUse, InternalServiceClaims.ServiceTokenUse)
        };

        claims.AddRange(scopes
            .Where(scope => !string.IsNullOrWhiteSpace(scope))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(scope => new Claim(InternalServiceClaims.Scope, scope)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: now.AddMinutes(_internalServiceOptions.TokenLifetimeMinutes).UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
