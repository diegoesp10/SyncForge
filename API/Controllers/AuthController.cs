using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Security;
using Security.Contracts;
using Security.Services;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ISecurityService service, IRegistrationService registration) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(DependencyInjection.RegistrationRateLimitPolicy)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        await registration.RegisterAsync(request, cancellationToken);
        return Accepted();
    }

    [HttpPost("resend-confirmation")]
    [AllowAnonymous]
    [EnableRateLimiting(DependencyInjection.RegistrationRateLimitPolicy)]
    public async Task<IActionResult> ResendConfirmation(
        [FromBody] ResendConfirmationRequest request, CancellationToken cancellationToken)
    {
        await registration.ResendConfirmationAsync(request, cancellationToken);
        return Accepted();
    }

    [HttpGet("confirm-email")]
    [AllowAnonymous]
    [EnableRateLimiting(DependencyInjection.RegistrationRateLimitPolicy)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string token)
    {
        await registration.ConfirmEmailAsync(userId, token);
        return NoContent();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(DependencyInjection.LoginRateLimitPolicy)]
    public async Task<AuthTokenResponse> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";
        return await service.LoginAsync(request, cancellationToken);
    }

    [HttpGet("me")]
    public Task<UserResponse> Me(CancellationToken cancellationToken) =>
        service.GetUserAsync(CurrentUserId(), cancellationToken);

    [HttpPost("onboarding/complete")]
    public async Task<IActionResult> CompleteOnboarding(CancellationToken cancellationToken)
    {
        await service.CompleteOnboardingAsync(CurrentUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await service.LogoutAsync(CurrentUserId(), Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Jti)!), cancellationToken);
        return NoContent();
    }

    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
