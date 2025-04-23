using HospitalManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
    }
}
