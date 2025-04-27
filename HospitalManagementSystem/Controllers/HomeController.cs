using HospitalManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["ActivePage"] = "Dashboard";

            if (User.IsInRole("Admin"))
            {
                return View("AdminDashboard");
            }
            else if (User.IsInRole("Staff"))
            {
                return View("StaffDashboard");
            }
            else if (User.IsInRole("Doctor"))
            {
                return View("DoctorDashboard");
            }
            else if (User.IsInRole("Patient"))
            {
                return View("PatientDashboard");
            }

            return RedirectToAction("Login", "Account");
        }

        public IActionResult DoctorDashboard()
        {
            ViewData["ActivePage"] = "Dashboard";
            return View();
        }
        public IActionResult StaffDashboard()
        {
            ViewData["ActivePage"] = "Dashboard";
            return View();
        }
        public IActionResult PatientDashboard()
        {
            ViewData["ActivePage"] = "Dashboard";
            return View();
        }
    }
}
