using Domain.Files;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.ToTable("StoredFiles");
        builder.HasKey(file => file.Id);
        builder.Property(file => file.FileName).IsRequired().HasMaxLength(260);
        builder.Property(file => file.ContentType).IsRequired().HasMaxLength(200);
        builder.Property(file => file.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(file => file.FailureCode).HasConversion<string>().HasMaxLength(32);
        builder.Property(file => file.ResultJson).HasColumnType("nvarchar(max)");
        builder.Property(file => file.Version).IsConcurrencyToken();
        builder.HasIndex(file => new { file.Status, file.UploadedAt });
        builder.HasIndex(file => file.UploadedAt);
    }
}
