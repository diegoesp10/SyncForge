using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Security.Identity;

namespace Security.Persistence;

public sealed class SecurityDbContext(DbContextOptions<SecurityDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<AuthSession> AuthSessions => Set<AuthSession>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(user => user.DisplayName).HasMaxLength(120).IsRequired();
            entity.HasIndex(user => user.NormalizedEmail)
                .IsUnique()
                .HasFilter("[NormalizedEmail] IS NOT NULL");
            entity.HasIndex(user => new { user.EntraTenantId, user.EntraObjectId })
                .IsUnique()
                .HasFilter("[EntraTenantId] IS NOT NULL AND [EntraObjectId] IS NOT NULL");
        });
        builder.Entity<IdentityRole<Guid>>(entity => entity.ToTable("Roles"));
        builder.Entity<IdentityUserRole<Guid>>(entity => entity.ToTable("UserRoles"));
        builder.Entity<IdentityUserClaim<Guid>>(entity => entity.ToTable("UserClaims"));
        builder.Entity<IdentityUserLogin<Guid>>(entity => entity.ToTable("UserLogins"));
        builder.Entity<IdentityRoleClaim<Guid>>(entity => entity.ToTable("RoleClaims"));
        builder.Entity<IdentityUserToken<Guid>>(entity => entity.ToTable("UserTokens"));

        builder.Entity<AuthSession>(entity =>
        {
            entity.ToTable("AuthSessions");
            entity.HasKey(session => session.Id);
            entity.Property(session => session.SecurityStamp).HasMaxLength(256).IsRequired();
            entity.HasOne(session => session.User).WithMany(user => user.Sessions)
                .HasForeignKey(session => session.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(session => session.ExpiresAt);
        });

        builder.Entity<IdentityRole<Guid>>().HasData(
            new IdentityRole<Guid>
            {
                Id = Guid.Parse("4c9868f3-10df-41ba-b33a-46ae2a021001"),
                Name = AppRoles.User,
                NormalizedName = AppRoles.User.ToUpperInvariant(),
                ConcurrencyStamp = "syncforge-user-role-v1"
            },
            new IdentityRole<Guid>
            {
                Id = Guid.Parse("4c9868f3-10df-41ba-b33a-46ae2a021002"),
                Name = AppRoles.Admin,
                NormalizedName = AppRoles.Admin.ToUpperInvariant(),
                ConcurrencyStamp = "syncforge-admin-role-v1"
            },
            new IdentityRole<Guid>
            {
                Id = Guid.Parse("4c9868f3-10df-41ba-b33a-46ae2a021003"),
                Name = AppRoles.SuperAdmin,
                NormalizedName = AppRoles.SuperAdmin.ToUpperInvariant(),
                ConcurrencyStamp = "syncforge-superadmin-role-v1"
            });

        builder.Entity<AppUser>().HasData(new AppUser
        {
            Id = InitialSuperAdmin.Id,
            UserName = InitialSuperAdmin.UserName,
            NormalizedUserName = "DIEGOESPINA",
            Email = InitialSuperAdmin.Email,
            NormalizedEmail = "DIEGOESPINARODRIGUEZ@GMAIL.COM",
            DisplayName = InitialSuperAdmin.UserName,
            CreatedAt = new DateTimeOffset(2026, 9, 25, 0, 0, 0, TimeSpan.Zero),
            IsActive = true,
            EmailConfirmed = false,
            LockoutEnabled = true,
            SecurityStamp = "syncforge-diego-pending-setup-v1",
            ConcurrencyStamp = "syncforge-diego-pending-setup-v1"
        });
        builder.Entity<IdentityUserRole<Guid>>().HasData(new IdentityUserRole<Guid>
        {
            UserId = InitialSuperAdmin.Id,
            RoleId = Guid.Parse("4c9868f3-10df-41ba-b33a-46ae2a021003")
        });
    }
}
