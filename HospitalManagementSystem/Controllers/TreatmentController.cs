using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    public class TreatmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TreatmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> History(int patientId)
        {
            if (patientId == 0)
            {
                var history = await _context.TreatmentRecords
                .Include(tr => tr.Doctor)
                .OrderByDescending(tr => tr.TreatmentDate)
                .ToListAsync();
                return View(history);
            }
            var historyByPatient = await _context.TreatmentRecords
                .Include(tr => tr.Doctor)
                .Where(tr => tr.PatientId == patientId)
                .OrderByDescending(tr => tr.TreatmentDate)
                .ToListAsync();

            var patient = await _context.Patients.FindAsync(patientId);
            ViewBag.PatientName = patient?.FullName;

            return View(historyByPatient);
        }
        // GET: Treatment/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var treatmentRecord = await _context.TreatmentRecords
                .Include(tr => tr.Doctor)
                .FirstOrDefaultAsync(m => m.RecordId == id);
            if (treatmentRecord == null)
            {
                return NotFound();
            }

            return View(treatmentRecord);
        }

        // POST: Treatment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var treatmentRecord = await _context.TreatmentRecords.FindAsync(id);
            if (treatmentRecord != null)
            {
                _context.TreatmentRecords.Remove(treatmentRecord);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(History), new { patientId = treatmentRecord.PatientId });
        }
        // GET: Treatment/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var treatmentRecord = await _context.TreatmentRecords.FindAsync(id);
            if (treatmentRecord == null)
            {
                return NotFound();
            }

            ViewBag.Doctors = _context.Doctors.ToList();
            return View(treatmentRecord);
        }

        // POST: Treatment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TreatmentRecord treatmentRecord)
        {
            if (id != treatmentRecord.RecordId)
            {
                return NotFound();
            }

                try
                {
                    _context.Update(treatmentRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TreatmentRecordExists(treatmentRecord.RecordId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(History), new { patientId = treatmentRecord.PatientId });
        }

        private bool TreatmentRecordExists(int id)
        {
            return _context.TreatmentRecords.Any(e => e.RecordId == id);
        }

        [Authorize(Roles = "Admin, Doctor")]
        public IActionResult Create(int patientId)
        {
            // Pass PatientId and list of doctors to the view
            ViewBag.PatientId = patientId;
            ViewBag.Doctors = _context.Doctors.ToList();
            ViewData["ActivePage"] = "PatientAssistance";

            return View();
        }

        // POST: Treatment/Create
        [HttpPost]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> Create(TreatmentRecord treatmentRecord)
        {
                _context.Add(treatmentRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(History), new { patientId = treatmentRecord.PatientId });
        }
        [HttpGet]
        public JsonResult GetPatientsByDoctor(int doctorId)
        {
            var patients = _context.Patients
                .Where(p => p.AssignedDoctorId == doctorId) // Assuming a Patient has a DoctorId property
                .ToList()
                .Select(p => new { p.PatientId, p.FullName })  // Return only necessary fields
                .ToList();

            return Json(patients);
        }

        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> AddTreatment(int patientId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == currentUser.Email);

            if (doctor == null)
            {
                return NotFound("Doctor not found.");
            }

            if (User.IsInRole("Admin"))
            {
                ViewBag.Patients = await _context.Patients
                    .Include(p => p.AssignedDoctor)
                    .Include(p => p.Ward)
                    .ToListAsync();
            }
            else
            {
                ViewBag.Patients = await _context.Patients
                    .Where(p => p.AssignedDoctorId == doctor.DoctorId)
                    .Include(p => p.AssignedDoctor)
                    .Include(p => p.Ward)
                    .ToListAsync();
            }

            ViewBag.Doctors = _context.Doctors.ToList();
            ViewData["ActivePage"] = "PatientAssistance";

            return View();
        }

        // POST: Treatment/Create
        [HttpPost]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> AddTreatment(TreatmentRecord treatmentRecord)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == currentUser.Email);
            if (doctor == null)
            {
                return NotFound("Doctor not found.");
            }

            treatmentRecord.DoctorId = doctor.DoctorId;
            _context.Add(treatmentRecord);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(History), new { patientId = treatmentRecord.PatientId });
        }
    }
}
