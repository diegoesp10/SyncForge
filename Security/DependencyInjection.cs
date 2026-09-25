using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Security.Authentication;
using Security.Identity;
using Security.Persistence;
using Security.Resources;
using Security.Services;
using Security.Email;

namespace Security;

public static class DependencyInjection
{
    public const string LoginRateLimitPolicy = "auth-login";
    public const string RegistrationRateLimitPolicy = "auth-registration";

    public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SyncForgeAuth");
        var encodedKey = configuration["Security:SigningKey"];
        byte[] key;
        try { key = Convert.FromBase64String(encodedKey ?? string.Empty); }
        catch (FormatException) { key = []; }
        if (string.IsNullOrWhiteSpace(connectionString) || key.Length < 32)
            throw new InvalidOperationException(SecurityErrorMessages.Get(SecurityErrorCode.SecurityConfigurationMissing, "en"));

        var lifetimeHours = configuration.GetValue<int?>("Security:AccessTokenLifetimeHours") ?? 24;
        if (lifetimeHours is < 1 or > 24)
            throw new InvalidOperationException(SecurityErrorMessages.Get(SecurityErrorCode.SecurityConfigurationMissing, "en"));

        var tokenOptions = new LocalTokenOptions
        {
            Issuer = configuration["Security:Issuer"] ?? "SyncForge.Local",
            Audience = configuration["Security:Audience"] ?? "SyncForge.Api",
            SigningKey = key,
            Lifetime = TimeSpan.FromHours(lifetimeHours)
        };
        services.AddSingleton(tokenOptions);
        services.AddDataProtection();
        services.AddDbContext<SecurityDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Password.RequiredLength = 12;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<SecurityDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();
        services.Configure<DataProtectionTokenProviderOptions>(options =>
            options.TokenLifespan = TimeSpan.FromHours(24));
        services.Configure<SmtpEmailOptions>(configuration.GetSection("Email:Smtp"));
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<ISecurityService, SecurityService>();
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<SuperAdminCreationService>();
        services.AddSingleton<LocalTokenIssuer>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = tokenOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = tokenOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(tokenOptions.SigningKey),
                    RequireSignedTokens = true,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = JwtRegisteredClaimNames.Sub
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        if (!Guid.TryParse(context.Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var userId)
                            || !Guid.TryParse(context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value, out var sessionId))
                        {
                            context.Fail(SecurityErrorMessages.Get(SecurityErrorCode.AuthenticationRequired, "en"));
                            return;
                        }

                        var db = context.HttpContext.RequestServices.GetRequiredService<SecurityDbContext>();
                        var session = await db.AuthSessions.AsNoTracking().Include(item => item.User)
                            .SingleOrDefaultAsync(item => item.Id == sessionId && item.UserId == userId);
                        var now = context.HttpContext.RequestServices.GetRequiredService<TimeProvider>().GetUtcNow();
                        if (session is null || session.RevokedAt is not null || session.ExpiresAt <= now
                            || !session.User.IsActive || !session.User.EmailConfirmed
                            || session.SecurityStamp != session.User.SecurityStamp)
                        {
                            context.Fail(SecurityErrorMessages.Get(SecurityErrorCode.AuthenticationRequired, "en"));
                            return;
                        }

                        var users = context.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();
                        var roles = await users.GetRolesAsync(session.User);
                        if (roles.Count == 0)
                        {
                            context.Fail(SecurityErrorMessages.Get(SecurityErrorCode.AuthenticationRequired, "en"));
                            return;
                        }
                        if (context.Principal?.Identity is ClaimsIdentity identity)
                        {
                            foreach (var claim in identity.FindAll(ClaimTypes.Role).ToArray())
                                identity.RemoveClaim(claim);
                            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
                            foreach (var role in roles)
                                identity.AddClaim(new Claim(ClaimTypes.Role, role));
                        }
                    },
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.Headers.WWWAuthenticate = "Bearer";
                        await WriteProblemAsync(context.HttpContext, 401, SecurityErrorCode.AuthenticationRequired);
                    },
                    OnForbidden = context => WriteProblemAsync(
                        context.HttpContext, 403, SecurityErrorCode.AccessDenied)
                };
            });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireRole(AppRoles.All)
                .Build();
        });
        services.AddRateLimiter(options =>
        {
            options.AddPolicy(LoginRateLimitPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
            options.AddPolicy(RegistrationRateLimitPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 3,
                    Window = TimeSpan.FromMinutes(10),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
            options.OnRejected = (context, _) =>
                new ValueTask(WriteProblemAsync(context.HttpContext, 429,
                    context.HttpContext.Request.Path.StartsWithSegments("/api/auth/login")
                        ? SecurityErrorCode.TooManyLoginAttempts
                        : SecurityErrorCode.TooManyRegistrationAttempts));
        });
        return services;
    }

    private static async Task WriteProblemAsync(HttpContext context, int status, SecurityErrorCode code)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        var detail = SecurityErrorMessages.Get(code, SecurityLanguage.Resolve(context));
        await JsonSerializer.SerializeAsync(context.Response.Body, new
        {
            type = "about:blank",
            title = detail,
            status,
            detail
        });
    }
}
