using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Security.Identity;
using Security.Persistence;
using Security.Resources;

namespace Security.Services;

public sealed class SuperAdminCreationService(SecurityDbContext db, UserManager<AppUser> users)
{
    public async Task SetInitialPasswordAsync(string password)
    {
        if (!await db.Database.CanConnectAsync() || (await db.Database.GetPendingMigrationsAsync()).Any())
            throw new SecurityOperationException(SecurityErrorCode.SecurityDatabaseNotReady, 409);

        var user = await users.FindByIdAsync(InitialSuperAdmin.Id.ToString());
        if (user is null || !await users.IsInRoleAsync(user, AppRoles.SuperAdmin))
            throw new SecurityOperationException(SecurityErrorCode.SecurityDatabaseNotReady, 409);
        if (user.PasswordHash is not null)
            throw new SecurityOperationException(SecurityErrorCode.SuperAdminAlreadyExists, 409);
        if (string.IsNullOrWhiteSpace(password))
            throw new SecurityOperationException(SecurityErrorCode.PasswordPolicyViolation, 400);

        var result = await users.AddPasswordAsync(user, password);
        if (!result.Succeeded)
            throw new SecurityOperationException(SecurityErrorCode.PasswordPolicyViolation, 400);
    }
}
