using AWQP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AWQP.Infrastructure.Persistence.Configurations;

public sealed class CheckPointMasterConfiguration : IEntityTypeConfiguration<CheckPointMaster>
{
    public void Configure(EntityTypeBuilder<CheckPointMaster> builder)
    {
        builder.HasIndex(x => x.CheckPointName).IsUnique();
        builder.Property(x => x.CheckPointName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(512);
        builder.Property(x => x.MinimumSpecs).HasMaxLength(128);
        builder.Property(x => x.MaximumSpecs).HasMaxLength(128);
    }
}

public sealed class CheckSheetDefinitionConfiguration : IEntityTypeConfiguration<CheckSheetDefinition>
{
    public void Configure(EntityTypeBuilder<CheckSheetDefinition> builder)
    {
        builder.Property(x => x.DepartmentCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.DepartmentName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.SectionCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.SectionName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.MachineName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Frequency).HasMaxLength(64).IsRequired();
        builder.Property(x => x.DocumentControlNo).HasMaxLength(64);
        builder.Property(x => x.CheckSheetDisplayName).HasMaxLength(256);
        builder.HasMany(x => x.TemplateItems).WithOne(x => x.CheckSheetDefinition).HasForeignKey(x => x.CheckSheetDefinitionId);
    }
}

public sealed class CheckSheetTemplateItemConfiguration : IEntityTypeConfiguration<CheckSheetTemplateItem>
{
    public void Configure(EntityTypeBuilder<CheckSheetTemplateItem> builder)
    {
        builder.Property(x => x.Items).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Standard).HasMaxLength(256);
        builder.Property(x => x.CheckPoint).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Abnormality).HasMaxLength(256);
    }
}

public sealed class CheckSheetDataConfiguration : IEntityTypeConfiguration<CheckSheetData>
{
    public void Configure(EntityTypeBuilder<CheckSheetData> builder)
    {
        builder.ToTable("CheckSheetData", "ehs");
        builder.Property(x => x.DepartmentCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.DepartmentName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.SectionCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.SectionName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.MachineName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Frequency).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Items).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Standard).HasMaxLength(256);
        builder.Property(x => x.CheckPoint).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Abnormality).HasMaxLength(256);
        builder.Property(x => x.SpecsMinimum).HasMaxLength(128);
        builder.Property(x => x.SpecsMaximum).HasMaxLength(128);
        builder.Property(x => x.ActualSpecs).HasMaxLength(128);
        builder.Property(x => x.Image).HasMaxLength(512);
        builder.Property(x => x.Image1).HasMaxLength(512);
        builder.Property(x => x.Image2).HasMaxLength(512);
        builder.Property(x => x.Image3).HasMaxLength(512);
        builder.Property(x => x.Image4).HasMaxLength(512);
        builder.Property(x => x.Priority).HasMaxLength(32);
        builder.Property(x => x.Status).HasMaxLength(16);
        builder.Property(x => x.AuditCategory).HasMaxLength(128);
        builder.Property(x => x.CreatedByName).HasMaxLength(128);
        builder.HasIndex(x => new { x.DepartmentCode, x.SectionCode, x.MachineName, x.Frequency, x.CheckingDate });
    }
}
