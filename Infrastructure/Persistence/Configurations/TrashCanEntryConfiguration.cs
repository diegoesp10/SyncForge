using Domain.Files;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class TrashCanEntryConfiguration : IEntityTypeConfiguration<TrashCanEntry>
{
    public void Configure(EntityTypeBuilder<TrashCanEntry> builder)
    {
        builder.ToTable("TrashCan");
        builder.HasKey(entry => entry.FileId);
        builder.HasOne(entry => entry.File)
            .WithOne()
            .HasForeignKey<TrashCanEntry>(entry => entry.FileId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(entry => entry.Version).IsConcurrencyToken();
        builder.HasIndex(entry => entry.PurgeAt);
    }
}
