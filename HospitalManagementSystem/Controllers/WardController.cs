using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize]
    public class WardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var wards = await _context.Wards.Include(w => w.Beds).ToListAsync();

            ViewData["ActivePage"] = "WardManagement";
            return View(wards);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Ward ward, int bedCount)
        {
            if (ModelState.IsValid)
            {
                _context.Wards.Add(ward);
                await _context.SaveChangesAsync();

                for (int i = 0; i < bedCount; i++)
                {
                    _context.Beds.Add(new Bed { WardId = ward.WardId });
                }
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(ward);
        }
        [HttpGet]
        public IActionResult GetAvailableBeds(int wardId)
        {
            var beds = _context.Beds
                .Where(b => b.WardId == wardId && !b.IsOccupied)
                .Select(b => new
                {
                    bedId = b.BedId,
                    bedNumber = b.BedId,
                    wardName = b.Ward.WardName
                })
                .ToList();

            return Json(beds);
        }

    }
}
