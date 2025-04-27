using System.Numerics;
using System.Security.Claims;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DataController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DataController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/data/counts
        [HttpGet("counts")]
        public async Task<IActionResult> GetCounts()
        {
            var counts = new
            {
                teamCount = await _context.Teams.CountAsync(),
                staffCount = await _context.Staff.CountAsync(),
                doctorCount = await _context.Doctors.CountAsync(),
                patientCount = await _context.Patients.CountAsync(),
                billingCount = await _context.Billings.CountAsync(),
                appointmentsCount = await _context.Appointments.CountAsync(),
                medicinesCount = await _context.Medicines.CountAsync(),
                treatmentsCount = await _context.TreatmentRecords.CountAsync(),
                reportsCount = 10 // Static placeholder
            };

            return Ok(counts);
        }

        [HttpGet("patient-info")]
        public async Task<IActionResult> GetPatientInformation()
        {
            var counts = new
            {
                unAssignedPatients = await _context.Patients.CountAsync(x => x.AssignedDoctorId == null),
                assignedPatients = await _context.Patients.CountAsync(x => x.AssignedDoctorId != null),
                notAdmitted = await _context.Patients.CountAsync(x => x.WardId == null),
                admitted = await _context.Patients.CountAsync(x => x.WardId != null)
            };

            return Ok(counts);
        }

        [HttpGet("monthly-revenue")]
        public async Task<IActionResult> GetMonthlyRevenue()
        {
            var data = await _context.Billings
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(b => b.TotalAmount)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            // Format month as yyyy-MM in C#
            var formattedResult = data.Select(d => new
            {
                Month = $"{d.Year}-{d.Month.ToString().PadLeft(2, '0')}",
                d.Revenue
            });

            return Ok(formattedResult);
        }
        [HttpGet("appointments")]
        public IActionResult GetAppointmentsByDay()
        {
            var appointments = _context.Appointments
                .AsEnumerable() // Forces the rest of the query to run in memory
                .GroupBy(a => a.AppointmentDate.DayOfWeek)
                .Select(g => new
                {
                    Day = g.Key.ToString(), // Converts DayOfWeek enum to string (e.g., "Monday")
                    Count = g.Count()
                })
                .OrderBy(x => (int)System.Enum.Parse(typeof(DayOfWeek), x.Day)) // Ensures Mon-Sun order
                .ToList();

            // Convert to ChartJS-friendly format
            var chartData = new
            {
                labels = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" },
                datasets = new[]
                {
            new {
                label = "Appointments",
                data = appointments.OrderBy(x => (int)System.Enum.Parse(typeof(DayOfWeek), x.Day))
                                   .Select(x => x.Count)
                                   .ToArray(),
                backgroundColor = "#0d6efd"
            }
        }
            };

            return new JsonResult(chartData);
        }
        [HttpGet("counts-cards")]
        public async Task<IActionResult> GetDoctorDashboardCounts()
        {
            if (User.IsInRole("Doctor"))
            {
                var doctorEmail = User.Identity?.Name;
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == doctorEmail);
                if (doctor != null)
                {
                    var appointmentsToday = await _context.Appointments
                        .CountAsync(a => a.DoctorId == doctor.DoctorId && a.AppointmentDate.Date == DateTime.Today);

                    var myPatientsCount = await _context.Patients
                        .CountAsync(p => p.AssignedDoctorId == doctor.DoctorId);

                    var treatmentsCount = await _context.TreatmentRecords
                        .CountAsync(t => t.DoctorId == doctor.DoctorId);

                    var prescriptionsCount = await _context.TreatmentRecords
                        .Where(t => t.DoctorId == doctor.DoctorId)
                        .SelectMany(t => t.TreatmentMedicines)
                        .CountAsync();

                    return Ok(new
                    {
                        appointmentsToday,
                        myPatientsCount,
                        treatmentsCount,
                        prescriptionsCount
                    });
                }
            }
            else if (User.IsInRole("Admin"))
            {
                var appointmentsToday = await _context.Appointments
                    .CountAsync(a => a.AppointmentDate.Date == DateTime.Today);

                var myPatientsCount = await _context.Patients.CountAsync();

                var treatmentsCount = await _context.TreatmentRecords.CountAsync();

                var prescriptionsCount = await _context.TreatmentRecords
                    .SelectMany(t => t.TreatmentMedicines)
                    .CountAsync();

                return Ok(new
                {
                    appointmentsToday,
                    myPatientsCount,
                    treatmentsCount,
                    prescriptionsCount
                });
            }

            return Unauthorized();
        }

        [HttpGet("appointments-chart")]
        public async Task<IActionResult> GetDoctorAppointmentsChart()
        {
            IQueryable<Appointment> query;

            if (User.IsInRole("Doctor"))
            {
                var doctorEmail = User.Identity?.Name;
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == doctorEmail);
                if (doctor == null) return NotFound("Doctor not found");

                query = _context.Appointments.Where(a => a.DoctorId == doctor.DoctorId);
            }
            else if (User.IsInRole("Admin"))
            {
                query = _context.Appointments;
            }
            else
            {
                return Unauthorized();
            }

            // Materialize the data first to avoid issues with DateTime.ToString in LINQ-to-Entities
            var appointmentList = await query.ToListAsync();

            var grouped = appointmentList
                .GroupBy(a => a.AppointmentDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Count = g.Count()
                })
                .ToList();

            return Ok(new
            {
                labels = grouped.Select(a => a.Date).ToArray(),
                data = grouped.Select(a => a.Count).ToArray()
            });
        }

        [HttpGet("treatments-chart")]
        public async Task<IActionResult> GetDoctorTreatmentsChart()
        {
            IQueryable<TreatmentRecord> query;

            if (User.IsInRole("Doctor"))
            {
                var doctorEmail = User.Identity?.Name;
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == doctorEmail);
                if (doctor == null) return NotFound("Doctor not found");

                query = _context.TreatmentRecords.Where(t => t.DoctorId == doctor.DoctorId);
            }
            else if (User.IsInRole("Admin"))
            {
                query = _context.TreatmentRecords;
            }
            else
            {
                return Unauthorized();
            }

            // Materialize first, then group and format in memory
            var treatmentList = await query.ToListAsync();

            var grouped = treatmentList
                .GroupBy(t => t.TreatmentDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Count = g.Count()
                })
                .ToList();

            return Ok(new
            {
                labels = grouped.Select(t => t.Date).ToArray(),
                data = grouped.Select(t => t.Count).ToArray()
            });
        }
        [HttpGet("basic-info")]
        public async Task<IActionResult> GetStaffInfo()
        {
            var staffEmail = User.Identity?.Name;
            var staff = await _context.Staff.Include(s => s.AssignedWard).FirstOrDefaultAsync(s => s.Email == staffEmail);

            if (staff == null)
                return NotFound("Staff not found");

            return Ok(new
            {
                staff.FullName,
                staff.Email,
                staff.PhoneNumber,
                staff.Department,
                staff.AssignedWard?.WardName,
                staff.IsActive,
                staff.CreatedAt
            });
        }

        [HttpGet("ward-patients")]
        public async Task<IActionResult> GetWardPatients()
        {
            var staffEmail = User.Identity?.Name;
            var staff = await _context.Staff.FirstOrDefaultAsync(s => s.Email == staffEmail);
            if (staff == null)
            {
                return NotFound();
            }

            var patients = await _context.Patients
                                .Where(p => p.Ward != null && p.Ward.WardId == staff.AssignedWardId)
                                .Select(p => new
                                {
                                    p.PatientId,
                                    p.FullName,
                                    p.Status,
                                    p.AssignedDoctor,
                                    p.Gender,
                                    p.MedicalHistory
                                })
                                .ToListAsync();


            return Ok(patients);
        }

        [HttpGet("today-appointments")]
        public async Task<IActionResult> GetTodayAppointments()
        {
            var today = DateTime.Today;

            var appointments = await _context.Appointments
                .Where(a => a.AppointmentDate.Date == today)
                .Include(a => a.Doctor)
                .Select(a => new
                {
                    PatientName = a.Patient.FullName,
                    DoctorName = a.Doctor.FullName,
                    a.AppointmentDate,
                    a.StartTime,
                    a.EndTime,
                    a.Status
                })
                .ToListAsync();

            return Ok(appointments);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var patientCount = await _context.Patients.CountAsync();
            var todayAppointments = await _context.Appointments.CountAsync(a => a.AppointmentDate.Date == DateTime.Today);
            var activeStaff = await _context.Staff.CountAsync(s => s.IsActive);

            return Ok(new
            {
                patientCount,
                todayAppointments,
                activeStaff
            });
        }

        [HttpGet("GetPatientInfo")]
        public async Task<ActionResult<PatientDashboardViewModel>> GetPatientInfo(int? patientId = null)
        {
            var patientEmail = User.Identity?.Name;

            Patient? patient = null;

            if (User.IsInRole("Patient"))
            {
                patient = await _context.Patients
                    .Include(p => p.AssignedDoctor)
                    .Include(p => p.Ward)
                    .Include(p => p.Bed)
                    .FirstOrDefaultAsync(d => d.Email == patientEmail);
            }
            else if (User.IsInRole("Admin"))
            {
                if (patientId == null)
                    return BadRequest("PatientId is required for Admin.");

                patient = await _context.Patients
                    .Include(p => p.AssignedDoctor)
                    .Include(p => p.Ward)
                    .Include(p => p.Bed)
                    .FirstOrDefaultAsync(d => d.PatientId == patientId.Value);
            }

            if (patient == null)
                return NotFound();

            var model = new PatientDashboardViewModel
            {
                FullName = patient.FullName,
                DateOfBirth = patient.DateOfBirth.ToString("yyyy-MM-dd"),
                Gender = patient.Gender,
                PhoneNumber = patient.PhoneNumber,
                Email = patient.Email,
                Address = patient.Address,
                Status = patient.Status,
                CreatedAt = patient.CreatedAt.ToString("yyyy-MM-dd"),
                DischargeDate = patient.DischargeDate?.ToString("yyyy-MM-dd"),
                WardName = patient.Ward?.WardName,
                BedNumber = patient.Bed?.BedId ?? 0, // ✅ Safe check
                AssignedDoctorName = patient.AssignedDoctor?.FullName, // ✅ Safe check
                MedicalHistory = patient.MedicalHistory
            };

            return Ok(model);
        }


        [HttpGet("GetMedicalHistory")]
        public async Task<IActionResult> GetMedicalHistory()
        {
            var patientEmail = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(patientEmail))
                return Unauthorized();

            var patient = await _context.Patients.FirstOrDefaultAsync(d => d.Email == patientEmail);
            if (patient == null)
                return NotFound();

            var histories = await _context.TreatmentRecords
                .Where(t => t.PatientId == patient.PatientId)
                .Select(t => $"{t.TreatmentDate.ToShortDateString()} - {t.Diagnosis}")
                .ToListAsync();

            return Ok(histories);
        }

        [HttpGet("GetBillingInfo")]
        public async Task<IActionResult> GetBillingInfo()
        {
            var patientEmail = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(patientEmail))
                return Unauthorized();

            var patient = await _context.Patients.FirstOrDefaultAsync(d => d.Email == patientEmail);
            if (patient == null)
                return NotFound();

            var totalBills = await _context.Billings
                .Where(b => b.PatientId == patient.PatientId)
                .CountAsync();

            return Ok(new { totalBills });
        }

        [HttpGet("GetAppointments")]
        public async Task<IActionResult> GetAppointments()
        {
            var patientEmail = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(patientEmail))
                return Unauthorized();

            var patient = await _context.Patients.FirstOrDefaultAsync(d => d.Email == patientEmail);
            if (patient == null)
                return NotFound();

            var totalAppointments = await _context.Appointments
                .Where(a => a.PatientId == patient.PatientId)
                .CountAsync();

            return Ok(new { totalAppointments });
        }
    }
}
