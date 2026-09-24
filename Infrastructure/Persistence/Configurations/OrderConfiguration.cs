using Domain.Imports;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(order => order.Id);
        builder.Property(order => order.SourceSystem).IsRequired().HasMaxLength(200);
        builder.Property(order => order.ExternalId).IsRequired().HasMaxLength(200);
        builder.Property(order => order.CustomerName).IsRequired().HasMaxLength(200);
        builder.Property(order => order.Currency).IsRequired().HasMaxLength(3);
        builder.Property(order => order.Amount).HasPrecision(18, 2);
        builder.HasIndex(order => new { order.SourceSystem, order.ExternalId }).IsUnique();
        builder.HasOne<ImportJob>()
            .WithMany()
            .HasForeignKey(order => order.ImportJobId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
