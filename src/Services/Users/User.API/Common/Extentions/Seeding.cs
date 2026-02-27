using BuildingBlocks.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Users.API.Domain.Models;
using Users.API.Feature.User.Common;
using Users.API.Infrastructure;

namespace Users.API.Common.Extentions;

public static class Seeding
{
    public static async Task SeedAdminUser(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var adminUsername = "all";
        var adminEmail = "all@gmail.com";
        var adminPassword = "Admin@123";
        var adminDisplayName = "Admin User";

        // ===========================
        // 1️⃣ Seed Roles
        // ===========================
        foreach (var roleName in Roles.AllRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                Console.WriteLine($"Role created: {roleName}");
            }
        }
        
        var user = await userManager.FindByEmailAsync(adminEmail);
        if (user != null)
        {
            Console.WriteLine("Admin user already exists.");
            return;
        }

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            UserName = adminUsername,
            Email = adminEmail,
            DisplayName = adminDisplayName,
            EmailConfirmed = true,
            LastLogin = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            Console.WriteLine($"Failed to create admin user: {errors}");
            return;
        }

        // ===========================
        // 3️⃣ Assign All Roles
        // ===========================
        foreach (var roleName in Roles.AllRoles)
        {
            await userManager.AddToRoleAsync(adminUser, roleName);
        }
        Console.WriteLine("Admin user created with all roles.");
    }
}