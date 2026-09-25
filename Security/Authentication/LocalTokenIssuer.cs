using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Security.Contracts;
using Security.Identity;

namespace Security.Authentication;

public sealed class LocalTokenIssuer(LocalTokenOptions options)
{
    public AuthTokenResponse Issue(AppUser user, AuthSession session, bool isFirstLogin)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, session.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
        };
        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: session.CreatedAt.UtcDateTime,
            expires: session.ExpiresAt.UtcDateTime,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(options.SigningKey), SecurityAlgorithms.HmacSha256));
        return new AuthTokenResponse(
            new JwtSecurityTokenHandler().WriteToken(token), "Bearer", session.ExpiresAt,
            isFirstLogin, user.OnboardingCompletedAt is null);
    }
}
