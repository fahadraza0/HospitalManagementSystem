using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize]
    public class TeamController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeamController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var teams = _context.Teams.Include(t => t.Doctors).ToList();
            return View(teams);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Team team)
        {
            if (ModelState.IsValid)
            {
                _context.Teams.Add(team);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        public IActionResult AddDoctor(int teamId)
        {
            var team = _context.Teams.Include(t => t.Doctors)
                                     .FirstOrDefault(t => t.TeamId == teamId);
            if (team == null)
            {
                return NotFound();
            }
            ViewBag.Doctors = _context.Doctors.Where(d => d.TeamId == null).ToList();
            return View(team);
        }

        [HttpPost]
        public async Task<IActionResult> AddDoctor(int teamId, int doctorId)
        {
            // Fetch the doctor to ensure they exist
            var doctor = await _context.Doctors.FindAsync(doctorId);

            // Check if doctor exists
            if (doctor == null)
            {
                // Handle the case where doctor is not found
                ModelState.AddModelError("", "Doctor not found.");
                return RedirectToAction(nameof(Index)); // Or any appropriate error handling
            }

            // Fetch the team to ensure the team exists
            var team = await _context.Teams.FindAsync(teamId);

            // Check if the team exists
            if (team == null)
            {
                // Handle the case where the team does not exist
                ModelState.AddModelError("", "Team not found.");
                return RedirectToAction(nameof(Index)); // Or any appropriate error handling
            }

            // Assign the doctor to the team
            doctor.TeamId = teamId;

            // Save the changes
            await _context.SaveChangesAsync();

            // Redirect to the Index page after successful operation
            return RedirectToAction(nameof(Index));
        }
    }
}
