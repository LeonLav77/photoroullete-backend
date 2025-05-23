using Microsoft.AspNetCore.Identity;
using Vjezba.Model;

namespace Vjezba.Infrastructure
{
    public static class DatabaseSeeder
    {
        public static async Task SeedDatabase(WebApplication app)
        {
            try
            {
                using var scope = app.Services.CreateScope();
                await SeedRolesAndUsers(scope);
            }
            catch (Exception ex)
            {
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        private static async Task SeedRolesAndUsers(IServiceScope scope)
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            await SeedRoles(roleManager);
            await SeedAdminUser(userManager);
            await SeedManagerUser(userManager);
        }

        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            
            if (!await roleManager.RoleExistsAsync("Manager"))
                await roleManager.CreateAsync(new IdentityRole("Manager"));
        }

        private static async Task SeedAdminUser(UserManager<AppUser> userManager)
        {
            var adminEmail = "admin@example.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    OIB = "12345678901",
                    JMBG = "1234567890123"
                };

                if ((await userManager.CreateAsync(adminUser, "Admin123!")).Succeeded)
                    await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        private static async Task SeedManagerUser(UserManager<AppUser> userManager)
        {
            var managerEmail = "manager@example.com";
            if (await userManager.FindByEmailAsync(managerEmail) == null)
            {
                var managerUser = new AppUser
                {
                    UserName = managerEmail,
                    Email = managerEmail,
                    EmailConfirmed = true,
                    OIB = "23456789012",
                    JMBG = "2345678901234"
                };

                if ((await userManager.CreateAsync(managerUser, "Manager123!")).Succeeded)
                    await userManager.AddToRoleAsync(managerUser, "Manager");
            }
        }
    }
}