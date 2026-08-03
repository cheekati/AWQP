using AWQP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AWQP.Infrastructure.Persistence.Configurations;

public sealed class WorkPermitConfiguration : IEntityTypeConfiguration<WorkPermit>
{
    public void Configure(EntityTypeBuilder<WorkPermit> builder)
    {
        builder.HasIndex(x => x.PermitNumber).IsUnique();
        builder.HasIndex(x => x.SupplierCode);
        builder.HasIndex(x => x.Status);
        builder.Property(x => x.WorkType).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Department).HasMaxLength(32).IsRequired();
        builder.Property(x => x.DepartmentName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Section).HasMaxLength(32).IsRequired();
        builder.Property(x => x.SectionName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.PlantLocation).HasMaxLength(128).IsRequired();
        builder.Property(x => x.WorkInfo).HasMaxLength(256).IsRequired();
        builder.Property(x => x.SupplierCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.SupplierName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.PoNumber).HasMaxLength(64);
        builder.Property(x => x.InvoiceNumber).HasMaxLength(100);
        builder.Property(x => x.PoCost).HasPrecision(18, 2);
        builder.HasMany(x => x.DailyAtmosphericReadings)
            .WithOne(x => x.WorkPermit)
            .HasForeignKey(x => x.WorkPermitId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class DailyAtmosphericReadingConfiguration : IEntityTypeConfiguration<DailyAtmosphericReading>
{
    public void Configure(EntityTypeBuilder<DailyAtmosphericReading> builder)
    {
        builder.HasIndex(x => new { x.WorkPermitId, x.ReadingDate }).IsUnique();
        builder.Property(x => x.OxygenContentPercent).HasPrecision(5, 2);
        builder.Property(x => x.ToxicGasH2SPpm).HasPrecision(8, 2);
        builder.Property(x => x.CarbonMonoxidePpm).HasPrecision(8, 2);
        builder.Property(x => x.CombustibleGasLelPercent).HasPrecision(5, 2);
        builder.Property(x => x.PicName).HasMaxLength(128);
        builder.Property(x => x.Remarks).HasMaxLength(512);
    }
}

public sealed class AtmosphericParameterRangeConfiguration : IEntityTypeConfiguration<AtmosphericParameterRange>
{
    public void Configure(EntityTypeBuilder<AtmosphericParameterRange> builder)
    {
        builder.HasIndex(x => x.ParameterCode).IsUnique();
        builder.Property(x => x.ParameterCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.ParameterName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Unit).HasMaxLength(32).IsRequired();
        builder.Property(x => x.DisplayRange).HasMaxLength(64).IsRequired();
        builder.Property(x => x.MinAcceptable).HasPrecision(10, 2);
        builder.Property(x => x.MaxAcceptable).HasPrecision(10, 2);
    }
}
