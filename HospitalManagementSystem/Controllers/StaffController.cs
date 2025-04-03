using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public StaffController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var staff = await _context.Staff.ToListAsync();
            return View(staff);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Staff staff)
        {
            // ✅ Check for duplicate email
            if (await _userManager.FindByEmailAsync(staff.Email) != null)
            {
                ModelState.AddModelError("", "Email already exists.");
                return View(staff);
            }

            // ✅ Create the User in AspNetUsers
            var user = new ApplicationUser
            {
                UserName = staff.Email,
                Email = staff.Email,
                PhoneNumber = staff.PhoneNumber,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, staff.Password);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to create user.");
                return View(staff);
            }

            // ✅ Assign the Staff role if it doesn't exist
            if (!await _roleManager.RoleExistsAsync("Staff"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Staff"));
            }

            await _userManager.AddToRoleAsync(user, "Staff");

            // ✅ Link Staff with the User
            staff.UserId = user.Id;
            _context.Add(staff);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Staff created successfully with user login!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var staff = await _context.Staff.FindAsync(id);
            if (staff == null) return NotFound();

            return View(staff);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Staff staff, string NewPassword)
        {
            if (id != staff.StaffId)
            {
                return NotFound();
            }
            var existingStaff = await _context.Staff.FindAsync(id);

            if (existingStaff == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByEmailAsync(existingStaff.Email);

            // Update staff details
            existingStaff.FullName = staff.FullName;
            existingStaff.Email = staff.Email;
            existingStaff.PhoneNumber = staff.PhoneNumber;
            existingStaff.Department = staff.Department;
            existingStaff.AssignedWard = staff.AssignedWard;
            existingStaff.Role = staff.Role;
            existingStaff.IsActive = staff.IsActive;

            // Update password only if a new one is provided
            if (!string.IsNullOrEmpty(NewPassword))
            {
                var passwordHasher = new PasswordHasher<ApplicationUser>();
                user.PasswordHash = passwordHasher.HashPassword(user, NewPassword);
                await _userManager.UpdateAsync(user);
            }

            _context.Update(existingStaff);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Staff updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ✅ Disable User when deleting Staff
        public async Task<IActionResult> Delete(int id)
        {
            var staff = await _context.Staff.FindAsync(id);
            if (staff == null) return NotFound();

            // ✅ Find the associated User
            var user = await _userManager.FindByEmailAsync(staff.Email);
            if (user != null)
            {
                // Disable the user instead of deleting them
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;  // Disable indefinitely
                await _userManager.UpdateAsync(user);
            }

            _context.Staff.Remove(staff);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Staff deleted and User disabled!";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int id)
        {
            var staff = await _context.Staff.FirstOrDefaultAsync(m => m.StaffId == id);

            if (staff == null)
            {
                return NotFound();
            }

            return View(staff);
        }
    }
}
