using AWQP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AWQP.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasIndex(x => x.CustomerCode).IsUnique();
        builder.Property(x => x.CustomerCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.HasMany(x => x.Contacts).WithOne(x => x.Customer).HasForeignKey(x => x.CustomerId);
        builder.HasMany(x => x.Addresses).WithOne(x => x.Customer).HasForeignKey(x => x.CustomerId);
        builder.HasMany(x => x.Industries).WithOne(x => x.Customer).HasForeignKey(x => x.CustomerId);
    }
}

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasIndex(x => x.ProductCode).IsUnique();
        builder.Property(x => x.ProductCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.OuterDiameterMm).HasPrecision(18, 4);
        builder.Property(x => x.InnerDiameterMm).HasPrecision(18, 4);
        builder.Property(x => x.LengthMm).HasPrecision(18, 4);
        builder.Property(x => x.WallThicknessMm).HasPrecision(18, 4);
        builder.HasMany(x => x.Revisions).WithOne(x => x.Product).HasForeignKey(x => x.ProductId);
    }
}

public sealed class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.HasIndex(x => new { x.Name, x.IndustrySegment }).IsUnique();
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
    }
}

public sealed class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.HasIndex(x => x.WorkOrderNumber).IsUnique();
        builder.HasIndex(x => new { x.BatchNumber, x.LotNumber });
        builder.Property(x => x.WorkOrderNumber).HasMaxLength(64).IsRequired();
        builder.Property(x => x.BatchNumber).HasMaxLength(64).IsRequired();
        builder.Property(x => x.LotNumber).HasMaxLength(64).IsRequired();
        builder.HasMany(x => x.Operations).WithOne(x => x.WorkOrder).HasForeignKey(x => x.WorkOrderId);
        builder.HasMany(x => x.ProductSerials).WithOne(x => x.WorkOrder).HasForeignKey(x => x.WorkOrderId);
    }
}

public sealed class ManufacturingOperationConfiguration : IEntityTypeConfiguration<ManufacturingOperation>
{
    public void Configure(EntityTypeBuilder<ManufacturingOperation> builder)
    {
        builder.HasIndex(x => new { x.WorkOrderId, x.Sequence }).IsUnique();
        builder.Ignore(x => x.YieldPercentage);
    }
}

public sealed class ProductSerialConfiguration : IEntityTypeConfiguration<ProductSerial>
{
    public void Configure(EntityTypeBuilder<ProductSerial> builder)
    {
        builder.HasIndex(x => x.SerialNumber).IsUnique();
        builder.Property(x => x.SerialNumber).HasMaxLength(96).IsRequired();
    }
}

public sealed class CleanRoomConfiguration : IEntityTypeConfiguration<CleanRoom>
{
    public void Configure(EntityTypeBuilder<CleanRoom> builder)
    {
        builder.HasIndex(x => x.CleanRoomCode).IsUnique();
        builder.Property(x => x.CleanRoomCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.TemperatureMinC).HasPrecision(6, 2);
        builder.Property(x => x.TemperatureMaxC).HasPrecision(6, 2);
        builder.Property(x => x.HumidityMinPercent).HasPrecision(6, 2);
        builder.Property(x => x.HumidityMaxPercent).HasPrecision(6, 2);
    }
}

public sealed class InventoryConfiguration : IEntityTypeConfiguration<InventoryStock>
{
    public void Configure(EntityTypeBuilder<InventoryStock> builder)
    {
        builder.HasIndex(x => new { x.ItemId, x.BinId, x.LotNumber }).IsUnique();
        builder.Property(x => x.QuantityOnHand).HasPrecision(18, 4);
    }
}
