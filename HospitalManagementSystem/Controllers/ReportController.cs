using HospitalManagementSystem.Data;
using HospitalManagementSystem.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Patients by Ward Report
        public async Task<IActionResult> PatientsByWard()
        {
            var wards = await _context.Wards
                .Include(w => w.Patients)
                .ToListAsync();

            ViewData["ActivePage"] = "MyReports";
            ViewData["ActivePage"] = "PatientsByWard";
            return View(wards);
        }

        // Patients by Doctor Report
        public async Task<IActionResult> PatientsByDoctor()
        {
            var doctors = await _context.Doctors
                .Include(d => d.Patients)
                .ToListAsync();

            ViewData["ActivePage"] = "MyReports";
            ViewData["ActivePage"] = "PatientsByDoctor";
            return View(doctors);
        }

        // ✅ Treatment History Report
        public async Task<IActionResult> TreatmentHistory()
        {
            var records = await _context.TreatmentRecords
                .Include(tr => tr.Patient)
                .Include(tr => tr.Doctor)
                .OrderByDescending(tr => tr.TreatmentDate)
                .ToListAsync();

            ViewData["ActivePage"] = "MyReports";
            ViewData["ActivePage"] = "TreatmentHistory";
            return View(records);
        }

        // ✅ Team Reports
        public async Task<IActionResult> TeamReports()
        {
            var teams = await _context.Teams
                .Include(t => t.Doctors)
                .ToListAsync();

            ViewData["ActivePage"] = "MyReports";
            ViewData["ActivePage"] = "TeamReports";
            return View(teams);
        }

        // ✅ Ward Occupancy Report
        public async Task<IActionResult> WardOccupancy()
        {
            var wards = await _context.Wards
                .Include(w => w.Patients)
                .Select(w => new WardOccupancyViewModel
                {
                    WardName = w.WardName,
                    TotalBeds = w.Capacity,
                    OccupiedBeds = w.CurrentOccupancy,
                    AvailableBeds = w.AvailableBeds
                })
                .ToListAsync();

            ViewData["ActivePage"] = "WardOccupancy";
            return View(wards);
        }

        // ✅ Patient Admission and Discharge Trends Report
        //public async Task<IActionResult> AdmissionTrends()
        //{
        //    var admissions = await _context.Patients
        //        .GroupBy(p => p.AdmissionDate.Date)
        //        .Select(g => new
        //        {
        //            Date = g.Key,
        //            TotalAdmissions = g.Count()
        //        })
        //        .OrderBy(a => a.Date)
        //        .ToListAsync();

        //    return View(admissions);
        //}

        // ✅ Discharge Reports
        public async Task<IActionResult> DischargeReports()
        {
            var discharges = await _context.Patients
                .Where(p => p.DischargeDate != null)
                .Select(p => new
                {
                    PatientName = p.FullName,
                    Ward = p.WardId,
                    Doctor = p.AssignedDoctor.FullName,
                    DischargeDate = p.DischargeDate
                })
                .OrderByDescending(p => p.DischargeDate)
                .ToListAsync();

            ViewData["ActivePage"] = "MyReports";
            ViewData["ActivePage"] = "DischargeReports";
            return View(discharges);
        }

        // ✅ Appointment Reports
        public async Task<IActionResult> AppointmentReports()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            ViewData["ActivePage"] = "MyReports";
            ViewData["ActivePage"] = "AppointmentReports";
            return View(appointments);
        }

        //// ✅ Financial Reports (Billing and Payments)
        //public async Task<IActionResult> FinancialReports()
        //{
        //    var payments = await _context.Payments
        //        .Include(p => p.Patient)
        //        .Include(p => p.Doctor)
        //        .OrderByDescending(p => p.PaymentDate)
        //        .ToListAsync();

        //    return View(payments);
        //}

        // ✅ Patient Statistics (Gender, Age, etc.)
        public async Task<IActionResult> PatientStatistics()
        {
            var stats = await _context.Patients
                .GroupBy(p => new { p.Gender })
                .Select(g => new
                {
                    g.Key.Gender,
                    Count = g.Count()
                })
                .ToListAsync();

            ViewData["ActivePage"] = "MyReports";
            ViewData["ActivePage"] = "PatientStatistics";
            return View(stats);
        }

        // ✅ Staff Activity Reports
        public async Task<IActionResult> StaffActivityReports()
        {
            var staffActivities = await _context.Staff
                .ToListAsync();

            ViewData["ActivePage"] = "MyReports";
            ViewData["ActivePage"] = "StaffActivityReports";
            return View(staffActivities);
        }

        // ✅ Emergency Consultation Requests
        public async Task<IActionResult> EmergencyConsultations()
        {
            var emergencyRequests = await _context.Appointments
                .Where(a => a.IsEmergency)
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ToListAsync();

            ViewData["ActivePage"] = "MyReports";
            ViewData["ActivePage"] = "EmergencyConsultations";
            return View(emergencyRequests);
        }
    }
}
