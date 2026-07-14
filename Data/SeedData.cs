// Data/SeedData.cs - Fixed
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Grand_Arbre_portal.Models;
using Grand_Arbre_portal.Data;

namespace GrandArbrePortal.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Create roles
            string[] roles = { "Admin", "Employee", "Client" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create Admin user
            var adminEmail = "admin@grandarbre.co.za";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Admin User",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };
                await userManager.CreateAsync(adminUser, "Admin123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Create sample access codes for demonstration
            if (!await context.AccessCodes.AnyAsync())
            {
                await context.AccessCodes.AddRangeAsync(
                    new AccessCode
                    {
                        Code = "EMP2024",
                        Role = "Employee",
                        CreatedByUserId = adminUser.Id,
                        CreatedByUserName = "Admin User",
                        CreatedAt = DateTime.Now,
                        ExpiryDate = DateTime.Now.AddMonths(6),
                        IsUsed = false,
                        IsActive = true
                    },
                    new AccessCode
                    {
                        Code = "CLT2024",
                        Role = "Client",
                        CreatedByUserId = adminUser.Id,
                        CreatedByUserName = "Admin User",
                        CreatedAt = DateTime.Now,
                        ExpiryDate = DateTime.Now.AddMonths(6),
                        IsUsed = false,
                        IsActive = true
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}