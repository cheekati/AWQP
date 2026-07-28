using ChangeManagement.Domain.Entities;
using ChangeManagement.Domain.Enums;
using ChangeManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChangeManagement.Infrastructure.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        await db.Database.EnsureCreatedAsync();

        if (await db.Roles.AnyAsync())
        {
            logger.LogInformation("Database already seeded.");
            return;
        }

        logger.LogInformation("Seeding database...");

        var roles = AppRoles.All.Select(r => new Role
        {
            Name = r,
            Description = $"{r} role",
            CreatedBy = "system"
        }).ToList();
        await db.Roles.AddRangeAsync(roles);

        var departments = new[]
        {
            new Department { Code = "ENG", Name = "Engineering", CreatedBy = "system" },
            new Department { Code = "QA", Name = "Quality Assurance", CreatedBy = "system" },
            new Department { Code = "PRD", Name = "Production", CreatedBy = "system" },
            new Department { Code = "SAFE", Name = "Safety", CreatedBy = "system" },
            new Department { Code = "OPS", Name = "Operations", CreatedBy = "system" }
        };
        await db.Departments.AddRangeAsync(departments);

        var divisions = new[]
        {
            new Division { Code = "DIV-A", Name = "Division A", CreatedBy = "system" },
            new Division { Code = "DIV-B", Name = "Division B", CreatedBy = "system" },
            new Division { Code = "DIV-C", Name = "Division C", CreatedBy = "system" }
        };
        await db.Divisions.AddRangeAsync(divisions);

        var products = new[]
        {
            new Product { Code = "PRD-001", Name = "Product Alpha", CreatedBy = "system" },
            new Product { Code = "PRD-002", Name = "Product Beta", CreatedBy = "system" },
            new Product { Code = "PRD-003", Name = "Product Gamma", CreatedBy = "system" }
        };
        await db.Products.AddRangeAsync(products);

        var customers = new[]
        {
            new Customer { Code = "CUS-001", Name = "ACME Corp", CreatedBy = "system" },
            new Customer { Code = "CUS-002", Name = "Globex Industries", CreatedBy = "system" }
        };
        await db.Customers.AddRangeAsync(customers);

        await db.SaveChangesAsync();

        var password = BCrypt.Net.BCrypt.HashPassword("Password@123");
        var users = new List<(string UserName, string First, string Last, string Email, string Role, Guid? DeptId)>
        {
            ("admin", "System", "Administrator", "admin@cms.local", AppRoles.Administrator, departments[4].Id),
            ("requester", "Rita", "Requester", "requester@cms.local", AppRoles.Requester, departments[0].Id),
            ("safety", "Sam", "Safety", "safety@cms.local", AppRoles.Safety, departments[3].Id),
            ("depthead", "Dana", "DeptHead", "depthead@cms.local", AppRoles.DepartmentHead, departments[0].Id),
            ("qa", "Quinn", "Quality", "qa@cms.local", AppRoles.QA, departments[1].Id),
            ("coo", "Chris", "COO", "coo@cms.local", AppRoles.COO, departments[4].Id)
        };

        foreach (var u in users)
        {
            var role = roles.First(r => r.Name == u.Role);
            var user = new User
            {
                UserName = u.UserName,
                FirstName = u.First,
                LastName = u.Last,
                Email = u.Email,
                PasswordHash = password,
                DepartmentId = u.DeptId,
                EmployeeId = $"EMP-{u.UserName.ToUpperInvariant()}",
                IsActive = true,
                CreatedBy = "system"
            };
            user.UserRoles.Add(new UserRole { RoleId = role.Id });
            await db.Users.AddAsync(user);
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Seed completed. Default password for all users: Password@123");
    }
}
