using ChangeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Division> Divisions => Set<Division>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<EngineeringRequest> EngineeringRequests => Set<EngineeringRequest>();
    public DbSet<EngineeringRequestReason> EngineeringRequestReasons => Set<EngineeringRequestReason>();
    public DbSet<EngineeringRequestAttachment> EngineeringRequestAttachments => Set<EngineeringRequestAttachment>();
    public DbSet<VerificationHistory> VerificationHistories => Set<VerificationHistory>();
    public DbSet<ApprovalHistory> ApprovalHistories => Set<ApprovalHistory>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<VerificationAssignment> VerificationAssignments => Set<VerificationAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.UserName).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.UserName).HasMaxLength(50).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            e.HasOne(x => x.Department).WithMany(d => d.Users).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Role>(e =>
        {
            e.HasIndex(x => x.Name).IsUnique();
            e.Property(x => x.Name).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<UserRole>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
            e.HasOne(x => x.User).WithMany(u => u.UserRoles).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Role).WithMany(r => r.UserRoles).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Department>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Division>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
        });

        modelBuilder.Entity<Customer>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<EngineeringRequest>(e =>
        {
            e.HasIndex(x => x.ErNumber).IsUnique();
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.RequesterId);
            e.HasIndex(x => x.CreatedDate);
            e.Property(x => x.ErNumber).HasMaxLength(30).IsRequired();
            e.Property(x => x.Title).HasMaxLength(250).IsRequired();
            e.Property(x => x.Customer).HasMaxLength(200).IsRequired();
            e.Property(x => x.Process).HasMaxLength(4000);
            e.Property(x => x.DetailsOfEvaluation).HasMaxLength(4000);
            e.Property(x => x.PresentCondition).HasMaxLength(4000);
            e.Property(x => x.NewCondition).HasMaxLength(4000);
            e.Property(x => x.Merit).HasMaxLength(4000);
            e.Property(x => x.Demerit).HasMaxLength(4000);
            e.Property(x => x.MaterialDisposition).HasMaxLength(4000);
            e.Property(x => x.TestLotDescription).HasMaxLength(2000);
            e.Property(x => x.SafetyDataSheet).HasMaxLength(2000);
            e.Property(x => x.ChemicalLabel).HasMaxLength(2000);
            e.Property(x => x.ChemicalClassification).HasMaxLength(2000);
            e.Property(x => x.ChemicalInventoryManagementSystem).HasMaxLength(2000);
            e.Property(x => x.LatestRejectionComments).HasMaxLength(4000);
            e.HasOne(x => x.Division).WithMany(d => d.EngineeringRequests).HasForeignKey(x => x.DivisionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Department).WithMany(d => d.EngineeringRequests).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Product).WithMany(p => p.EngineeringRequests).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Requester).WithMany(u => u.EngineeringRequests).HasForeignKey(x => x.RequesterId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EngineeringRequestReason>(e =>
        {
            e.HasIndex(x => new { x.EngineeringRequestId, x.Reason }).IsUnique();
            e.HasOne(x => x.EngineeringRequest).WithMany(r => r.Reasons).HasForeignKey(x => x.EngineeringRequestId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EngineeringRequestAttachment>(e =>
        {
            e.Property(x => x.FileName).HasMaxLength(260).IsRequired();
            e.Property(x => x.OriginalFileName).HasMaxLength(260).IsRequired();
            e.Property(x => x.ContentType).HasMaxLength(150);
            e.Property(x => x.StoragePath).HasMaxLength(500).IsRequired();
            e.HasOne(x => x.EngineeringRequest).WithMany(r => r.Attachments).HasForeignKey(x => x.EngineeringRequestId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VerificationHistory>(e =>
        {
            e.HasIndex(x => x.EngineeringRequestId);
            e.HasOne(x => x.EngineeringRequest).WithMany(r => r.VerificationHistories).HasForeignKey(x => x.EngineeringRequestId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Verifier).WithMany().HasForeignKey(x => x.VerifierId).OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.Comments).HasMaxLength(4000);
        });

        modelBuilder.Entity<ApprovalHistory>(e =>
        {
            e.HasIndex(x => x.EngineeringRequestId);
            e.HasOne(x => x.EngineeringRequest).WithMany(r => r.ApprovalHistories).HasForeignKey(x => x.EngineeringRequestId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Approver).WithMany().HasForeignKey(x => x.ApproverId).OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.Comments).HasMaxLength(4000);
        });

        modelBuilder.Entity<Comment>(e =>
        {
            e.Property(x => x.Content).HasMaxLength(4000).IsRequired();
            e.HasOne(x => x.EngineeringRequest).WithMany(r => r.Comments).HasForeignKey(x => x.EngineeringRequestId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.IsRead });
            e.Property(x => x.Subject).HasMaxLength(250).IsRequired();
            e.Property(x => x.Message).HasMaxLength(4000).IsRequired();
            e.HasOne(x => x.User).WithMany(u => u.Notifications).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.EngineeringRequest).WithMany().HasForeignKey(x => x.EngineeringRequestId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasIndex(x => x.EntityName);
            e.HasIndex(x => x.CreatedDate);
            e.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
            e.Property(x => x.EntityId).HasMaxLength(50).IsRequired();
            e.Property(x => x.Action).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<VerificationAssignment>(e =>
        {
            e.HasIndex(x => new { x.EngineeringRequestId, x.Stage, x.IsCompleted });
            e.HasOne(x => x.EngineeringRequest).WithMany().HasForeignKey(x => x.EngineeringRequestId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.AssignedToUser).WithMany().HasForeignKey(x => x.AssignedToUserId).OnDelete(DeleteBehavior.SetNull);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedDate ??= DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
