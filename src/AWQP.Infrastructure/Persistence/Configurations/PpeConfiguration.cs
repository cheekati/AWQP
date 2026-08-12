using AWQP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AWQP.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasIndex(x => x.EmployeeNumber).IsUnique();
        builder.Property(x => x.EmployeeNumber).HasMaxLength(32).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Department).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Designation).HasMaxLength(64).IsRequired();
    }
}

public sealed class PpeItemConfiguration : IEntityTypeConfiguration<PpeItem>
{
    public void Configure(EntityTypeBuilder<PpeItem> builder)
    {
        builder.HasIndex(x => x.ItemCode).IsUnique();
        builder.Property(x => x.ItemCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(64).IsRequired();
        builder.Property(x => x.UnitOfMeasure).HasMaxLength(16).IsRequired();
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.HasMany(x => x.StockEntries).WithOne(x => x.PpeItem).HasForeignKey(x => x.PpeItemId);
        builder.HasMany(x => x.Requests).WithOne(x => x.PpeItem).HasForeignKey(x => x.PpeItemId);
    }
}

public sealed class PpeRequestConfiguration : IEntityTypeConfiguration<PpeRequest>
{
    public void Configure(EntityTypeBuilder<PpeRequest> builder)
    {
        builder.HasIndex(x => x.RequestNumber).IsUnique();
        builder.Property(x => x.RequestNumber).HasMaxLength(32).IsRequired();
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId);
        builder.HasOne(x => x.Issue).WithOne(x => x.PpeRequest).HasForeignKey<PpeIssue>(x => x.PpeRequestId);
    }
}

public sealed class PpeIssueConfiguration : IEntityTypeConfiguration<PpeIssue>
{
    public void Configure(EntityTypeBuilder<PpeIssue> builder)
    {
        builder.HasIndex(x => x.PpeRequestId).IsUnique();
        builder.Property(x => x.IssuedBy).HasMaxLength(128).IsRequired();
    }
}

public sealed class PpeStockEntryConfiguration : IEntityTypeConfiguration<PpeStockEntry>
{
    public void Configure(EntityTypeBuilder<PpeStockEntry> builder)
    {
        builder.Property(x => x.Remarks).HasMaxLength(256);
    }
}
