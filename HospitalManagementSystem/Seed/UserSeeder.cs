using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace HospitalManagementSystem.Seed
{
    public class UserSeeder
    {
        public static async Task SeedUsers(UserManager<ApplicationUser> userManager)
        {
            await CreateUserIfNotExists(userManager, "admin@hospital.com", "Admin@123", "Admin");
            await CreateUserIfNotExists(userManager, "doctor@hospital.com", "Doctor@123", "Doctor");
            await CreateUserIfNotExists(userManager, "patient@hospital.com", "Patient@123", "Patient");
            await CreateUserIfNotExists(userManager, "staff@hospital.com", "Staff@123", "Staff");
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
    }
}
