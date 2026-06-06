using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AWQP.Infrastructure.Persistence;

public sealed class AwqpDbContextFactory : IDesignTimeDbContextFactory<AwqpDbContext>
{
    public AwqpDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AwqpDbContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable("AWQP_CONNECTION_STRING")
                          ?? "Server=localhost,1433;Database=AWQP;User Id=sa;Password=Change_this_password_123!;TrustServerCertificate=True")
            .Options;
        return new AwqpDbContext(options);
    }
}
