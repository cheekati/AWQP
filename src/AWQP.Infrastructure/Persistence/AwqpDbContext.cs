using System.Text.Json;
using AWQP.Application.Common;
using AWQP.Domain.CleanRoom;
using AWQP.Domain.Common;
using AWQP.Domain.Customers;
using AWQP.Domain.Documents;
using AWQP.Domain.Inventory;
using AWQP.Domain.Logistics;
using AWQP.Domain.Production;
using AWQP.Domain.Products;
using AWQP.Domain.Quality;
using AWQP.Domain.Sales;
using AWQP.Domain.Traceability;
using AWQP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AWQP.Infrastructure.Persistence;

public sealed class AwqpDbContext(DbContextOptions<AwqpDbContext> options, ICurrentUserService? currentUser = null)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerContact> CustomerContacts => Set<CustomerContact>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<CustomerIndustry> CustomerIndustries => Set<CustomerIndustry>();
    public DbSet<CustomerDocument> CustomerDocuments => Set<CustomerDocument>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<MaterialGrade> MaterialGrades => Set<MaterialGrade>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductSpecification> ProductSpecifications => Set<ProductSpecification>();
    public DbSet<ProductDimension> ProductDimensions => Set<ProductDimension>();
    public DbSet<ProductRevision> ProductRevisions => Set<ProductRevision>();
    public DbSet<EngineeringDrawing> EngineeringDrawings => Set<EngineeringDrawing>();
    public DbSet<EngineeringChangeRequest> EngineeringChangeRequests => Set<EngineeringChangeRequest>();
    public DbSet<RequestForQuote> RequestForQuotes => Set<RequestForQuote>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
    public DbSet<SalesApproval> SalesApprovals => Set<SalesApproval>();
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<ProductionSchedule> ProductionSchedules => Set<ProductionSchedule>();
    public DbSet<ResourceAllocation> ResourceAllocations => Set<ResourceAllocation>();
    public DbSet<ManufacturingOperation> ManufacturingOperations => Set<ManufacturingOperation>();
    public DbSet<InspectionRecord> InspectionRecords => Set<InspectionRecord>();
    public DbSet<InspectionParameterResult> InspectionParameterResults => Set<InspectionParameterResult>();
    public DbSet<Defect> Defects => Set<Defect>();
    public DbSet<NonConformanceReport> NonConformanceReports => Set<NonConformanceReport>();
    public DbSet<CorrectivePreventiveAction> CorrectivePreventiveActions => Set<CorrectivePreventiveAction>();
    public DbSet<RootCauseAnalysis> RootCauseAnalyses => Set<RootCauseAnalysis>();
    public DbSet<CleanRoomArea> CleanRoomAreas => Set<CleanRoomArea>();
    public DbSet<CleanRoomEnvironmentalReading> CleanRoomEnvironmentalReadings => Set<CleanRoomEnvironmentalReading>();
    public DbSet<CleanRoomAccessLog> CleanRoomAccessLogs => Set<CleanRoomAccessLog>();
    public DbSet<EnvironmentalComplianceRecord> EnvironmentalComplianceRecords => Set<EnvironmentalComplianceRecord>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<WarehouseLocation> WarehouseLocations => Set<WarehouseLocation>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<VacuumPackagingRecord> VacuumPackagingRecords => Set<VacuumPackagingRecord>();
    public DbSet<ShippingInspection> ShippingInspections => Set<ShippingInspection>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentLine> ShipmentLines => Set<ShipmentLine>();
    public DbSet<ProductSerial> ProductSerials => Set<ProductSerial>();
    public DbSet<ProductGenealogyLink> ProductGenealogyLinks => Set<ProductGenealogyLink>();
    public DbSet<TraceabilityEvent> TraceabilityEvents => Set<TraceabilityEvent>();
    public DbSet<ManagedDocument> ManagedDocuments => Set<ManagedDocument>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<UserActivityAudit> UserActivityAudits => Set<UserActivityAudit>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("erp");
        builder.Entity<ApplicationUser>().ToTable("Users", "auth");
        builder.Entity<IdentityRole<Guid>>().ToTable("Roles", "auth");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles", "auth");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims", "auth");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins", "auth");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims", "auth");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens", "auth");
        builder.Entity<RefreshToken>().ToTable("RefreshTokens", "auth");

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties().Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(4);
            }

        }

        ConfigureCommon(builder);
        ConfigureIndexes(builder);
        ConfigureRelationships(builder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = BuildAuditEntries();
        var saved = await base.SaveChangesAsync(cancellationToken);
        if (auditEntries.Count > 0)
        {
            AuditLogs.AddRange(auditEntries);
            await base.SaveChangesAsync(cancellationToken);
        }

        return saved;
    }

    private void ConfigureCommon(ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes().Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
        {
            builder.Entity(entityType.ClrType).Property(nameof(BaseEntity.Id)).ValueGeneratedNever();
            builder.Entity(entityType.ClrType).Property(nameof(BaseEntity.CreatedBy)).HasMaxLength(128);
            builder.Entity(entityType.ClrType).Property(nameof(BaseEntity.ModifiedBy)).HasMaxLength(128);
            builder.Entity(entityType.ClrType).Property(nameof(BaseEntity.RowVersion)).IsRowVersion();
        }
    }

    private static void ConfigureIndexes(ModelBuilder builder)
    {
        builder.Entity<Customer>().HasIndex(x => x.Code).IsUnique();
        builder.Entity<Product>().HasIndex(x => x.Code).IsUnique();
        builder.Entity<MaterialGrade>().HasIndex(x => x.Code).IsUnique();
        builder.Entity<RequestForQuote>().HasIndex(x => x.RfqNumber).IsUnique();
        builder.Entity<Quotation>().HasIndex(x => x.QuotationNumber).IsUnique();
        builder.Entity<SalesOrder>().HasIndex(x => x.SalesOrderNumber).IsUnique();
        builder.Entity<WorkOrder>().HasIndex(x => x.WorkOrderNumber).IsUnique();
        builder.Entity<WorkOrder>().HasIndex(x => new { x.BatchNumber, x.LotNumber });
        builder.Entity<InspectionRecord>().HasIndex(x => x.InspectionNumber).IsUnique();
        builder.Entity<InventoryItem>().HasIndex(x => x.ItemNumber).IsUnique();
        builder.Entity<InventoryItem>().HasIndex(x => new { x.BatchNumber, x.LotNumber });
        builder.Entity<Shipment>().HasIndex(x => x.ShipmentNumber).IsUnique();
        builder.Entity<ProductSerial>().HasIndex(x => x.SerialNumber).IsUnique();
        builder.Entity<TraceabilityEvent>().HasIndex(x => new { x.ProductSerialId, x.EventAt });
        builder.Entity<CleanRoomEnvironmentalReading>().HasIndex(x => new { x.CleanRoomAreaId, x.RecordedAt });
    }

    private static void ConfigureRelationships(ModelBuilder builder)
    {
        builder.Entity<ProductGenealogyLink>()
            .HasOne(x => x.ParentProductSerial)
            .WithMany(x => x.ChildLinks)
            .HasForeignKey(x => x.ParentProductSerialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ProductGenealogyLink>()
            .HasOne(x => x.ChildProductSerial)
            .WithMany(x => x.ParentLinks)
            .HasForeignKey(x => x.ChildProductSerialId)
            .OnDelete(DeleteBehavior.Restrict);

        foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(x => x.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    private List<AuditLog> BuildAuditEntries()
    {
        ChangeTracker.DetectChanges();
        var user = currentUser?.UserName ?? "system";
        var now = DateTimeOffset.UtcNow;
        var audits = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.Entity is AuditLog || entry.State is EntityState.Detached or EntityState.Unchanged)
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = user;
            }
            else
            {
                entry.Entity.ModifiedAt = now;
                entry.Entity.ModifiedBy = user;
            }

            var oldValues = entry.State == EntityState.Added ? null : entry.OriginalValues.Properties.ToDictionary(p => p.Name, p => entry.OriginalValues[p.Name]);
            var newValues = entry.State == EntityState.Deleted ? null : entry.CurrentValues.Properties.ToDictionary(p => p.Name, p => entry.CurrentValues[p.Name]);
            audits.Add(new AuditLog
            {
                EntityName = entry.Entity.GetType().Name,
                EntityId = entry.Entity.Id.ToString(),
                Action = entry.State.ToString(),
                ChangedBy = user,
                ChangedAt = now,
                OldValuesJson = oldValues is null ? null : JsonSerializer.Serialize(oldValues),
                NewValuesJson = newValues is null ? null : JsonSerializer.Serialize(newValues)
            });
        }

        return audits;
    }
}
