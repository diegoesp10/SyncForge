using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Security.Authentication;
using Security.Contracts;
using Security.Email;
using Security.Identity;
using Security.Persistence;
using Security.Resources;

namespace Security.Services;

public sealed class SecurityService(
    UserManager<AppUser> users,
    SignInManager<AppUser> signIn,
    SecurityDbContext db,
    LocalTokenIssuer tokens,
    LocalTokenOptions tokenOptions,
    IEmailSender sender,
    TimeProvider clock) : ISecurityService
{
    public async Task<AuthTokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new SecurityOperationException(SecurityErrorCode.InvalidCredentials, 401);

        var user = await users.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActive)
            throw new SecurityOperationException(SecurityErrorCode.InvalidCredentials, 401);

        var result = await signIn.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded || (await users.GetRolesAsync(user)).Count == 0)
            throw new SecurityOperationException(SecurityErrorCode.InvalidCredentials, 401);

        var now = clock.GetUtcNow();
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var isFirstLogin = await db.Users
            .Where(item => item.Id == user.Id && item.LastSignedInAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.LastSignedInAt, now), cancellationToken) == 1;
        user.LastSignedInAt = now;
        var session = new AuthSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SecurityStamp = user.SecurityStamp ?? string.Empty,
            CreatedAt = now,
            ExpiresAt = now.Add(tokenOptions.Lifetime)
        };
        user.Sessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        await db.AuthSessions.Where(item => item.ExpiresAt < now.AddDays(-1))
            .ExecuteDeleteAsync(cancellationToken);
        return tokens.Issue(user, session, isFirstLogin);
    }

    public async Task LogoutAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await db.AuthSessions.SingleOrDefaultAsync(
            item => item.Id == sessionId && item.UserId == userId, cancellationToken);
        if (session is null || session.RevokedAt is not null)
            return;
        session.RevokedAt = clock.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteOnboardingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var completedAt = clock.GetUtcNow();
        await db.Users.Where(user => user.Id == userId && user.OnboardingCompletedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(user => user.OnboardingCompletedAt, completedAt),
                cancellationToken);
    }

    public async Task<UserResponse> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await users.FindByIdAsync(userId.ToString())
            ?? throw new SecurityOperationException(SecurityErrorCode.UserNotFound, 404);
        return await ToResponseAsync(user);
    }

    public async Task<IReadOnlyList<UserResponse>> ListUsersAsync(CancellationToken cancellationToken = default)
    {
        var entries = await db.Users.AsNoTracking().OrderBy(user => user.Email).ToListAsync(cancellationToken);
        var result = new List<UserResponse>(entries.Count);
        foreach (var user in entries)
            result.Add(await ToResponseAsync(user));
        return result;
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (!sender.IsConfigured)
            throw new SecurityOperationException(SecurityErrorCode.EmailDeliveryUnavailable, 503);
        if (!IsAllowedManagedRole(request.Role))
            throw new SecurityOperationException(SecurityErrorCode.InvalidRole, 400);
        if (string.IsNullOrWhiteSpace(request.Email) || !new EmailAddressAttribute().IsValid(request.Email)
            || string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Trim().Length > 120)
            throw new SecurityOperationException(SecurityErrorCode.InvalidUserRequest, 400);
        if (string.IsNullOrWhiteSpace(request.Password))
            throw new SecurityOperationException(SecurityErrorCode.PasswordPolicyViolation, 400);
        if (await users.FindByEmailAsync(request.Email.Trim()) is not null)
            throw new SecurityOperationException(SecurityErrorCode.UserAlreadyExists, 409);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            DisplayName = request.DisplayName.Trim(),
            EmailConfirmed = false,
            CreatedAt = clock.GetUtcNow()
        };
        var created = await users.CreateAsync(user, request.Password);
        if (!created.Succeeded)
            throw new SecurityOperationException(
                created.Errors.Any(error => error.Code.Contains("Password", StringComparison.OrdinalIgnoreCase))
                    ? SecurityErrorCode.PasswordPolicyViolation : SecurityErrorCode.InvalidUserRequest, 400);
        var assigned = await users.AddToRoleAsync(user, request.Role);
        if (!assigned.Succeeded)
            throw new SecurityOperationException(SecurityErrorCode.InvalidRole, 400);
        await transaction.CommitAsync(cancellationToken);
        var confirmationToken = await users.GenerateEmailConfirmationTokenAsync(user);
        await sender.SendConfirmationAsync(user.Email!, user.Id, confirmationToken, cancellationToken);
        return await ToResponseAsync(user);
    }

    public async Task<UserResponse> ChangeRoleAsync(Guid userId, ChangeUserRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsAllowedManagedRole(request.Role))
            throw new SecurityOperationException(SecurityErrorCode.InvalidRole, 400);
        var user = await users.FindByIdAsync(userId.ToString())
            ?? throw new SecurityOperationException(SecurityErrorCode.UserNotFound, 404);
        var existing = await users.GetRolesAsync(user);
        if (existing.Contains(AppRoles.SuperAdmin))
            throw new SecurityOperationException(SecurityErrorCode.InvalidRole, 403);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        if (existing.Count > 0)
            Ensure(await users.RemoveFromRolesAsync(user, existing), SecurityErrorCode.InvalidRole);
        Ensure(await users.AddToRoleAsync(user, request.Role), SecurityErrorCode.InvalidRole);
        Ensure(await users.UpdateSecurityStampAsync(user), SecurityErrorCode.InvalidUserRequest);
        await transaction.CommitAsync(cancellationToken);
        return await ToResponseAsync(user);
    }

    public async Task<UserResponse> SetStatusAsync(Guid userId, SetUserStatusRequest request, CancellationToken cancellationToken = default)
    {
        if (request.IsActive is null)
            throw new SecurityOperationException(SecurityErrorCode.InvalidUserRequest, 400);
        var user = await users.FindByIdAsync(userId.ToString())
            ?? throw new SecurityOperationException(SecurityErrorCode.UserNotFound, 404);
        if (await users.IsInRoleAsync(user, AppRoles.SuperAdmin))
            throw new SecurityOperationException(SecurityErrorCode.InvalidRole, 403);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        user.IsActive = request.IsActive.Value;
        Ensure(await users.UpdateAsync(user), SecurityErrorCode.InvalidUserRequest);
        Ensure(await users.UpdateSecurityStampAsync(user), SecurityErrorCode.InvalidUserRequest);
        await transaction.CommitAsync(cancellationToken);
        return await ToResponseAsync(user);
    }

    private async Task<UserResponse> ToResponseAsync(AppUser user) =>
        new(user.Id, user.Email ?? string.Empty, user.DisplayName, user.IsActive,
            user.EmailConfirmed, (await users.GetRolesAsync(user)).ToArray(),
            user.LastSignedInAt, user.OnboardingCompletedAt is null);

    private static bool IsAllowedManagedRole(string? role) => role is AppRoles.User or AppRoles.Admin;

    private static void Ensure(IdentityResult result, SecurityErrorCode code)
    {
        if (!result.Succeeded)
            throw new SecurityOperationException(code, 400);
    }
}
