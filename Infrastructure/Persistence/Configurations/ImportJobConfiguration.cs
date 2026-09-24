using Domain.Imports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ImportJobConfiguration : IEntityTypeConfiguration<ImportJob>
{
    public void Configure(EntityTypeBuilder<ImportJob> builder)
    {
        builder.ToTable("ImportJobs");
        builder.HasKey(job => job.Id);
        builder.Property(job => job.SourceSystem).IsRequired().HasMaxLength(200);
        builder.Property(job => job.FileName).IsRequired().HasMaxLength(260);
        builder.Property(job => job.StoredFileKey).IsRequired().HasMaxLength(512);
        builder.Property(job => job.Format).HasConversion<string>().HasMaxLength(20);
        builder.Property(job => job.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(job => job.FailureCode).HasConversion<string>().HasMaxLength(32);
        builder.Property(job => job.Version).IsConcurrencyToken();
        builder.HasMany(job => job.Attempts)
            .WithOne()
            .HasForeignKey(attempt => attempt.ImportJobId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(job => job.Attempts).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(job => new { job.Status, job.CreatedAt });
    }
}
