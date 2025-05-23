// Vjezba.Web/Helpers/IdentitySeeder.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Vjezba.Model;

namespace Vjezba.Web.Helpers
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                // Create roles if they don't exist
                string[] roleNames = { "Admin", "Manager" };
                
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // Create Admin user if it doesn't exist
                string adminEmail = "admin@example.com";
                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    AppUser adminUser = new AppUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        OIB = "12345678901",
                        JMBG = "1234567890123"
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }

                // Create Manager user if it doesn't exist
                string managerEmail = "manager@example.com";
                if (await userManager.FindByEmailAsync(managerEmail) == null)
                {
                    AppUser managerUser = new AppUser
                    {
                        UserName = managerEmail,
                        Email = managerEmail,
                        EmailConfirmed = true,
                        OIB = "23456789012",
                        JMBG = "2345678901234"
                    };

                    var result = await userManager.CreateAsync(managerUser, "Manager123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(managerUser, "Manager");
                    }
                }
            }
        }
    }
}