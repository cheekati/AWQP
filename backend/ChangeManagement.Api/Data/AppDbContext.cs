using ChangeManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<EngineeringRequest> EngineeringRequests => Set<EngineeringRequest>();
    public DbSet<ErAttachment> ErAttachments => Set<ErAttachment>();
    public DbSet<ErComment> ErComments => Set<ErComment>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();
    public DbSet<LookupItem> LookupItems => Set<LookupItem>();
    public DbSet<ErSequence> ErSequences => Set<ErSequence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.FullName).HasMaxLength(200);
        });

        modelBuilder.Entity<EngineeringRequest>(e =>
        {
            e.HasIndex(x => new { x.ErNumber, x.SubmissionNumber }).IsUnique();
            e.Property(x => x.ErNumber).HasMaxLength(20);
            e.Property(x => x.Title).HasMaxLength(200);
            e.Ignore(x => x.DisplayErNumber);
            e.HasOne(x => x.Requester)
                .WithMany(u => u.Requests)
                .HasForeignKey(x => x.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ErAttachment>(e =>
        {
            e.HasOne(x => x.EngineeringRequest)
                .WithMany(r => r.Attachments)
                .HasForeignKey(x => x.EngineeringRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ErComment>(e =>
        {
            e.HasOne(x => x.EngineeringRequest)
                .WithMany(r => r.Comments)
                .HasForeignKey(x => x.EngineeringRequestId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EmailLog>(e =>
        {
            e.HasOne(x => x.EngineeringRequest)
                .WithMany(r => r.EmailLogs)
                .HasForeignKey(x => x.EngineeringRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LookupItem>(e =>
        {
            e.HasIndex(x => new { x.Category, x.Value }).IsUnique();
        });

        modelBuilder.Entity<ErSequence>(e =>
        {
            e.HasIndex(x => x.YearMonth).IsUnique();
        });
    }
}
