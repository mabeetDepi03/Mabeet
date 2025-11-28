using MabeetApi.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MabeetApi.Data
{
    public static class AppDbInitializer
    {
        public static async Task SeedUsersAndRolesAsync(IServiceProvider serviceProvider)
        {
            using var serviceScope = serviceProvider.CreateScope();

            var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Apply migrations (optional but recommended)
            context.Database.Migrate();

            // Seed Roles (Admin, Owner, Client)
            foreach (var roleName in Enum.GetNames(typeof(UserRole)))
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // Seed Admin
            await CreateUserIfNotExists(
                userManager,
                email: "admin@mabeet.com",
                username: "admin",
                firstName: "System",
                lastName: "Admin",
                nationalId: "11111111111111",
                phone: "01000000000",
                role: UserRole.Admin,
                password: "Admin@123"
            );

            // Seed Owner
            await CreateUserIfNotExists(
                userManager,
                email: "owner@mabeet.com",
                username: "owner",
                firstName: "Default",
                lastName: "Owner",
                nationalId: "22222222222222",
                phone: "01100000000",
                role: UserRole.Owner,
                password: "Owner@123"
            );

            // Seed Client
            await CreateUserIfNotExists(
                userManager,
                email: "client@mabeet.com",
                username: "client",
                firstName: "Default",
                lastName: "Client",
                nationalId: "33333333333333",
                phone: "01200000000",
                role: UserRole.Client,
                password: "Client@123"
            );
        }

        private static async Task CreateUserIfNotExists(
            UserManager<AppUser> userManager,
            string email,
            string username,
            string firstName,
            string lastName,
            string nationalId,
            string phone,
            UserRole role,
            string password)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new AppUser
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName,
                    NationalID = nationalId,
                    PhoneNumber = phone,
                    RoleType = role,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role.ToString());
                }
            }
        }
    }
}
