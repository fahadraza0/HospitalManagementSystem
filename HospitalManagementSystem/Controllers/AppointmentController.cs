using System.Security.Claims;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Display the appointment booking page
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Book()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return NotFound("Patient profile not found.");
            }

            ViewBag.PatientId = patient.PatientId;
            ViewBag.Doctors = await _context.Doctors.Where(d => d.IsAvailable).ToListAsync();
            ViewData["ActivePage"] = "Appointment";
            return View();
        }

        // ✅ Post Appointment Booking
        [HttpPost]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Book(Appointment appointment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            if (patient == null) return NotFound("Patient not found.");

            appointment.PatientId = patient.PatientId;

            var doctor = await _context.Doctors.FindAsync(appointment.DoctorId);
            if (doctor == null || !doctor.IsAvailable)
            {
                ModelState.AddModelError("", "Selected doctor is not available.");
                ViewBag.Doctors = await _context.Doctors.Where(d => d.IsAvailable).ToListAsync();

                ViewData["ActivePage"] = "Appointment";
                return View(appointment);
            }

            // Validate time slot
            var startTime = appointment.StartTime;
            var endTime = appointment.EndTime;

            var startTimeLimit = new TimeSpan(9, 0, 0);   // 9 AM
            var endTimeLimit = new TimeSpan(17, 0, 0);    // 5 PM

            if (startTime < startTimeLimit || endTime > endTimeLimit)
            {
                ModelState.AddModelError("", "Appointments must be between 9 AM and 5 PM.");
                ViewBag.Doctors = await _context.Doctors.Where(d => d.IsAvailable).ToListAsync();
                return View(appointment);
            }

            // Prevent overlapping appointments
            bool isOverlapping = await _context.Appointments
                .AnyAsync(a =>
                    a.DoctorId == appointment.DoctorId &&
                    a.AppointmentDate.Date == appointment.AppointmentDate.Date &&
                    a.StartTime < endTime &&
                    a.EndTime > startTime);

            if (isOverlapping)
            {
                ModelState.AddModelError("", "This time slot is already booked. Please choose another slot.");
                ViewBag.Doctors = await _context.Doctors.Where(d => d.IsAvailable).ToListAsync();
                return View(appointment);
            }

            // Set appointment status
            appointment.Status = appointment.IsEmergency ? "Confirmed" : "Pending";
            appointment.CreatedAt = DateTime.Now;

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // ✅ Generate bill after booking if confirmed
            //if (appointment.Status == "Confirmed")
            //{
            //    await GenerateBill(appointment);
            //}

            TempData["SuccessMessage"] = "Appointment booked successfully!";

            ViewData["ActivePage"] = "Appointment";
            return RedirectToAction("Index", "Appointment");
        }

        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> MyAppointments()
        {
            var currentUser = User.Identity.Name;

            var patientId = await _context.Patients
                    .Where(p => p.Email == currentUser)
                    .Select(p => p.PatientId)
                    .FirstOrDefaultAsync();

            var appointments = await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ThenInclude(d => d.Teams)
                .ToListAsync();

            ViewData["ActivePage"] = "Appointment";
            return View(appointments);
        }

        [Authorize(Roles = "Admin, Doctor, Staff")]
        public async Task<IActionResult> Index()
        {
            var currentUser = User.Identity.Name;

            // Check if the user is an Admin or Staff
            if (User.IsInRole("Admin") || User.IsInRole("Staff"))
            {
                var appointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .ThenInclude(d => d.Teams)
                    .ToListAsync();

                ViewData["ActivePage"] = "Appointment";
                return View(appointments);
            }
            // Check if the user is a Doctor
            else if (User.IsInRole("Doctor"))
            {
                var doctorId = await _context.Doctors
                    .Where(d => d.Email == currentUser) // Assuming doctor is identified by their username
                    .Select(d => d.DoctorId)
                    .FirstOrDefaultAsync();

                var appointments = await _context.Appointments
                    .Where(a => a.DoctorId == doctorId)
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .ThenInclude(d => d.Teams)
                    .ToListAsync();

                ViewData["ActivePage"] = "Appointment";
                return View(appointments);
            }
            // Check if the user is a Patient
            else if (User.IsInRole("Patient"))
            {
                var patientId = await _context.Patients
                    .Where(p => p.Email == currentUser)
                    .Select(p => p.PatientId)
                    .FirstOrDefaultAsync();

                var appointments = await _context.Appointments
                    .Where(a => a.PatientId == patientId)
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .ThenInclude(d => d.Teams)
                    .ToListAsync();

                ViewData["ActivePage"] = "Appointment";
                return View(appointments);
            }

            // Fallback if user role is not recognized
            return Unauthorized();
        }

        // ✅ Display Create Appointment Form
        [Authorize(Roles = "Admin, Doctor, Staff")]
        public IActionResult Create()
        {
            ViewBag.Doctors = _context.Doctors.Where(d => d.IsAvailable).ToList();
            ViewBag.Patients = _context.Patients.ToList();

            ViewData["ActivePage"] = "Appointment";
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Doctor, Staff")]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            var doctor = await _context.Doctors.FindAsync(appointment.DoctorId);

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == appointment.PatientId);

            if (doctor == null || !doctor.IsAvailable)
            {
                ModelState.AddModelError("", "Selected doctor is not available.");
                ViewBag.Doctors = _context.Doctors.Where(d => d.IsAvailable).ToList();
                ViewBag.Patients = _context.Patients.ToList();
                return View(appointment);
            }

           var startTime = appointment.StartTime;  // Parse the start time
            var endTime = appointment.EndTime;      // Parse the end time

            var appointmentStartTime = startTime;
            var appointmentEndTime = endTime;

            var startTimeLimit = new TimeSpan(9, 0, 0);   // 9 AM
            var endTimeLimit = new TimeSpan(17, 0, 0);    // 5 PM

            if (appointmentStartTime < startTimeLimit || appointmentEndTime > endTimeLimit)
            {
                ModelState.AddModelError("", "Appointments must be between 9 AM and 5 PM.");
                ViewBag.Doctors = _context.Doctors.Where(d => d.IsAvailable).ToList();
                ViewBag.Patients = _context.Patients.ToList();

                ViewData["ActivePage"] = "Appointment";
                return View(appointment);
            }

            // Check for overlapping appointments with the same doctor
            bool isOverlapping = await _context.Appointments
                .AnyAsync(a =>
                    a.DoctorId == appointment.DoctorId &&
                    a.AppointmentDate.Date == appointment.AppointmentDate.Date &&
                    a.StartTime < appointmentEndTime &&
                    a.EndTime > appointmentStartTime);

            if (isOverlapping)
            {
                ModelState.AddModelError("", "This time slot is already booked. Please choose another slot.");
                ViewBag.Doctors = _context.Doctors.Where(d => d.IsAvailable).ToList();
                ViewBag.Patients = _context.Patients.ToList();

                ViewData["ActivePage"] = "Appointment";
                return View(appointment);
            }

            appointment.Status = appointment.IsEmergency ? "Confirmed" : "Pending";
            appointment.CreatedAt = DateTime.Now;

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // ✅ Generate bill after booking if confirmed
            //if (appointment.Status == "Confirmed")
            //{
            //    await GenerateBill(appointment);
            //}

            TempData["SuccessMessage"] = "Appointment booked successfully!";
            ViewData["ActivePage"] = "Appointment";
            return RedirectToAction("Index", "Appointment");
        }

        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> Confirm(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = "Confirmed";
                await _context.SaveChangesAsync();
            }
            //await GenerateBill(appointment);

            TempData["SuccessMessage"] = "Appointment confirmed!";
            ViewData["ActivePage"] = "Appointment";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = "Cancelled";
                await _context.SaveChangesAsync();
            }
            TempData["SuccessMessage"] = "Appointment cancelled!";
            ViewData["ActivePage"] = "Appointment";
            return RedirectToAction(nameof(Index));
        }

        // ✅ Generate Bill Logic
        private async Task GenerateBill(Appointment appointment)
        {
            var doctor = await _context.Doctors.FindAsync(appointment.DoctorId);
            if (doctor != null)
            {
                // Calculate the hours based on appointment start and end times
                var hours = (appointment.EndTime - appointment.StartTime).TotalHours;

                var appointmentStartDateTime = appointment.AppointmentDate.Add(appointment.StartTime);
                var appointmentEndDateTime = appointment.AppointmentDate.Add(appointment.EndTime);

                var treatmentRecords = await _context.TreatmentRecords
                    .Where(tr => tr.PatientId == appointment.PatientId &&
                                 tr.TreatmentDate >= appointment.AppointmentDate)
                    .Include(tr => tr.TreatmentMedicines)  // Include the prescribed medicines
                    .ToListAsync();

                // Calculate the total medicine cost by summing the cost of all medicines in the treatment records
                var medicineCost = treatmentRecords
                    .SelectMany(tr => tr.TreatmentMedicines)
                    .Sum(tm => tm.TotalCost);

                // Create the bill for the appointment
                var bill = new Billing
                {
                    AppointmentId = appointment.AppointmentId,
                    PatientId = appointment.PatientId,
                    DoctorId = doctor.DoctorId,
                    DoctorFee = (decimal)hours * doctor.HourlyRate,
                    MedicineCost = medicineCost,
                    TotalAmount = (decimal)hours * doctor.HourlyRate + medicineCost,
                    CreatedAt = DateTime.Now,
                    IsPaid = false // Initial state, can be updated once payment is processed
                };

                // Add the new bill to the database
                _context.Billings.Add(bill);
                await _context.SaveChangesAsync();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, DateTime date)
        {
            var slots = await GenerateAvailableTimeSlots(doctorId, date);
            return Json(slots);
        }

        // ✅ Slot generation logic
        private async Task<List<string>> GenerateAvailableTimeSlots(int doctorId, DateTime date)
        {
            var appointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date)
                .ToListAsync();

            var slots = new List<string>();
            var startTime = new TimeSpan(9, 0, 0);  // 9:00 AM
            var endTime = new TimeSpan(17, 0, 0);   // 5:00 PM

            while (startTime < endTime)
            {
                var slotEnd = startTime.Add(TimeSpan.FromHours(1));

                // Check if the slot is booked
                bool isBooked = appointments.Any(a =>
                    a.AppointmentDate.TimeOfDay < slotEnd &&
                    a.AppointmentDate.TimeOfDay >= startTime
                );

                if (!isBooked)
                {
                    slots.Add($"{startTime:hh\\:mm} - {slotEnd:hh\\:mm}");
                }

                startTime = slotEnd;
            }

            return slots;
        }
    }
}
