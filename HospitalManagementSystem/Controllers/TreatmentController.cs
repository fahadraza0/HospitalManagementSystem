using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.ViewModel;
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
        public ActionResult Index()
        {
            return RedirectToAction("History", new { patientId = 0 });
        }
        public async Task<IActionResult> History(int patientId)
        {
            if (patientId == 0)
            {
                var history = await _context.TreatmentRecords
                .Include(tr => tr.Doctor)
                .OrderByDescending(tr => tr.TreatmentDate)
                .ToListAsync();

                ViewBag.PatientName = "admin@hospital.com";
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

        public async Task<IActionResult> Details(int id)
        {
            var treatment = await _context.TreatmentRecords
                .Include(tr => tr.Doctor)
                .Include(tr => tr.Patient)
                .FirstOrDefaultAsync(tr => tr.RecordId == id);

            if (treatment == null)
                return NotFound();

            var medicines = await _context.TreatmentMedicines
                .Include(tm => tm.Medicine)
                .Where(tm => tm.TreatmentId == id)
                .ToListAsync();

            ViewBag.Medicines = medicines;
            return View(treatment);
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
        public IActionResult Create(int? patientId = null)
        {
            ViewData["ActivePage"] = "PatientAssistance";

            var viewModel = new TreatmentRecordViewModel
            {
                TreatmentRecord = new TreatmentRecord(),
                Medicines = _context.Medicines.ToList(),
                MedicinesWithQuantities = new List<MedicineSelection>(), // Make sure the list is initialized
            };

            if (User.IsInRole("Admin"))
            {
                var doctors = _context.Doctors.ToList();
                ViewBag.Doctors = doctors;
            }
            else if (User.IsInRole("Doctor"))
            {
                var doctorEmail = User.Identity?.Name;
                var doctor = _context.Doctors.FirstOrDefault(d => d.Email == doctorEmail);
                if (doctor != null)
                {
                    ViewBag.DoctorId = doctor.DoctorId;

                    viewModel.TreatmentRecord.DoctorId = doctor.DoctorId;

                    ViewBag.Patients = _context.Patients
                        .Where(p => p.AssignedDoctorId == doctor.DoctorId)
                        .ToList();
                }
            }

            ViewBag.PatientId = patientId;
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> Create(TreatmentRecordViewModel model)
        {           

            // Save TreatmentRecord
            
            _context.TreatmentRecords.Add(model.TreatmentRecord);
            await _context.SaveChangesAsync();

            // Save selected medicines (many-to-many with quantities)
            if (model.MedicinesWithQuantities != null)
            {
                foreach (var med in model.MedicinesWithQuantities)
                {
                    if (med.MedicineId > 0 && med.Quantity > 0)
                    {
                        _context.TreatmentMedicines.Add(new TreatmentMedicine
                        {
                            TreatmentId = model.TreatmentRecord.RecordId,
                            MedicineId = med.MedicineId,
                            Quantity = med.Quantity
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(History), new { patientId = model.TreatmentRecord.PatientId });
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
