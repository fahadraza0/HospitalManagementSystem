using System.Security.Claims;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize]
    public class BillingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BillingController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles ="Admin, Staff")]
        public async Task<IActionResult> Index()
        {
            var bills = await _context.Billings
                .Include(b => b.Patient)
                .Include(b => b.Doctor)
                .Include(b => b.Appointment)
                .ToListAsync();

            ViewData["ActivePage"] = "Billing";
            return View(bills);
        }

        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> Details(int id)
        {
            var bill = await _context.Billings
                .Include(b => b.Patient)
                .Include(b => b.Doctor)
                .Include(b => b.Appointment)
                .Include(b => b.TreatmentMedicines)
                    .ThenInclude(tm => tm.Medicine)
                .FirstOrDefaultAsync(b => b.BillingId == id);

            if (bill == null) return NotFound();

            ViewData["ActivePage"] = "Billing";
            return View(bill);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Doctors = await _context.Doctors.ToListAsync();

            ViewData["ActivePage"] = "CreateBilling";
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> Create(Billing billing)
        {
            // Check if a paid bill already exists for the same patient and doctor
            bool billAlreadyPaid = await _context.Billings
                .AnyAsync(b => b.PatientId == billing.PatientId
                            && b.DoctorId == billing.DoctorId
                            && b.IsPaid == true);

            if (billAlreadyPaid)
            {
                TempData["ErrorMessage"] = "Bill has already been paid for this patient and doctor.";
                return RedirectToAction(nameof(Index));
            }

            // If no paid bill exists, proceed
            billing.CreatedAt = DateTime.Now;
            _context.Billings.Add(billing);
            await _context.SaveChangesAsync();

            var relatedTreatments = await _context.TreatmentRecords
                .Where(tr => tr.PatientId == billing.PatientId
                             && tr.DoctorId == billing.DoctorId
                             && tr.BillId == null)
                .ToListAsync();

            foreach (var treatment in relatedTreatments)
            {
                treatment.BillId = billing.BillingId;
                if (billing.IsPaid == true)
                {
                    treatment.IsFinalTreatment = true;
                }
            }

            await _context.SaveChangesAsync();

            ViewData["ActivePage"] = "Billing";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var bill = await _context.Billings.FindAsync(id);
            if (bill == null) return NotFound();

            bill.IsPaid = true;
            bill.UpdatedAt = DateTime.Now;

            var treatmentRecord = _context.TreatmentRecords.FirstOrDefault(x => x.BillId == id);
            if (treatmentRecord != null)
            {
                treatmentRecord.IsFinalTreatment = true;
            }

            await _context.SaveChangesAsync();

            ViewData["ActivePage"] = "Billing";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<JsonResult> GetPatientsByDoctor(int doctorId)
        {
            var patients = await _context.Patients
                .Where(p => p.AssignedDoctorId == doctorId)
                .Select(p => new { p.PatientId, p.FullName })
                .ToListAsync();

            return Json(patients);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<JsonResult> GetAppointmentsByPatientAndDoctor(int patientId, int doctorId)
        {
            var appointments = await _context.Appointments
                .Where(a => a.PatientId == patientId && a.DoctorId == doctorId)
                .Select(a => new
                {
                    a.AppointmentId,
                    TimeRange = a.AppointmentDate.ToString("yyyy-MM-dd") + " (" + a.StartTime.ToString(@"hh\:mm") + " - " + a.EndTime.ToString(@"hh\:mm") + ")"
                })
                .ToListAsync();

            return Json(appointments);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<JsonResult> GetBillingDetails(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
            {
                return Json(new { error = "Appointment not found." });
            }

            var patientId = appointment.PatientId;
            var doctorId = appointment.DoctorId;
            var duration = appointment.EndTime - appointment.StartTime;
            double totalHours = duration.TotalHours;

            decimal doctorFee = 0;
            decimal medicineCost = 0;

            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor != null)
            {
                doctorFee = (decimal)totalHours * doctor.HourlyRate;
            }

            var treatments = await _context.TreatmentRecords
                .Where(t => t.PatientId == patientId && t.DoctorId == doctorId)
                .Include(t => t.TreatmentMedicines)
                    .ThenInclude(tm => tm.Medicine)
                .ToListAsync();

            foreach (var treatment in treatments)
            {
                foreach (var tm in treatment.TreatmentMedicines)
                {
                    medicineCost += tm.TotalCost;
                }
            }

            return Json(new
            {
                doctorFee = doctorFee.ToString("0.00"),
                medicineCost = medicineCost.ToString("0.00"),
                totalAmount = (doctorFee + medicineCost).ToString("0.00"),
                patientId = patientId,
                doctorId = doctorId
            });
        }
        [Authorize(Roles = "Admin, Patient")]
        public async Task<IActionResult> MyBills()
        {
            var patientEmail = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(patientEmail))
                return Unauthorized();

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Email == patientEmail);
            if (patient == null)
                return NotFound();

            var myBills = await _context.Billings
                .Where(b => b.PatientId == patient.PatientId)
                .Include(b => b.Doctor)
                .Include(b => b.Appointment)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            ViewData["ActivePage"] = "MyBills";
            return View(myBills);
        }

        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> MyPatientsBills()
        {
            var doctorEmail = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(doctorEmail))
                return Unauthorized();

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == doctorEmail);
            if (doctor == null)
                return NotFound();

            var myPatientsBills = await _context.Billings
                .Where(b => b.DoctorId == doctor.DoctorId)
                .Include(b => b.Patient)
                .Include(b => b.Appointment)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            ViewData["ActivePage"] = "MyPatientsBills";
            return View(myPatientsBills);
        }
    }
}
