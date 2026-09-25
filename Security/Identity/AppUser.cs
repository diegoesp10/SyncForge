using Microsoft.AspNetCore.Identity;

namespace Security.Identity;

public sealed class AppUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastSignedInAt { get; set; }
    public ICollection<AuthSession> Sessions { get; set; } = new List<AuthSession>();

    // An Entra account can be linked later by stable tenant and object IDs.
    public Guid? EntraTenantId { get; set; }
    public Guid? EntraObjectId { get; set; }
}
