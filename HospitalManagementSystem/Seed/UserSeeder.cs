using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace HospitalManagementSystem.Seed
{
    public class UserSeeder
    {
        public static async Task SeedUsers(UserManager<ApplicationUser> userManager)
        {
            await CreateUserIfNotExists(userManager, "admin@hospital.com", "Admin@123", "Admin");
            await CreateUserIfNotExists(userManager, "guru99", "guru99@hospital.com", "MERCURY", "Admin");
        }

        private static async Task CreateUserIfNotExists(UserManager<ApplicationUser> userManager, string email, string password, string role)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
        private static async Task CreateUserIfNotExists(UserManager<ApplicationUser> userManager, string username, string email, string password, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true,
                    NormalizedUserName = username.ToUpper(),
                    NormalizedEmail = email.ToUpper()
                };

                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
                else
                {
                    // Log full errors if something fails
                    throw new Exception($"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
