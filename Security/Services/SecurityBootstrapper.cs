using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Security.Identity;
using Security.Persistence;
using Security.Resources;

namespace Security.Services;

public sealed class SecurityBootstrapper(
    SecurityDbContext db,
    UserManager<AppUser> users,
    RoleManager<IdentityRole<Guid>> roles,
    TimeProvider clock)
{
    public async Task CreateInitialSuperAdminAsync(string email, string displayName, string password)
    {
        if (!await db.Database.CanConnectAsync() || (await db.Database.GetPendingMigrationsAsync()).Any()
            || !await roles.RoleExistsAsync(AppRoles.SuperAdmin))
            throw new SecurityOperationException(SecurityErrorCode.SecurityDatabaseNotReady, 409);
        if ((await users.GetUsersInRoleAsync(AppRoles.SuperAdmin)).Count > 0)
            throw new SecurityOperationException(SecurityErrorCode.SuperAdminAlreadyExists, 409);
        if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email)
            || string.IsNullOrWhiteSpace(displayName) || displayName.Trim().Length > 120)
            throw new SecurityOperationException(SecurityErrorCode.InvalidUserRequest, 400);
        if (string.IsNullOrWhiteSpace(password))
            throw new SecurityOperationException(SecurityErrorCode.PasswordPolicyViolation, 400);
        if (await users.FindByEmailAsync(email.Trim()) is not null)
            throw new SecurityOperationException(SecurityErrorCode.UserAlreadyExists, 409);

        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = email.Trim(),
            UserName = email.Trim(),
            DisplayName = displayName.Trim(),
            CreatedAt = clock.GetUtcNow()
        };
        var created = await users.CreateAsync(user, password);
        if (!created.Succeeded)
            throw new SecurityOperationException(SecurityErrorCode.PasswordPolicyViolation, 400);
        var assigned = await users.AddToRoleAsync(user, AppRoles.SuperAdmin);
        if (!assigned.Succeeded)
            throw new SecurityOperationException(SecurityErrorCode.SecurityDatabaseNotReady, 409);
        await transaction.CommitAsync();
    }
}
