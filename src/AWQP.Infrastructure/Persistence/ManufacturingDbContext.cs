using AWQP.Application.Interfaces;
using AWQP.Domain.Common;
using AWQP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AWQP.Infrastructure.Persistence;

public sealed class ManufacturingDbContext : DbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public ManufacturingDbContext(DbContextOptions<ManufacturingDbContext> options, ICurrentUserService? currentUserService = null) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserActivityAudit> UserActivityAudits => Set<UserActivityAudit>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerContact> CustomerContacts => Set<CustomerContact>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<CustomerIndustry> CustomerIndustries => Set<CustomerIndustry>();
    public DbSet<CustomerDocument> CustomerDocuments => Set<CustomerDocument>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductRevision> ProductRevisions => Set<ProductRevision>();
    public DbSet<EngineeringChangeRequest> EngineeringChangeRequests => Set<EngineeringChangeRequest>();
    public DbSet<RequestForQuotation> RequestForQuotations => Set<RequestForQuotation>();
    public DbSet<RequestForQuotationLine> RequestForQuotationLines => Set<RequestForQuotationLine>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<QuotationLine> QuotationLines => Set<QuotationLine>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<ProductionSchedule> ProductionSchedules => Set<ProductionSchedule>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<ManufacturingOperation> ManufacturingOperations => Set<ManufacturingOperation>();
    public DbSet<ProductSerial> ProductSerials => Set<ProductSerial>();
    public DbSet<InspectionRecord> InspectionRecords => Set<InspectionRecord>();
    public DbSet<InspectionMeasurement> InspectionMeasurements => Set<InspectionMeasurement>();
    public DbSet<DefectRecord> DefectRecords => Set<DefectRecord>();
    public DbSet<NonConformanceReport> NonConformanceReports => Set<NonConformanceReport>();
    public DbSet<CapaRecord> CapaRecords => Set<CapaRecord>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<WarehouseLocation> WarehouseLocations => Set<WarehouseLocation>();
    public DbSet<Bin> Bins => Set<Bin>();
    public DbSet<InventoryStock> InventoryStocks => Set<InventoryStock>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<CleanRoom> CleanRooms => Set<CleanRoom>();
    public DbSet<EnvironmentalReading> EnvironmentalReadings => Set<EnvironmentalReading>();
    public DbSet<CleanRoomAccessLog> CleanRoomAccessLogs => Set<CleanRoomAccessLog>();
    public DbSet<PackagingBatch> PackagingBatches => Set<PackagingBatch>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentLine> ShipmentLines => Set<ShipmentLine>();
    public DbSet<ManagedDocument> ManagedDocuments => Set<ManagedDocument>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PpeItem> PpeItems => Set<PpeItem>();
    public DbSet<PpeRequest> PpeRequests => Set<PpeRequest>();
    public DbSet<PpeIssue> PpeIssues => Set<PpeIssue>();
    public DbSet<PpeStockEntry> PpeStockEntries => Set<PpeStockEntry>();
    public DbSet<WorkPermit> WorkPermits => Set<WorkPermit>();
    public DbSet<DailyAtmosphericReading> DailyAtmosphericReadings => Set<DailyAtmosphericReading>();
    public DbSet<AtmosphericParameterRange> AtmosphericParameterRanges => Set<AtmosphericParameterRange>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureSchemas(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ManufacturingDbContext).Assembly);
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(t => typeof(AuditableEntity).IsAssignableFrom(t.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(ConvertFilterExpression(entityType.ClrType));
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var user = _currentUserService?.UserName ?? "system";
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = now;
                entry.Entity.CreatedBy = user;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedAtUtc = now;
                entry.Entity.ModifiedBy = user;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    private static void ConfigureSchemas(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>().ToTable("ApplicationUsers", "auth");
        modelBuilder.Entity<RefreshToken>().ToTable("RefreshTokens", "auth");
        modelBuilder.Entity<UserActivityAudit>().ToTable("UserActivityAudits", "auth");

        modelBuilder.Entity<Customer>().ToTable("Customers", "erp");
        modelBuilder.Entity<CustomerContact>().ToTable("CustomerContacts", "erp");
        modelBuilder.Entity<CustomerAddress>().ToTable("CustomerAddresses", "erp");
        modelBuilder.Entity<CustomerIndustry>().ToTable("CustomerIndustries", "erp");
        modelBuilder.Entity<CustomerDocument>().ToTable("CustomerDocuments", "erp");
        modelBuilder.Entity<ProductCategory>().ToTable("ProductCategories", "erp");
        modelBuilder.Entity<Product>().ToTable("Products", "erp");
        modelBuilder.Entity<ProductRevision>().ToTable("ProductRevisions", "erp");
        modelBuilder.Entity<EngineeringChangeRequest>().ToTable("EngineeringChangeRequests", "erp");
        modelBuilder.Entity<RequestForQuotation>().ToTable("RequestForQuotations", "sales");
        modelBuilder.Entity<RequestForQuotationLine>().ToTable("RequestForQuotationLines", "sales");
        modelBuilder.Entity<Quotation>().ToTable("Quotations", "sales");
        modelBuilder.Entity<QuotationLine>().ToTable("QuotationLines", "sales");
        modelBuilder.Entity<SalesOrder>().ToTable("SalesOrders", "sales");
        modelBuilder.Entity<SalesOrderLine>().ToTable("SalesOrderLines", "sales");

        modelBuilder.Entity<Machine>().ToTable("Machines", "mes");
        modelBuilder.Entity<Shift>().ToTable("Shifts", "mes");
        modelBuilder.Entity<ProductionSchedule>().ToTable("ProductionSchedules", "mes");
        modelBuilder.Entity<WorkOrder>().ToTable("WorkOrders", "mes");
        modelBuilder.Entity<ManufacturingOperation>().ToTable("ManufacturingOperations", "mes");
        modelBuilder.Entity<ProductSerial>().ToTable("ProductSerials", "mes");

        modelBuilder.Entity<InspectionRecord>().ToTable("InspectionRecords", "qms");
        modelBuilder.Entity<InspectionMeasurement>().ToTable("InspectionMeasurements", "qms");
        modelBuilder.Entity<DefectRecord>().ToTable("DefectRecords", "qms");
        modelBuilder.Entity<NonConformanceReport>().ToTable("NonConformanceReports", "qms");
        modelBuilder.Entity<CapaRecord>().ToTable("CapaRecords", "qms");

        modelBuilder.Entity<Item>().ToTable("Items", "whs");
        modelBuilder.Entity<Warehouse>().ToTable("Warehouses", "whs");
        modelBuilder.Entity<WarehouseLocation>().ToTable("WarehouseLocations", "whs");
        modelBuilder.Entity<Bin>().ToTable("Bins", "whs");
        modelBuilder.Entity<InventoryStock>().ToTable("InventoryStocks", "whs");
        modelBuilder.Entity<InventoryTransaction>().ToTable("InventoryTransactions", "whs");

        modelBuilder.Entity<CleanRoom>().ToTable("CleanRooms", "cleanroom");
        modelBuilder.Entity<EnvironmentalReading>().ToTable("EnvironmentalReadings", "cleanroom");
        modelBuilder.Entity<CleanRoomAccessLog>().ToTable("CleanRoomAccessLogs", "cleanroom");
        modelBuilder.Entity<PackagingBatch>().ToTable("PackagingBatches", "shipping");
        modelBuilder.Entity<Shipment>().ToTable("Shipments", "shipping");
        modelBuilder.Entity<ShipmentLine>().ToTable("ShipmentLines", "shipping");
        modelBuilder.Entity<ManagedDocument>().ToTable("ManagedDocuments", "docs");

        modelBuilder.Entity<Employee>().ToTable("Employees", "ehs");
        modelBuilder.Entity<PpeItem>().ToTable("PpeItems", "ehs");
        modelBuilder.Entity<PpeRequest>().ToTable("PpeRequests", "ehs");
        modelBuilder.Entity<PpeIssue>().ToTable("PpeIssues", "ehs");
        modelBuilder.Entity<PpeStockEntry>().ToTable("PpeStockEntries", "ehs");
        modelBuilder.Entity<WorkPermit>().ToTable("WorkPermits", "ehs");
        modelBuilder.Entity<DailyAtmosphericReading>().ToTable("DailyAtmosphericReadings", "ehs");
        modelBuilder.Entity<AtmosphericParameterRange>().ToTable("AtmosphericParameterRanges", "ehs");
    }

    private static System.Linq.Expressions.LambdaExpression ConvertFilterExpression(Type entityType)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(AuditableEntity.IsDeleted));
        var condition = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false));
        return System.Linq.Expressions.Expression.Lambda(condition, parameter);
    }
}
