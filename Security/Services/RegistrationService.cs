using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Security.Contracts;
using Security.Email;
using Security.Identity;
using Security.Persistence;
using Security.Resources;

namespace Security.Services;

public sealed class RegistrationService(
    UserManager<AppUser> users,
    SecurityDbContext db,
    IEmailSender sender,
    TimeProvider clock) : IRegistrationService
{
    public async Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        EnsureMailConfigured();
        if (string.IsNullOrWhiteSpace(request.Email) || !new EmailAddressAttribute().IsValid(request.Email)
            || string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Trim().Length > 120)
            throw new SecurityOperationException(SecurityErrorCode.InvalidUserRequest, 400);
        if (string.IsNullOrWhiteSpace(request.Password))
            throw new SecurityOperationException(SecurityErrorCode.PasswordPolicyViolation, 400);

        var email = request.Email.Trim();
        if (await users.FindByEmailAsync(email) is not null)
            return; // Keep the public response identical for existing accounts.

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            DisplayName = request.DisplayName.Trim(),
            CreatedAt = clock.GetUtcNow(),
            EmailConfirmed = false
        };
        await using (var transaction = await db.Database.BeginTransactionAsync(cancellationToken))
        {
            var created = await users.CreateAsync(user, request.Password);
            if (!created.Succeeded)
                throw new SecurityOperationException(
                    created.Errors.Any(error => error.Code.Contains("Password", StringComparison.OrdinalIgnoreCase))
                        ? SecurityErrorCode.PasswordPolicyViolation : SecurityErrorCode.InvalidUserRequest, 400);
            var assigned = await users.AddToRoleAsync(user, AppRoles.User);
            if (!assigned.Succeeded)
                throw new SecurityOperationException(SecurityErrorCode.InvalidRole, 400);
            await transaction.CommitAsync(cancellationToken);
        }
        await SendConfirmationAsync(user, cancellationToken);
    }

    public async Task ResendConfirmationAsync(ResendConfirmationRequest request, CancellationToken cancellationToken)
    {
        EnsureMailConfigured();
        if (string.IsNullOrWhiteSpace(request.Email) || !new EmailAddressAttribute().IsValid(request.Email))
            throw new SecurityOperationException(SecurityErrorCode.InvalidUserRequest, 400);
        var user = await users.FindByEmailAsync(request.Email.Trim());
        if (user is { EmailConfirmed: false, IsActive: true })
            await SendConfirmationAsync(user, cancellationToken);
    }

    public async Task ConfirmEmailAsync(Guid userId, string token)
    {
        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(token))
            throw new SecurityOperationException(SecurityErrorCode.InvalidConfirmationToken, 400);
        var user = await users.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new SecurityOperationException(SecurityErrorCode.InvalidConfirmationToken, 400);
        if (user.EmailConfirmed)
            return;
        var confirmed = await users.ConfirmEmailAsync(user, token);
        if (!confirmed.Succeeded)
            throw new SecurityOperationException(SecurityErrorCode.InvalidConfirmationToken, 400);
    }

    private async Task SendConfirmationAsync(AppUser user, CancellationToken cancellationToken)
    {
        var token = await users.GenerateEmailConfirmationTokenAsync(user);
        await sender.SendConfirmationAsync(user.Email!, user.Id, token, cancellationToken);
    }

    private void EnsureMailConfigured()
    {
        if (!sender.IsConfigured)
            throw new SecurityOperationException(SecurityErrorCode.EmailDeliveryUnavailable, 503);
    }
}
