using ChangeManagement.Api.Models;

namespace ChangeManagement.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!db.Users.Any())
        {
            var users = new List<User>
            {
                new() { Email = "requester@cms.local", FullName = "Alex Requester", Role = UserRole.Requester, Department = "Engineering", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!") },
                new() { Email = "safety@cms.local", FullName = "Sam Safety", Role = UserRole.Safety, Department = "Safety", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!") },
                new() { Email = "depthead@cms.local", FullName = "Dana DeptHead", Role = UserRole.DeptHead, Department = "Engineering", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!") },
                new() { Email = "qa@cms.local", FullName = "Quinn QA", Role = UserRole.QA, Department = "Quality", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!") },
                new() { Email = "coo@cms.local", FullName = "Casey COO", Role = UserRole.COO, Department = "Executive", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!") },
                new() { Email = "admin@cms.local", FullName = "Admin User", Role = UserRole.Admin, Department = "IT", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!") },
            };
            db.Users.AddRange(users);
        }

        if (!db.LookupItems.Any())
        {
            var departments = new[] { "Engineering", "Production", "Quality", "Purchasing", "Safety", "Industrial", "R&D" };
            var products = new[] { "Product A", "Product B", "Product C", "Widget X", "Component Y", "Assembly Z" };

            var lookups = new List<LookupItem>();
            for (var i = 0; i < departments.Length; i++)
                lookups.Add(new LookupItem { Category = "Department", Value = departments[i], SortOrder = i + 1 });
            for (var i = 0; i < products.Length; i++)
                lookups.Add(new LookupItem { Category = "Product", Value = products[i], SortOrder = i + 1 });

            db.LookupItems.AddRange(lookups);
        }

        await db.SaveChangesAsync();
    }
}
