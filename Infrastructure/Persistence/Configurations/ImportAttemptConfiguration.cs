using Domain.Imports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ImportAttemptConfiguration : IEntityTypeConfiguration<ImportAttempt>
{
    public void Configure(EntityTypeBuilder<ImportAttempt> builder)
    {
        builder.ToTable("ImportAttempts");
        builder.HasKey(attempt => attempt.Id);
        builder.Property(attempt => attempt.FailureCode).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(attempt => new { attempt.ImportJobId, attempt.Number }).IsUnique();
    }
}
