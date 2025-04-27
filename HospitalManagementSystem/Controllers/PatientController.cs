using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PatientController(
            ApplicationDbContext context,
            IWebHostEnvironment hostEnvironment,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "Admin, Doctor, Staff")]
        // 🟢 Display Patients
        public async Task<IActionResult> Index()
        {
            // Get the logged-in user's information
            var currentUser = await _userManager.GetUserAsync(User);

            // If the user is a doctor, filter patients by their assigned doctor
            if (User.IsInRole("Doctor"))
            {
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.Email == currentUser.Email);  // Assuming email is used to identify the doctor

                var patients = await _context.Patients
                    .Where(p => p.AssignedDoctorId == doctor.DoctorId)  // Filter by the assigned doctor
                    .Include(p => p.AssignedDoctor)
                    .Include(p => p.Ward)
                    .ToListAsync();

                ViewData["ActivePage"] = "Patient";
                return View(patients);
            }
            else if (User.IsInRole("Admin") || (User.IsInRole("Staff")))
            {
                var patients = await _context.Patients
                    .Include(p => p.AssignedDoctor)
                    .Include(p => p.Ward)
                    .ToListAsync();

                ViewData["ActivePage"] = "Patient";
                return View(patients);
            }

            // If the user is not a doctor or admin, return an empty view or handle as needed
            return Unauthorized();
        }

        [Authorize(Roles = "Admin, Doctor, Staff")]
        public async Task<IActionResult> Create()
        {
            try
            {
                // Fetch available doctors and wards asynchronously
                var doctors = await _context.Doctors
                    .Where(d => d.IsAvailable)
                    .ToListAsync();

                var wards = await _context.Wards
                    .Where(w => w.CurrentOccupancy < w.Capacity)
                    .ToListAsync();

                // Null safety check
                ViewBag.Doctors = doctors ?? new List<Doctor>();
                ViewBag.Wards = wards ?? new List<Ward>();

                ViewData["ActivePage"] = "Patient";
                return View();
            }
            catch (Exception)
            {
                // Return an error view or redirect to the index with an error message
                TempData["ErrorMessage"] = "An error occurred while loading the form. Please try again.";
                ViewData["ActivePage"] = "Patient";
                return RedirectToAction("Index");
            }
        }

        // 🔥 Register Patient + Create User
        [HttpPost]
        [Authorize(Roles = "Admin, Doctor, Staff")]
        public async Task<IActionResult> Create(Patient patient, IFormFile? documentFile, string email, string password)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Doctors = await _context.Doctors.Where(d => d.IsAvailable).ToListAsync();
                ViewBag.Wards = await _context.Wards.Where(w => w.CurrentOccupancy < w.Capacity).ToListAsync();

                ViewData["ActivePage"] = "Patient";
                return View(patient);
            }

            // ✅ Upload Document if Provided
            if (documentFile != null && documentFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = $"{Guid.NewGuid()}_{documentFile.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await documentFile.CopyToAsync(fileStream);
                }

                patient.DocumentPath = "/uploads/" + uniqueFileName;
            }

            // ✅ Assign Doctor with Least Workload
            var doctor = _context.Doctors
                .Where(d => d.IsAvailable)
                .OrderBy(d => d.AssignedPatientsCount)
                .FirstOrDefault();

            if (doctor != null)
            {
                patient.AssignedDoctorId = doctor.DoctorId;
                doctor.AssignedPatientsCount++;
            }

            // ✅ Assign Ward & Bed Automatically
            var ward = _context.Wards.Include(w => w.Beds)
                .FirstOrDefault(w => w.CurrentOccupancy < w.Capacity && w.Beds.Any(b => !b.IsOccupied));

            if (ward != null)
            {
                var bed = ward.Beds.FirstOrDefault(b => !b.IsOccupied);
                if (bed != null)
                {
                    patient.WardId = ward.WardId;
                    patient.BedId = bed.BedId;
                    bed.IsOccupied = true;
                    ward.CurrentOccupancy++;
                }
            }

            patient.Status = "Admitted";

            // ✅ Create User Account
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = patient.FullName,
                PhoneNumber = patient.PhoneNumber,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // ✅ Assign "Patient" Role
                if (!await _roleManager.RoleExistsAsync("Patient"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Patient"));
                }
                await _userManager.AddToRoleAsync(user, "Patient");

                // ✅ Link Patient with User Account
                patient.UserId = user.Id;   // Link user with patient
                _context.Patients.Add(patient);

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Patient registered successfully with login access!";
                ViewData["ActivePage"] = "Patient";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                ViewBag.Doctors = await _context.Doctors.Where(d => d.IsAvailable).ToListAsync();
                ViewBag.Wards = await _context.Wards.Where(w => w.CurrentOccupancy < w.Capacity).ToListAsync();
                ViewData["ActivePage"] = "Patient";
                return View(patient);
            }
        }

        [Authorize(Roles = "Admin, Doctor, Staff")]
        public async Task<IActionResult> Discharge(int id)
        {
            var patient = await _context.Patients.Include(p => p.Bed).FirstOrDefaultAsync(p => p.PatientId == id);
            if (patient != null)
            {
                var bed = patient.Bed;
                if (bed != null)
                {
                    bed.IsOccupied = false;
                }

                patient.Status = "Discharged";
                patient.DischargeDate = DateTime.Now;
                patient.AssignedDoctorId = null;

                await _context.SaveChangesAsync();
            }
            ViewData["ActivePage"] = "Patient";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin, Doctor, Staff")]
        public IActionResult Transfer(int id)
        {
            var patient = _context.Patients.Find(id);

            if (patient == null)
            {
                return NotFound();
            }

            // Load wards with available beds
            ViewBag.Wards = _context.Wards
                .Include(w => w.Beds)
                .Where(w => w.Beds.Any(b => !b.IsOccupied))
                .ToList();

            ViewData["ActivePage"] = "Patient";
            return View(patient);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Doctor, Staff")]
        public async Task<IActionResult> Transfer(int id, int newWardId, int newBedId)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                var oldBed = _context.Beds.Find(patient.BedId);
                var newBed = _context.Beds.Find(newBedId);

                if (oldBed != null) oldBed.IsOccupied = false;
                if (newBed != null)
                {
                    patient.BedId = newBed.BedId;
                    newBed.IsOccupied = true;
                }

                patient.WardId = newWardId;
                patient.Status = "Transferred";
                await _context.SaveChangesAsync();
            }
            ViewData["ActivePage"] = "Patient";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> MedicalHistory()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var historyByPatient = await _context.TreatmentRecords
                .Include(tr => tr.Doctor)
                .Include(pt => pt.Patient)
                .Where(tr => tr.Patient.Email == currentUser.Email)
                .OrderByDescending(tr => tr.TreatmentDate)
                .ToListAsync();

            // Fetch the medicines for each treatment record
            var treatmentMedicines = await _context.TreatmentMedicines
                .Include(tm => tm.Medicine)
                .Where(tm => historyByPatient.Select(tr => tr.RecordId).Contains(tm.TreatmentId))
                .ToListAsync();

            // Add treatment medicines to each record in history
            foreach (var record in historyByPatient)
            {
                record.TreatmentMedicines = treatmentMedicines.Where(tm => tm.TreatmentId == record.RecordId).ToList();
            }

            ViewData["ActivePage"] = "MedicalHistory";
            return View(historyByPatient);
        }

    }
}
