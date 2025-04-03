using HospitalManagementSystem.Data;
using HospitalManagementSystem.DTO;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DoctorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Index()
        {
            var doctors = await _context.Doctors.Include(d => d.Team).ToListAsync();
            return View(doctors);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var teams = _context.Teams.ToList();
            ViewBag.Teams = teams;

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(DoctorCreateDTO doctorDTO)
        {
            if (ModelState.IsValid)
            {
                // Create a Doctor object and map from DTO
                var doctor = new Doctor
                {
                    FullName = doctorDTO.FullName,
                    Specialization = doctorDTO.Specialization,
                    Email = doctorDTO.Email,
                    PhoneNumber = doctorDTO.PhoneNumber,  // New field
                    TeamId = doctorDTO.TeamId,
                    HourlyRate = doctorDTO.HourlyRate,  // New field
                    Experience = doctorDTO.Experience,  // New field
                    IsAvailable = true, // Default value
                    AssignedPatientsCount = 0, // Default value
                    IsActive = true, // Default value
                    CreatedAt = DateTime.Now
                };

                // Add doctor to the database
                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                // Create the doctor user in the system
                string doctorEmail = doctorDTO.Email;
                await CreateUserIfNotExists(doctorEmail, doctorDTO.Password, "Doctor");

                return RedirectToAction(nameof(Index));
            }
            return View(doctorDTO);
        }


        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> AssignPatient(int id)
        {
            // Retrieve the doctor along with their patients
            var doctor = await _context.Doctors
                                       .Include(d => d.Patients)
                                       .FirstOrDefaultAsync(d => d.DoctorId == id);

            // Fetch patients who don't have an assigned doctor
            var unassignedPatients = await _context.Patients
                                                   .Where(p => p.AssignedDoctorId == null)
                                                   .ToListAsync();

            // Pass the list of unassigned patients to the View
            ViewBag.Patients = unassignedPatients;

            return View(doctor);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> AssignPatient(int doctorId, int patientId)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            var patient = await _context.Patients.FindAsync(patientId);

            if (doctor != null && patient != null)
            {
                patient.AssignedDoctorId = doctor.DoctorId;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Doctor")]
        public IActionResult Schedule(int id)
        {
            var doctor = _context.Doctors.Include(d => d.Schedules).FirstOrDefault(d => d.DoctorId == id);
            return View(doctor);
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Schedule(int doctorId, DateTime startTime, DateTime endTime)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);

            if (doctor != null)
            {
                _context.DoctorSchedules.Add(new DoctorSchedule
                {
                    DoctorId = doctorId,
                    StartTime = startTime,
                    EndTime = endTime
                });

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet]
        public IActionResult GetSchedules()
        {
            var schedules = _context.DoctorSchedules.Select(s => new
            {
                title = "Available",
                start = s.StartTime,
                end = s.EndTime,
                color = s.IsBooked ? "red" : "green"
            }).ToList();

            return Json(schedules);
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet]
        public IActionResult GetAvailableSlots(int doctorId)
        {
            var availableSlots = _context.DoctorSchedules
                .Where(s => s.DoctorId == doctorId && !s.IsBooked)
                .Select(s => new { s.StartTime, s.EndTime })
                .ToList();

            return Json(availableSlots);
        }

        //[Authorize(Roles = "Admin,Doctor")]
        //public IActionResult GenerateBill(int patientId, int doctorId, int appointmentId)
        //{
        //    var patient = _context.Patients.FirstOrDefault(p => p.PatientId == patientId);
        //    var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == doctorId);
        //    var appointment = _context.Appointments.FirstOrDefault(a => a.AppointmentId == appointmentId);

        //    if (patient == null || doctor == null || appointment == null)
        //    {
        //        return NotFound();
        //    }

        //    var bill = new Billing
        //    {
        //        PatientId = patient.PatientId,
        //        DoctorId = doctor.DoctorId,
        //        AppointmentId = appointment.AppointmentId,
        //        CreatedAt = DateTime.Now,
        //        DoctorFee = doctor.HourlyRate * (decimal)(appointment.EndTime - appointment.StartTime).TotalHours, // Assuming DoctorFee is hourly
        //        MedicineCost = 0, // Set to a default value or calculate based on prescribed medicines
        //        IsPaid = false
        //    };

        //    _context.Billings.Add(bill);
        //    _context.SaveChanges();

        //    return RedirectToAction("BillDetails", new { id = bill.BillId });
        //}

        // View for displaying the Bill Details
        //[Authorize(Roles = "Admin,Doctor")]
        //public IActionResult BillDetails(int id)
        //{
        //    var bill = _context.Billings
        //                       .Include(b => b.Patient)
        //                       .Include(b => b.Doctor)
        //                       .Include(b => b.Appointment)
        //                       .FirstOrDefault(b => b.BillId == id);

        //    if (bill == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(bill);
        //}

        //// Update Payment Status of a Bill
        //[HttpPost]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> UpdatePaymentStatus(int billId, bool isPaid)
        //{
        //    var bill = await _context.Billings.FindAsync(billId);
        //    if (bill != null)
        //    {
        //        bill.IsPaid = isPaid;
        //        bill.UpdatedAt = DateTime.Now;
        //        await _context.SaveChangesAsync();
        //    }
        //    return RedirectToAction("BillDetails", new { id = billId });
        //}

        #region Private Methods
        [Authorize(Roles = "Admin")]
        private async Task CreateUserIfNotExists(string email, string password, string role)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser { UserName = email, Email = email };
                await _userManager.CreateAsync(user, password);
            }

            if (!await _userManager.IsInRoleAsync(user, role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }
        }
        #endregion

    }
}
