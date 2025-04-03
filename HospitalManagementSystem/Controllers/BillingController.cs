//using HospitalManagementSystem.Data;
//using HospitalManagementSystem.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Linq;
//using System.Threading.Tasks;

//namespace HospitalManagementSystem.Controllers
//{
//    public class BillingController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public BillingController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // ✅ Display all bills
//        public async Task<IActionResult> Index()
//        {
//            var bills = await _context.Billings
//                .Include(b => b.Patient)
//                .Include(b => b.Doctor)
//                .ToListAsync();

//            return View(bills);
//        }

//        // ✅ Display bill details
//        public async Task<IActionResult> Details(int id)
//        {
//            var bill = await _context.Billings
//                .Include(b => b.Patient)
//                .Include(b => b.Doctor)
//                .Include(b => b.TreatmentMedicines)
//                .ThenInclude(tm => tm.Medicine)
//                .FirstOrDefaultAsync(b => b.BillId == id);

//            if (bill == null)
//            {
//                return NotFound();
//            }

//            return View(bill);
//        }

//        // ✅ Create a new bill
//        [HttpGet]
//        public async Task<IActionResult> Create(int patientId, int doctorId, double hours)
//        {
//            var patient = await _context.Patients.FindAsync(patientId);
//            var doctor = await _context.Doctors.FindAsync(doctorId);

//            if (patient == null || doctor == null)
//            {
//                return NotFound();
//            }

//            var bill = new Billing
//            {
//                PatientId = patientId,
//                DoctorId = doctorId,
//                TotalAmount = (decimal)(hours * doctor.HourlyRate),
//                IsPaid = false
//            };

//            return View(bill);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Create(Billing bill)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Billings.Add(bill);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }

//            return View(bill);
//        }

//        // ✅ Mark bill as paid
//        [HttpPost]
//        public async Task<IActionResult> MarkAsPaid(int id)
//        {
//            var bill = await _context.Billings.FindAsync(id);

//            if (bill == null)
//            {
//                return NotFound();
//            }

//            bill.IsPaid = true;
//            await _context.SaveChangesAsync();

//            return RedirectToAction(nameof(Index));
//        }
//    }
//}
