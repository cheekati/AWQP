using AWQP.Domain.CleanRoom;
using AWQP.Domain.Common;
using AWQP.Domain.Inventory;
using AWQP.Domain.Production;
using AWQP.Domain.Products;
using AWQP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AWQP.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AwqpDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var adminEmail = "admin@awqp.local";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                DisplayName = "System Administrator",
                Department = "IT"
            };
            await userManager.CreateAsync(admin, "ChangeMe!2026#Seed");
            await userManager.AddToRoleAsync(admin, Roles.Admin);
        }

        if (!await db.ProductCategories.AnyAsync())
        {
            db.ProductCategories.AddRange(
                new ProductCategory { Code = "SEM-QTZ", Name = "Semiconductor Quartz", Segment = IndustrySegment.Semiconductor },
                new ProductCategory { Code = "CSEM-QTZ", Name = "Compound Semiconductor Quartz", Segment = IndustrySegment.CompoundSemiconductor },
                new ProductCategory { Code = "OPT-QTZ", Name = "Optical Fiber Quartz", Segment = IndustrySegment.OpticalFiber },
                new ProductCategory { Code = "SOL-QTZ", Name = "Solar Cell Quartz", Segment = IndustrySegment.SolarCell });
        }

        if (!await db.MaterialGrades.AnyAsync())
        {
            db.MaterialGrades.AddRange(
                new MaterialGrade { Code = "HPQ-9999", Name = "High Purity Quartz 99.99", MinimumPurityPercent = 99.99m },
                new MaterialGrade { Code = "HPQ-99999", Name = "Ultra High Purity Quartz 99.999", MinimumPurityPercent = 99.999m });
        }

        if (!await db.CleanRoomAreas.AnyAsync())
        {
            db.CleanRoomAreas.Add(new CleanRoomArea
            {
                Code = "CR-1000-FIN",
                Name = "Final Operations Clean Room",
                IsoClass = "Class 1000",
                ParticleLimitPerCubicFoot = 1000
            });
        }

        if (!await db.Machines.AnyAsync())
        {
            db.Machines.AddRange(
                new Machine { Code = "LATHE-01", Name = "Quartz Precision Lathe", WorkCenter = "Forming", RatedCapacityPerHour = 4 },
                new Machine { Code = "FIRE-01", Name = "Quartz Firing Furnace", WorkCenter = "Firing", RatedCapacityPerHour = 8 },
                new Machine { Code = "VAC-01", Name = "Vacuum Packaging Station", WorkCenter = "Clean Room", RatedCapacityPerHour = 12, IsCleanRoomQualified = true });
        }

        if (!await db.Warehouses.AnyAsync())
        {
            var warehouse = new Warehouse { Code = "MAIN", Name = "Main Warehouse", SiteCode = "HQ" };
            warehouse.Locations.Add(new WarehouseLocation { Code = "RAW-A1", Name = "Raw Material Bin A1", Zone = "RAW", Bin = "A1" });
            warehouse.Locations.Add(new WarehouseLocation { Code = "FG-CR1", Name = "Clean Room Finished Goods", Zone = "FG", Bin = "CR1", IsCleanRoomStorage = true });
            db.Warehouses.Add(warehouse);
        }

        await db.SaveChangesAsync();
    }
}
